namespace ast_model.Symbols;

public static class SymbolHelpers
{
    public static ScopeAstThing NearestScope(this IAstThing node)
    {
        if (node is null)
        {
            return null;
        }
        if (node is ScopeAstThing astNode)
        {
            return astNode;
        }

        return node?.Parent.NearestScope();
    }

    public static ScopeAstThing NearestScopeAbove(this IAstThing node)
    {
        return node?.Parent?.NearestScope();
    }

    public static ISymbolTableEntry Resolve(this IAstThing node, Symbol symbol)
    {
        if (node.TryResolve(symbol, out var ste))
        {
            return ste;
        }

        throw new CompilationException($"Unable to resolve symbol {symbol.Name}");
    }

    public static bool TryResolve(this IAstThing node, Symbol symbol, out ISymbolTableEntry? result)
    {
        if (node is null)
        {
            result = default;
            return false;
        }
        if (node is ScopeAstThing sat)
        {
            return sat.TryResolve(symbol, out result);
        }

        return node.Parent.TryResolve(symbol, out result);
    }
}
