namespace ast_model.TypeSystem;

public interface IType
{
    public TypeName Name { get; init; }
    public NamespaceName Namespace { get; init; }
    public TypeId TypeId { get; init; } // no more than 64K types
}
