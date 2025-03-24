using ast;
using ast_generated;
using ast_model.TypeSystem;
using compiler.LanguageTransformations;
using FluentAssertions;

namespace ast_tests;

public class OverloadGatheringVisitorTests : VisitorTestsBase
{
    [Fact]
    public void Gather_ShouldGroupOverloadedFunctionsAndSubstituteThem()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();

        var methodDef1 = CreateMethodDef("Test", "int");
        var methodDef2 = CreateMethodDef("Test", "int");
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs(new List<MemberDef> { methodDef1, methodDef2 }).Build();
        methodDef1.Parent = methodDef1.FunctionDef.Parent = classDef;
        methodDef2.Parent = methodDef2.FunctionDef.Parent = classDef;

        // Act
        visitor.Gather(classDef);

        // Assert
        Assert.Single(classDef.MemberDefs);
        Assert.IsType<OverloadedFunctionDefinition>(classDef.MemberDefs.First());
    }

    [Fact]
    public void GatherOverloads_ShouldReturnGroupedMethodsByFunctionSignature()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var methodDef1 = CreateMethodDef("Test", "int");
        var methodDef2 = CreateMethodDef("Test", "int");
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs(new List<MemberDef> { methodDef1, methodDef2 }).Build();
        methodDef1.Parent = methodDef1.FunctionDef.Parent = classDef;
        methodDef2.Parent = methodDef2.FunctionDef.Parent = classDef;
        // Act
        var result = visitor.GatherOverloads(classDef);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result.First().Value.Count);
    }

    [Fact]
    public void SubstituteFunctionDefinitions_ShouldReplaceOldMethodsWithCombinedFunction()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var methodDef1 = CreateMethodDef("Test", "int");
        var methodDef2 = CreateMethodDef("Test", "int");
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs(new List<MemberDef> { methodDef1, methodDef2 }).Build();
        methodDef1.Parent = methodDef1.FunctionDef.Parent = classDef;
        methodDef2.Parent = methodDef2.FunctionDef.Parent = classDef;
        var combinedFunction = new OverloadedFunctionDefinitionBuilder()
        .WithOverloadClauses(new List<MethodDef> { methodDef1, methodDef2 })
        .WithSignature(new FunctionSignature())
        .Build();

        // Act
        visitor.SubstituteFunctionDefinitions(classDef, new List<MethodDef> { methodDef1, methodDef2 }, combinedFunction);

        // Assert
        Assert.Single(classDef.MemberDefs);
        Assert.Equal(combinedFunction, classDef.MemberDefs.First());
    }

    [Fact]
    public void VisitClassDef_ShouldCallGatherAndReturnBaseVisitClassDef()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs([]).Build();

        // Act
        var result = visitor.VisitClassDef(classDef);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(classDef);
    }
}
