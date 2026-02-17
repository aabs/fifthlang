namespace ast_model.Symbols;

public class SymbolTable : Dictionary<Symbol, ISymbolTableEntry>, ISymbolTable
{
    public IEnumerable<ISymbolTableEntry> All()
    {
        return Values;
    }

    public ISymbolTableEntry Resolve(Symbol symbol)
    {
        if (TryGetValue(symbol, out var result))
        {
            return result;
        }
        return null;
    }

    public ISymbolTableEntry ResolveByName(string symbolName)
    {
        foreach (var k in Keys)
        {
            if (k.Name == symbolName)
            {
                return this[k];
            }
        }

        return null;
    }
}
