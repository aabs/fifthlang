using ast;
using ast_generated;
using ast_model.TypeSystem;
using Fifth.LangProcessingPhases;
using System.Linq;

namespace ast_tests;

public class OverloadTransformingVisitorTests : VisitorTestsBase
{
    [Fact]
    public void GenerateGuardFunction_ShouldCreateGuardFunction()
    {
        // Arrange
        var visitor = new OverloadTransformingVisitor();
        var cd = CreateClassDef("class1");
        var methodDef1 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var methodDef2 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var clauses = new List<(Expression, MethodDef)>
        {
            (True, methodDef1),
            (False, methodDef2)
        };
        var overloadedFunction = new OverloadedFunctionDefinitionBuilder()
            .WithName(MemberName.From("Test1"))
            .WithOverloadClauses(new List<MethodDef> { methodDef1, methodDef2 }.Cast<IOverloadableFunction>().ToList())
            .WithSignature(methodDef1.FunctionDef.ToFunctionSignature())
            .Build().WithParent(cd);

        // Act
        var guardFunction = visitor.GenerateGuardFunction((OverloadedFunctionDefinition)overloadedFunction, clauses);

        // Assert
        Assert.NotNull(guardFunction);
        Assert.Equal("Test1", guardFunction.Name.Value);
    }

    [Fact]
    public void GetPrecondition_ShouldReturnCombinedPreconditions()
    {
        // Arrange
        var visitor = new OverloadTransformingVisitor();
        var methodDef = CreateMethodDef("Test", "int");
        methodDef.FunctionDef.Params = [CreateParamDef("p1", "int", True), CreateParamDef("p2", "int", False)];

        // Act
        var precondition = visitor.GetPrecondition(methodDef);

        // Assert
        Assert.NotNull(precondition);
        Assert.IsType<BinaryExp>(precondition);
    }

    [Fact]
    public void ProcessOverloadedFunctionDefinition_ShouldTransformFunction()
    {
        // Arrange
        var visitor = new OverloadTransformingVisitor();
        var cd = CreateClassDef("class1");
        var methodDef1 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var methodDef2 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var clauses = new List<(Expression, MethodDef)>
        {
            (True, methodDef1),
            (False, methodDef2)
        };
        var overloadedFunction = new OverloadedFunctionDefinitionBuilder()
            .WithName(MemberName.From("Test1"))
            .WithOverloadClauses(new List<MethodDef> { methodDef1, methodDef2 }.Cast<IOverloadableFunction>().ToList())
            .WithSignature(methodDef1.FunctionDef.ToFunctionSignature())
            .Build().WithParent(cd);

        // Act
        visitor.ProcessOverloadedFunctionDefinition((OverloadedFunctionDefinition)overloadedFunction);

        // Assert
        Assert.Contains(cd.MemberDefs, m => m.Name == "Test1_subclause1");
        Assert.Contains(cd.MemberDefs, m => m.Name == "Test1_subclause2");
    }
}
