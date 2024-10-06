namespace ast_model.Symbols;

public interface ISymbolTableEntry
{
    Dictionary<string, object> Annotations { get; init; }
    IAstThing OriginatingAstThing { get; init; }
    Symbol Symbol { get; init; }
}

public record SymbolTableEntry : ISymbolTableEntry
{
    public Symbol Symbol { get; init; }
    public required IAstThing OriginatingAstThing { get; init; }
    public required Dictionary<string, object> Annotations { get; init; }
}
