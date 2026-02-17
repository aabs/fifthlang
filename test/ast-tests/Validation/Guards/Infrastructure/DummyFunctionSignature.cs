using ast;
using ast_model.TypeSystem;

namespace ast_tests.Validation.Guards.Infrastructure;

internal sealed class DummyFunctionSignature : IFunctionSignature
{
    public FifthType? DeclaringType { get; set; }
    public FifthType[] FormalParameterTypes { get; set; } = [];
    public FifthType[] GenericTypeParameters { get; set; } = [];
    public MemberName Name { get; set; } = MemberName.From("testFunc");
    public FifthType ReturnType { get; set; } = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("Int32") };
}
