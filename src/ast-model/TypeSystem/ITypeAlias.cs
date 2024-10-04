namespace ast_model.TypeSystem;

public interface ITypeAlias : IType
{
    public TypeId AliasedTypeId { get; }
}
