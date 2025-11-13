using ast;
using ast_generated;
using ast_model.Symbols;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// SPARQL variable binding visitor that resolves Fifth variable references within SPARQL literals.
/// This visitor scans the SPARQL text for identifiers that match in-scope Fifth variables
/// and populates the Bindings list for safe parameterization.
/// </summary>
/// <remarks>
/// This implements User Story 2: Variable Binding via Parameters.
/// Variables referenced in SPARQL text (e.g., "age" in "?s ex:age age") are bound as
/// typed parameters using dotNetRDF's SparqlParameterizedString, preventing injection attacks.
/// 
/// TODO: Full implementation requires:
/// 1. SPARQL-aware parsing to distinguish variable identifiers from SPARQL keywords
/// 2. Handling of SPARQL prefixes and namespaces
/// 3. Proper position tracking within the SPARQL text for diagnostics
/// 4. Type compatibility validation (int, string, float, etc. are bindable; Graph, Triple are not)
/// </remarks>
public class SparqlVariableBindingVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic> diagnostics = new();

    /// <summary>
    /// Gets the list of diagnostics generated during variable binding resolution.
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => diagnostics.AsReadOnly();

    /// <summary>
    /// Visits a SparqlLiteralExpression and resolves variable references within the SPARQL text.
    /// </summary>
    public override SparqlLiteralExpression VisitSparqlLiteralExpression(SparqlLiteralExpression ctx)
    {
        // First visit children using base implementation
        var result = base.VisitSparqlLiteralExpression(ctx);

        // Find the nearest scope for symbol table lookups
        var nearestScope = ctx.NearestScope();
        if (nearestScope == null)
        {
            return result;
        }

        // TODO: Implement SPARQL text parsing to extract variable identifiers
        // For now, this is a placeholder that doesn't populate bindings
        // Full implementation would:
        // 1. Parse SPARQL text using a simple lexer or regex
        // 2. Identify identifiers that aren't SPARQL keywords
        // 3. Attempt to resolve each identifier against the symbol table
        // 4. For resolved variables, create VariableBinding entries
        // 5. For unresolved identifiers, emit SPARQL002 diagnostic

        // Example pseudo-code for full implementation:
        // var identifiers = ExtractIdentifiers(result.SparqlText);
        // var bindings = new List<VariableBinding>();
        // 
        // foreach (var identifier in identifiers)
        // {
        //     if (TryResolveVariable(identifier.Name, nearestScope, out var varDecl))
        //     {
        //         bindings.Add(new VariableBinding
        //         {
        //             Name = identifier.Name,
        //             ResolvedExpression = new VarRefExp { VarName = identifier.Name, VariableDecl = varDecl },
        //             PositionInLiteral = identifier.Position,
        //             Length = identifier.Length,
        //             Location = result.Location,
        //             Parent = result,
        //             Annotations = []
        //         });
        //     }
        //     else
        //     {
        //         // Emit SPARQL002: Unknown variable
        //         EmitUnknownVariableDiagnostic(identifier.Name, result);
        //     }
        // }
        // 
        // return result with { Bindings = bindings };

        return result;
    }

    /// <summary>
    /// Attempts to resolve a variable reference by name within the given scope.
    /// </summary>
    private bool TryResolveVariable(string varName, ScopeAstThing scope, out VariableDecl? resolvedDecl)
    {
        resolvedDecl = null;
        
        if (string.IsNullOrEmpty(varName) || scope == null)
        {
            return false;
        }

        var symbol = new Symbol(varName, SymbolKind.VarDeclStatement);
        
        if (scope.TryResolve(symbol, out var symbolTableEntry))
        {
            if (symbolTableEntry?.OriginatingAstThing is VariableDecl variableDecl)
            {
                resolvedDecl = variableDecl;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Emits a diagnostic for an unknown variable reference in SPARQL text.
    /// </summary>
    private void EmitUnknownVariableDiagnostic(string varName, SparqlLiteralExpression context)
    {
        var diagnostic = new Diagnostic
        {
            Code = SparqlDiagnostics.UnknownVariable,
            Message = SparqlDiagnostics.FormatUnknownVariable(varName),
            Severity = DiagnosticSeverity.Error,
            Filename = context.Location?.Filename ?? "",
            Line = context.Location?.Line ?? 0,
            Column = context.Location?.Column ?? 0
        };

        diagnostics.Add(diagnostic);
    }
}

/// <summary>
/// Represents a diagnostic message from the SPARQL variable binding process.
/// </summary>
public class Diagnostic
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public required string Filename { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
}

/// <summary>
/// Severity levels for diagnostics.
/// </summary>
public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}
