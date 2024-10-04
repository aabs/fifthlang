namespace ast_model.TypeSystem;

public interface IFunctionSignature
{
    TypeId[] FormalParameterTypes { get; set; }
    TypeId[] GenericTypeParameters { get; set; }
    MemberName Name { get; set; }
    TypeId ReturnType { get; set; }
    TypeId? DeclaringType { get; set; }
}
