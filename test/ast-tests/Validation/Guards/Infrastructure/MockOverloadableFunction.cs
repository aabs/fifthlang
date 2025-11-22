using ast;
using ast_model.TypeSystem;

namespace ast_tests.Validation.Guards.Infrastructure;

/// <summary>
/// Mock implementation of IOverloadableFunction for testing purposes.
/// </summary>
public class MockOverloadableFunction : IOverloadableFunction
{
    public MemberName Name => MemberName.From("testFunc");
    public List<ParamDef> Params { get; }
    public BlockStatement Body { get; set; } = new BlockStatement { Statements = [] };
    public FifthType ReturnType { get; set; } = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("Int32") };

    public MockOverloadableFunction(bool hasConstraints = false, int paramCount = 1)
    {
        Params = new List<ParamDef>();

        for (int i = 0; i < paramCount; i++)
        {
            var param = new ParamDef
            {
                Name = $"param{i}",
                TypeName = TypeName.From("int"),
                ParameterConstraint = hasConstraints ? new BooleanLiteralExp { Value = true } : null,
                Visibility = Visibility.Private,
                DestructureDef = null,
                CollectionType = CollectionType.SingleInstance
            };
            Params.Add(param);
        }
    }

    public MockOverloadableFunction WithConstraint(Expression constraint, int paramIndex = 0)
    {
        if (paramIndex < Params.Count)
        {
            Params[paramIndex].ParameterConstraint = constraint;
        }
        return this;
    }
}