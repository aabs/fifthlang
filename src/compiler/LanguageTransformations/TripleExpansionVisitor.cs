using ast;

namespace compiler.LanguageTransformations;

/// <summary>
/// Expands triples whose ObjectExp is a flat ListLiteral into a ListLiteral of individual Triple nodes.
/// Leaves nested-list (error) and empty-list (already diagnosed) cases untouched (empty list becomes empty list of triples via replacement logic returning an empty ListLiteral).
/// </summary>
public sealed class TripleExpansionVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<compiler.Diagnostic>? _diagnostics;

    public TripleExpansionVisitor(List<compiler.Diagnostic>? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public override BlockStatement VisitBlockStatement(BlockStatement ctx)
    {
        // First visit children normally
        var visited = base.VisitBlockStatement(ctx);
        // Then post-process statements for expansion inside variable initializers and expression statements
        for (int i = 0; i < visited.Statements.Count; i++)
        {
            switch (visited.Statements[i])
            {
                case VarDeclStatement vds when vds.InitialValue is TripleLiteralExp t:
                    vds.InitialValue = ExpandTripleIfNeeded(t);
                    break;
                case ExpStatement es when es.RHS is TripleLiteralExp t2:
                    es.RHS = ExpandTripleIfNeeded(t2);
                    break;
                case VarDeclStatement vds2 when vds2.InitialValue is MalformedTripleExp:
                    // Do not attempt expansion on malformed triples
                    break;
                case ExpStatement es2 when es2.RHS is MalformedTripleExp:
                    // Do not attempt expansion on malformed triples
                    break;
                case BlockStatement inner:
                    // already visited; nothing extra
                    break;
            }
        }
        return visited;
    }

    private Expression ExpandTripleIfNeeded(TripleLiteralExp triple)
    {
        if (triple.ObjectExp is ListLiteral list && list.ElementExpressions.Count > 0 && !list.ElementExpressions.Any(e => e is ListLiteral))
        {
            var expanded = list.ElementExpressions.Select(obj => new TripleLiteralExp
            {
                SubjectExp = triple.SubjectExp,
                PredicateExp = triple.PredicateExp,
                ObjectExp = obj
            }).Cast<Expression>().ToList();
            return new ListLiteral { ElementExpressions = expanded };
        }
        if (triple.ObjectExp is ListLiteral ll && ll.ElementExpressions.Count == 0)
        {
            // Empty list -> becomes empty list literal (no triples) and emit warning TRPL004
            _diagnostics?.Add(new compiler.Diagnostic(
                compiler.DiagnosticLevel.Warning,
                "Empty list in triple object produces no triples.",
                null,
                "TRPL004"));
            return new ListLiteral { ElementExpressions = new List<Expression>() };
        }
        if (triple.ObjectExp is ListLiteral ll2 && ll2.ElementExpressions.Any(e => e is ListLiteral))
        {
            // Nested list found -> emit error TRPL006 and leave triple unchanged for further handling
            _diagnostics?.Add(new compiler.Diagnostic(
                compiler.DiagnosticLevel.Error,
                "Nested lists not allowed in triple object.",
                null,
                "TRPL006"));
            return triple;
        }
        return triple;
    }
}
