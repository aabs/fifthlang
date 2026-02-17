namespace ast_model.TypeSystem;

public record TypeAlias
{
    public FifthType AliasedType { get; init; }
    public TypeName AlternateName { get; init; }
}
