using ast;

namespace ast_model.TypeSystem;

public record FunctionSignature : IFunctionSignature
{
    public TypeId[] FormalParameterTypes { get; set;}
    public TypeId[] GenericTypeParameters { get; set;}
    public MemberName Name { get; set;}
    public TypeId ReturnType { get; set;}
    public TypeId? DeclaringType { get; set; }
}
