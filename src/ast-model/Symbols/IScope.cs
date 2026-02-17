namespace ast_model.Symbols;

public interface IScope
{
    IScope EnclosingScope { get; init; }
    ISymbolTable SymbolTable { get; init; }

    void Declare(Symbol symbol, IAstThing astThing, Dictionary<string, object> annotations);

    bool TryResolve(Symbol symbol, out ISymbolTableEntry result);
}
