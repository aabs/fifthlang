namespace ast_model.TypeSystem;

public record FunctionSignature : IFunctionSignature
{
    public FifthType[] FormalParameterTypes { get; set; }
    public FifthType[] GenericTypeParameters { get; set; }
    public MemberName Name { get; set; }
    public FifthType ReturnType { get; set; }
    public FifthType? DeclaringType { get; set; }
}
