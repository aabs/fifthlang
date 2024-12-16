using ast_model.TypeSystem.Inference;
using FluentAssertions;
using Type = ast_model.TypeSystem.Inference.Type;

namespace ast_tests;

public class TypeInferenceTests
{
    private readonly Type boolType = new Type("bool");
    private readonly Type floatType = new Type("float");
    private readonly Type intType = new Type("int");
    private readonly Dictionary<string, BaseType> typeMap;
    private readonly TypeSystem typeSystem;
    private readonly Type voidType = new Type("void");

    public TypeInferenceTests()
    {
        typeMap = new()
        {
            ["int"] = intType,
            ["float"] = floatType,
            ["bool"] = boolType,
        };
        typeSystem = new TypeSystem()
            .WithType(intType)
            .WithType(boolType)
            .WithFunction([intType, intType], intType, "+")
            .WithFunction([intType, intType], intType, "-")
            .WithFunction([intType, intType], intType, "*")
            .WithFunction([intType, intType], floatType, "/")
            ;
    }

    [Fact]
    public void can_infer_function_return_types()
    {
        Arrow funResult = typeSystem.Build([], intType, "");
        typeSystem.WithFunction([intType], funResult, "bar");
        typeSystem.InferResultType([intType], "bar").Should().Be(funResult);
    }

    [Fact]
    public void can_infer_the_type_of_a_function_with_no_args()
    {
        typeSystem.WithFunction([], boolType, "foo");
        typeSystem.InferResultType([], "foo").Should().Be(boolType);
    }

    [Theory]
    [InlineData("int", "int", "+", "int")]
    [InlineData("int", "int", "-", "int")]
    [InlineData("int", "int", "*", "int")]
    [InlineData("int", "int", "/", "float")]
    public void can_infer_types_for_common_binary_operators(string t1, string t2, string op, string expected)
    {
        typeSystem.InferResultType([typeMap[t1], typeMap[t2]], op).Should().Be(typeMap[expected]);
    }

    [Fact]
    public void can_infer_types_of_functions_with_more_args()
    {
        typeSystem.WithFunction([intType, floatType, boolType], boolType, "foo");
        typeSystem.InferResultType([intType, floatType, boolType], "foo").Should().Be(boolType);
    }

    [Fact]
    public void can_infer_with_function_params()
    {
        Arrow funParam = typeSystem.Build([], intType, "");
        typeSystem.WithFunction([funParam], intType, "bar");
        typeSystem.InferResultType([funParam], "bar").Should().Be(intType);
    }
}
