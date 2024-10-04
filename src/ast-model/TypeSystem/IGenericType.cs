namespace ast_model.TypeSystem;

public interface IGenericType : IType
{
    public TypeId[] GenericTypeParameters { get; set; }
}
