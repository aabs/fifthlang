using System.Collections.Generic;
using ast;
using ast_generated;
using ast_model.Symbols;

namespace compiler.LanguageTransformations;

/// <summary>
/// Rewriter-based lowering pass for destructuring parameters.
/// Replaces the old visitor-based approach with statement-hoisting via DefaultAstRewriter.
/// 
/// Lowering rules:
/// 1) For each ParamDef with DestructureDef, hoist a temp variable for the parameter
/// 2) For each PropertyBindingDef in the destructure:
///    - Create a local variable that reads the property from the temp
///    - If Constraint exists, emit guard statement
///    - If nested DestructureDef exists, recursively process
/// 3) Insert all hoisted statements at the start of the function body
/// </summary>
public class DestructuringLoweringRewriter : DefaultAstRewriter
{
    private int _tmpCounter = 0;

    /// <summary>
    /// Generate a fresh temporary variable name
    /// </summary>
    private string FreshTempName(string prefix = "tmp") => $"__{prefix}{_tmpCounter++}";

    public override RewriteResult VisitFunctionDef(FunctionDef ctx)
    {
        // Check if this function has any destructured parameters
        bool hasDestructuring = ctx.Params != null && ctx.Params.Any(p => p.DestructureDef != null);

        if (!hasDestructuring)
        {
            // No destructuring - use default behavior
            return base.VisitFunctionDef(ctx);
        }

        var updatedParams = new List<ParamDef>(ctx.Params.Count);

        foreach (var param in ctx.Params)
        {
            if (param.DestructureDef == null)
            {
                updatedParams.Add(param);
                continue;
            }

            if (param.HasAnnotation(DestructuringConstraintPropagator.ConstraintAnnotationKey))
            {
                updatedParams.Add(param);
                continue;
            }

            var constraints = DestructuringConstraintUtilities.CollectConstraints(param, param.DestructureDef);
            if (constraints.Count == 0)
            {
                updatedParams.Add(param);
                continue;
            }

            var combinedConstraint = DestructuringConstraintUtilities.CombineConstraints(param, constraints);
            if (combinedConstraint == null)
            {
                updatedParams.Add(param);
                continue;
            }

            var updatedParam = param with { ParameterConstraint = combinedConstraint };
            updatedParam[DestructuringConstraintPropagator.ConstraintAnnotationKey] = true;
            updatedParams.Add(updatedParam);
        }

        // Process the function body first to get any nested prologues
        var bodyResult = Rewrite(ctx.Body);
        var bodyStatements = new List<Statement>();

        // Now process each parameter and emit destructuring statements
        var destructuringStatements = new List<Statement>();

        foreach (var param in updatedParams)
        {
            if (param.DestructureDef != null)
            {
                // Lower this destructured parameter
                var stmts = LowerParameterDestructuring(param);
                destructuringStatements.AddRange(stmts);
            }
        }

        // Combine: destructuring statements first, then original body statements
        bodyStatements.AddRange(destructuringStatements);
        bodyStatements.AddRange(bodyResult.Prologue);
        bodyStatements.AddRange(((BlockStatement)bodyResult.Node).Statements);

        var newBody = new BlockStatement
        {
            Statements = bodyStatements,
            Location = ctx.Body.Location,
            Annotations = ctx.Body.Annotations
        };

        var rebuilt = ctx with { Params = updatedParams, Body = newBody };
        return new RewriteResult(rebuilt, []);
    }

    /// <summary>
    /// Lower a destructured parameter into variable declarations
    /// </summary>
    private List<Statement> LowerParameterDestructuring(ParamDef param)
    {
        var statements = new List<Statement>();

        if (param.DestructureDef == null)
            return statements;

        // The parameter itself is accessible by param.Name
        var paramSourceExpr = new VarRefExp
        {
            VarName = param.Name,
            Location = param.Location ?? new SourceLocationMetadata(),
            Annotations = new Dictionary<string, object>()
        };

        // Process each property binding
        foreach (var binding in param.DestructureDef.Bindings)
        {
            var bindingStmts = LowerPropertyBinding(binding, paramSourceExpr, param);
            statements.AddRange(bindingStmts);
        }

        return statements;
    }

    /// <summary>
    /// Lower a property binding into variable declarations
    /// </summary>
    private List<Statement> LowerPropertyBinding(PropertyBindingDef binding, Expression sourceExpr, ParamDef param)
    {
        var statements = new List<Statement>();
        var loc = binding.Location ?? new SourceLocationMetadata();

        // Create member access: sourceExpr.PropertyName
        var propertyName = binding.ReferencedProperty?.Name ?? binding.ReferencedPropertyName;
        var propertyAccess = new MemberAccessExp
        {
            LHS = sourceExpr,
            RHS = new VarRefExp
            {
                VarName = propertyName.Value,
                Location = loc,
                Annotations = new Dictionary<string, object>()
            },
            Location = loc,
            Annotations = new Dictionary<string, object>()
        };

        // Determine the type for the variable
        var typeName = binding.ReferencedProperty?.TypeName ?? TypeName.From("object");

        // Create variable declaration: var introducedVar = sourceExpr.PropertyName
        var varDecl = new VariableDecl
        {
            Name = binding.IntroducedVariable.Value,
            TypeName = typeName,
            CollectionType = CollectionType.SingleInstance,
            Visibility = Visibility.Private,
            Location = loc,
            Annotations = new Dictionary<string, object>()
        };

        var varDeclStmt = new VarDeclStatement
        {
            VariableDecl = varDecl,
            InitialValue = propertyAccess,
            Location = loc,
            Annotations = new Dictionary<string, object>()
        };

        statements.Add(varDeclStmt);

        // Handle constraint if present
        if (binding.Constraint != null)
        {
            // Create guard statement for the constraint
            // The constraint should reference the introduced variable
            var guardStmt = new GuardStatement
            {
                Condition = binding.Constraint,
                Location = loc,
                Annotations = new Dictionary<string, object>()
            };
            statements.Add(guardStmt);
        }

        // Handle nested destructuring if present
        if (binding.DestructureDef != null)
        {
            // The introduced variable becomes the source for nested destructuring
            var nestedSourceExpr = new VarRefExp
            {
                VarName = binding.IntroducedVariable.Value,
                Location = loc,
                Annotations = new Dictionary<string, object>()
            };

            foreach (var nestedBinding in binding.DestructureDef.Bindings)
            {
                var nestedStmts = LowerPropertyBinding(nestedBinding, nestedSourceExpr, param);
                statements.AddRange(nestedStmts);
            }
        }

        return statements;
    }
}
