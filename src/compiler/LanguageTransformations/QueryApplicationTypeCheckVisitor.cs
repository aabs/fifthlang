using System.Collections.Generic;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Type checking rewriter for QueryApplicationExp nodes.
/// 
/// Validates that:
/// 1. Left-hand side (Query operand) is of type Query
/// 2. Right-hand side (Store operand) is of type Store
/// 3. Infers the result type as Result
/// 
/// Emits compile-time diagnostics for type mismatches.
/// Uses rewriter pattern (preferred over visitors for transformations).
/// </summary>
public class QueryApplicationTypeCheckRewriter : DefaultAstRewriter
{
    private readonly List<Diagnostic>? _diagnostics;
    
    // Fifth type representations
    private static readonly FifthType QueryType = new FifthType.TType { Name = TypeName.From("Query") };
    private static readonly FifthType StoreType = new FifthType.TType { Name = TypeName.From("Store") };
    private static readonly FifthType ResultType = new FifthType.TType { Name = TypeName.From("Result") };

    public QueryApplicationTypeCheckRewriter(List<Diagnostic>? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public override RewriteResult VisitQueryApplicationExp(QueryApplicationExp ctx)
    {
        // Rewrite children first to ensure their types are inferred
        var queryResult = Rewrite(ctx.Query);
        var storeResult = Rewrite(ctx.Store);

        var prologue = new List<Statement>();
        prologue.AddRange(queryResult.Prologue);
        prologue.AddRange(storeResult.Prologue);

        var visitedQuery = (Expression)(queryResult.Node ?? ctx.Query);
        var visitedStore = (Expression)(storeResult.Node ?? ctx.Store);

        // Check if Query operand has the correct type
        if (visitedQuery.Type != null && !IsQueryType(visitedQuery.Type))
        {
            _diagnostics?.Add(new Diagnostic(
                DiagnosticLevel.Error,
                $"Query application operator '<-' expects Query type on left-hand side, but got {visitedQuery.Type}",
                visitedQuery.Location?.ToString(),
                "QUERY_APP_001"
            ));
        }

        // Check if Store operand has the correct type
        if (visitedStore.Type != null && !IsStoreType(visitedStore.Type))
        {
            _diagnostics?.Add(new Diagnostic(
                DiagnosticLevel.Error,
                $"Query application operator '<-' expects Store type on right-hand side, but got {visitedStore.Type}",
                visitedStore.Location?.ToString(),
                "QUERY_APP_002"
            ));
        }

        // Return updated node with inferred type
        var updatedNode = ctx with 
        {
            Query = visitedQuery,
            Store = visitedStore,
            InferredType = ResultType
        };

        return new RewriteResult(updatedNode, prologue);
    }

    private static bool IsQueryType(FifthType type)
    {
        return type switch
        {
            FifthType.TType t => t.Name.ToString() == "Query",
            _ => false
        };
    }

    private static bool IsStoreType(FifthType type)
    {
        return type switch
        {
            FifthType.TType t => t.Name.ToString() == "Store",
            _ => false
        };
    }
}

