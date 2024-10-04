namespace ast_model.TypeSystem.PrimitiveTypes;

public abstract class PrimitiveAny : IType
{
    public TypeName Name { get; init; }
    public NamespaceName Namespace { get; init; }
    public TypeId TypeId { get; init; }
}
