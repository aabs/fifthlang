using System.Collections.Generic;
using System.Linq;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// Validation visitor for SPARQL comprehensions.
/// 
/// Validates:
/// 1. Generator type is list or tabular SELECT result
/// 2. For SPARQL generators: query is SELECT (not ASK/CONSTRUCT/DESCRIBE)
/// 3. For SPARQL object projections: property values are SPARQL variables (?varName)
/// 4. For SPARQL object projections: referenced variables exist in SELECT projection
/// 5. Constraints are boolean expressions
/// 
/// Emits diagnostic codes LCOMP001-006 for validation failures.
/// </summary>
public class SparqlComprehensionValidationVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<compiler.Diagnostic> compilerDiagnostics;
    
    public IReadOnlyList<Diagnostic> Diagnostics { get; private set; } = new List<Diagnostic>();
    
    public SparqlComprehensionValidationVisitor(List<compiler.Diagnostic>? compilerDiagnostics = null)
    {
        this.compilerDiagnostics = compilerDiagnostics ?? new List<compiler.Diagnostic>();
    }
    
    public override ListComprehension VisitListComprehension(ListComprehension ctx)
    {
        // Visit children first to ensure types are inferred
        var result = base.VisitListComprehension(ctx);
        
        var localDiagnostics = new List<Diagnostic>();
        
        // Validate generator type (must be list or tabular SELECT result)
        ValidateGeneratorType(result, localDiagnostics);
        
        // If generator is SPARQL literal, validate SELECT form and object projection
        if (result.Source is SparqlLiteralExpression sparqlSource)
        {
            ValidateSparqlComprehension(result, sparqlSource, localDiagnostics);
        }
        
        // Validate constraints are boolean
        ValidateConstraints(result, localDiagnostics);
        
        // Convert local diagnostics to compiler diagnostics
        foreach (var diag in localDiagnostics)
        {
            var compilerDiag = new compiler.Diagnostic(
                diag.Severity == DiagnosticSeverity.Error ? compiler.DiagnosticLevel.Error : compiler.DiagnosticLevel.Warning,
                diag.Message,
                diag.Filename,
                diag.Code);
            compilerDiagnostics.Add(compilerDiag);
        }
        
        Diagnostics = localDiagnostics.AsReadOnly();
        
        return result;
    }
    
    /// <summary>
    /// Validates that the generator (source) expression has a compatible type.
    /// For general comprehensions: must be a list type.
    /// For SPARQL comprehensions: type will be Result (tabular SELECT result).
    /// </summary>
    private void ValidateGeneratorType(ListComprehension ctx, List<Diagnostic> diagnostics)
    {
        if (ctx.Source.Type == null)
        {
            // Type not yet inferred - skip validation (will be caught by type inference pass)
            return;
        }
        
        var sourceType = ctx.Source.Type;
        
        // Check if it's a list type
        bool isListType = sourceType switch
        {
            FifthType.TType t => t.Name.ToString().StartsWith("List<") || 
                                  t.Name.ToString() == "List" ||
                                  t.Name.ToString() == "Result", // Tabular SELECT result
            FifthType.TGenericType gt => gt.Generic.ToString().StartsWith("List") ||
                                           gt.Generic.ToString() == "Result",
            _ => false
        };
        
        if (!isListType)
        {
            EmitDiagnostic(
                compiler.ComprehensionDiagnostics.InvalidGeneratorType,
                compiler.ComprehensionDiagnostics.FormatInvalidGeneratorType(sourceType.ToString()),
                DiagnosticSeverity.Error,
                ctx.Source,
                diagnostics);
        }
    }
    
    /// <summary>
    /// Validates SPARQL-specific comprehension rules.
    /// </summary>
    private void ValidateSparqlComprehension(ListComprehension ctx, SparqlLiteralExpression sparqlSource, List<Diagnostic> diagnostics)
    {
        // Introspect the SPARQL query to get form and projected variables
        var introspection = compiler.LanguageTransformations.SparqlSelectIntrospection.IntrospectQuery(sparqlSource.QueryText);
        
        if (!introspection.Success)
        {
            // Query parsing failed - emit generic error
            // Note: This might be a runtime-constructed query, so we're lenient here
            return;
        }
        
        // Validate query form is SELECT
        if (introspection.QueryForm != "SELECT")
        {
            EmitDiagnostic(
                compiler.ComprehensionDiagnostics.NonSelectQuery,
                compiler.ComprehensionDiagnostics.FormatNonSelectQuery(introspection.QueryForm ?? "unknown"),
                DiagnosticSeverity.Error,
                sparqlSource,
                diagnostics);
            return;
        }
        
        // If projection is object instantiation, validate SPARQL variable bindings
        if (ctx.Projection is ObjectInitializerExp objProj)
        {
            ValidateSparqlObjectProjection(objProj, introspection, ctx, diagnostics);
        }
    }
    
    /// <summary>
    /// Validates SPARQL object projection:
    /// - Property values must be SPARQL variable references (?varName)
    /// - Referenced variables must exist in SELECT projection
    /// </summary>
    private void ValidateSparqlObjectProjection(
        ObjectInitializerExp objProj, 
        compiler.LanguageTransformations.SparqlSelectIntrospection.IntrospectionResult introspection,
        ListComprehension ctx,
        List<Diagnostic> diagnostics)
    {
        if (objProj.Fields == null)
        {
            return;
        }
        
        foreach (var field in objProj.Fields)
        {
            if (field.Value == null)
            {
                continue;
            }
            
            // Check if initializer is a SPARQL variable reference
            // In the AST, SPARQL vars appear as VarRefExp with name starting with ?
            if (field.Value is VarRefExp varRef)
            {
                var varName = varRef.Name;
                
                // Check if it's a SPARQL variable (starts with ?)
                if (varName.StartsWith("?"))
                {
                    // Remove ? prefix for comparison
                    var cleanVarName = varName.TrimStart('?');
                    
                    // Validate variable exists in SELECT projection
                    if (!compiler.LanguageTransformations.SparqlSelectIntrospection.HasProjectedVariable(introspection, cleanVarName))
                    {
                        var availableVars = introspection.IsSelectStar 
                            ? "*" 
                            : string.Join(", ", introspection.ProjectedVariables.Select(v => "?" + v));
                        
                        EmitDiagnostic(
                            compiler.ComprehensionDiagnostics.UnknownSparqlVariable,
                            compiler.ComprehensionDiagnostics.FormatUnknownSparqlVariable(varName, availableVars),
                            DiagnosticSeverity.Error,
                            varRef,
                            diagnostics);
                    }
                }
            }
            else
            {
                // For SPARQL object projections, we require direct SPARQL variable bindings
                // (not arbitrary expressions, literals, or function calls)
                EmitDiagnostic(
                    compiler.ComprehensionDiagnostics.InvalidObjectProjectionBinding,
                    compiler.ComprehensionDiagnostics.FormatInvalidObjectProjectionBinding(),
                    DiagnosticSeverity.Error,
                    field.Value,
                    diagnostics);
            }
        }
    }
    
    /// <summary>
    /// Validates that all where constraints are boolean expressions.
    /// </summary>
    private void ValidateConstraints(ListComprehension ctx, List<Diagnostic> diagnostics)
    {
        if (ctx.Constraints == null || ctx.Constraints.Count == 0)
        {
            return;
        }
        
        foreach (var constraint in ctx.Constraints)
        {
            if (constraint.Type == null)
            {
                // Type not yet inferred - skip validation
                continue;
            }
            
            // Check if constraint type is boolean
            bool isBooleanType = constraint.Type switch
            {
                FifthType.TType t => t.Name.ToString() == "bool" || t.Name.ToString() == "Boolean",
                _ => false
            };
            
            if (!isBooleanType)
            {
                EmitDiagnostic(
                    compiler.ComprehensionDiagnostics.NonBooleanConstraint,
                    compiler.ComprehensionDiagnostics.FormatNonBooleanConstraint(constraint.Type.ToString()),
                    DiagnosticSeverity.Error,
                    constraint,
                    diagnostics);
            }
        }
    }
    
    /// <summary>
    /// Emits a diagnostic message.
    /// </summary>
    private void EmitDiagnostic(string code, string message, DiagnosticSeverity severity, AstThing context, List<Diagnostic> diagnostics)
    {
        var diagnostic = new Diagnostic
        {
            Code = code,
            Message = message,
            Severity = severity,
            Filename = context.Location?.Filename ?? "",
            Line = context.Location?.Line ?? 0,
            Column = context.Location?.Column ?? 0
        };
        
        diagnostics.Add(diagnostic);
    }
}
