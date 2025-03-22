namespace ast_model.TypeSystem;

public interface IFunctionSignature
{
    FifthType? DeclaringType { get; set; }
    FifthType[] FormalParameterTypes { get; set; }
    FifthType[] GenericTypeParameters { get; set; }
    MemberName Name { get; set; }
    FifthType ReturnType { get; set; }
}
