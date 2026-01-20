namespace ast_model.Symbols;

public interface ISymbolTableEntry
{
    Dictionary<string, object> Annotations { get; init; }
    IAstThing OriginatingAstThing { get; init; }
    Symbol Symbol { get; init; }
    string? QualifiedName { get; init; }
    bool IsImported { get; init; }
    bool IsLocalShadow { get; init; }
}

public record SymbolTableEntry : ISymbolTableEntry
{
    public Symbol Symbol { get; init; }
    public required IAstThing OriginatingAstThing { get; init; }
    public required Dictionary<string, object> Annotations { get; init; }
    public string? QualifiedName { get; init; }
    public bool IsImported { get; init; }
    public bool IsLocalShadow { get; init; }
}
