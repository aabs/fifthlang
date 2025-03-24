namespace ast_model.Symbols
{
    public interface ISymbolTable : IDictionary<Symbol, ISymbolTableEntry>
    {
        IEnumerable<ISymbolTableEntry> All();

        ISymbolTableEntry Resolve(Symbol symbol);

        ISymbolTableEntry ResolveByName(string symbol);
    }
}
