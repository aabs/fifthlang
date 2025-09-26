using ast;

namespace compiler.LanguageTransformations;

/// <summary>
/// Expands triples whose ObjectExp is a flat ListLiteral into a ListLiteral of individual Triple nodes.
/// Leaves nested-list (error) and empty-list (already diagnosed) cases untouched (empty list becomes empty list of triples via replacement logic returning an empty ListLiteral).
/// </summary>
public sealed class TripleExpansionVisitor : DefaultRecursiveDescentVisitor
{
    public override BlockStatement VisitBlockStatement(BlockStatement ctx)
    {
        // First visit children normally
        var visited = base.VisitBlockStatement(ctx);
        // Then post-process statements for expansion inside variable initializers and expression statements
        for (int i = 0; i < visited.Statements.Count; i++)
        {
            switch (visited.Statements[i])
            {
                case VarDeclStatement vds when vds.InitialValue is Triple t:
                    vds.InitialValue = ExpandTripleIfNeeded(t);
                    break;
                case ExpStatement es when es.RHS is Triple t2:
                    es.RHS = ExpandTripleIfNeeded(t2);
                    break;
                case BlockStatement inner:
                    // already visited; nothing extra
                    break;
            }
        }
        return visited;
    }

    private Expression ExpandTripleIfNeeded(Triple triple)
    {
        if (triple.ObjectExp is ListLiteral list && list.ElementExpressions.Count > 0 && !list.ElementExpressions.Any(e => e is ListLiteral))
        {
            var expanded = list.ElementExpressions.Select(obj => new Triple
            {
                SubjectExp = triple.SubjectExp,
                PredicateExp = triple.PredicateExp,
                ObjectExp = obj
            }).Cast<Expression>().ToList();
            return new ListLiteral { ElementExpressions = expanded };
        }
        if (triple.ObjectExp is ListLiteral ll && ll.ElementExpressions.Count == 0)
        {
            // Empty list -> becomes empty list literal (no triples)
            return new ListLiteral { ElementExpressions = new List<Expression>() };
        }
        return triple;
    }
}
