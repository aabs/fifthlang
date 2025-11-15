using System.Collections.Generic;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowering rewriter for QueryApplicationExp nodes.
/// 
/// Transforms query application expressions (query <- store) into function calls
/// to the runtime executor: Fifth.System.QueryApplicationExecutor.Execute(query, store).
/// 
/// Input AST:
///   QueryApplicationExp { Query = queryExpr, Store = storeExpr, InferredType = Result }
/// 
/// Output (lowered) AST:
///   FuncCallExp(Fifth.System.QueryApplicationExecutor.Execute, args: [queryExpr, storeExpr])
/// 
/// The lowering enables runtime execution of SPARQL queries against RDF stores
/// and returns a Result discriminated union.
/// </summary>
public class QueryApplicationLoweringRewriter : DefaultAstRewriter
{
    // Fifth type representation for Result
    private static readonly FifthType ResultType = new FifthType.TType { Name = TypeName.From("Result") };

    /// <summary>
    /// Rewrites QueryApplicationExp nodes to QueryApplicationExecutor.Execute calls.
    /// </summary>
    public override RewriteResult VisitQueryApplicationExp(QueryApplicationExp ctx)
    {
        // Rewrite the Query and Store operands first
        var queryResult = Rewrite(ctx.Query);
        var storeResult = Rewrite(ctx.Store);

        // Collect prologue statements from operands
        var prologue = new List<Statement>();
        prologue.AddRange(queryResult.Prologue);
        prologue.AddRange(storeResult.Prologue);

        // Create a function call to Fifth.System.QueryApplicationExecutor.Execute
        var funcCallExp = new FuncCallExp
        {
            // The function name will be resolved during code generation
            // For now, we mark it with an annotation
            FunctionDef = null, // Will be resolved later
            InvocationArguments = new List<Expression> 
            { 
                queryResult.Node as Expression ?? ctx.Query,
                storeResult.Node as Expression ?? ctx.Store
            },
            Type = ctx.InferredType ?? ResultType,
            Location = ctx.Location,
            Annotations = new Dictionary<string, object>
            {
                // Mark this as a query application lowering for downstream processing
                ["QueryApplicationLowering"] = true,
                ["RuntimeMethod"] = "Fifth.System.QueryApplicationExecutor.Execute"
            }
        };

        return new RewriteResult(funcCallExp, prologue);
    }
}
