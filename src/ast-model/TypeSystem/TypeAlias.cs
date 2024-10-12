namespace ast_model.TypeSystem;

public record TypeAlias 
{
    public TypeId AliasedType { get; init; }
    public TypeName AlternateName { get; init; }
}
