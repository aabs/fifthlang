using ast;
using ast_generated;
using ast_model.TypeSystem;
using compiler.LanguageTransformations;
using FluentAssertions;
using System.Linq;

namespace ast_tests;

public class ModuleOverloadTests : VisitorTestsBase
{
    [Fact]
    public void GatherModule_ShouldGroupOverloadedFunctionsAndSubstituteThem()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();

        var functionDef1 = CreateFunctionDef("Test", "int");
        var functionDef2 = CreateFunctionDef("Test", "int");
        var moduleDef = new ModuleDefBuilder()
            .WithOriginalModuleName("TestModule")
            .WithNamespaceDecl(NamespaceName.From("test"))
            .WithFunctions([functionDef1, functionDef2])
            .Build();
        functionDef1.Parent = moduleDef;
        functionDef2.Parent = moduleDef;

        // Act
        visitor.GatherModule(moduleDef);

        // Assert
        moduleDef.Functions.Should().ContainSingle();
        moduleDef.Functions.First().Should().BeOfType<OverloadedFunctionDef>();
    }

    [Fact]
    public void GatherModuleOverloads_ShouldReturnGroupedFunctionsByFunctionSignature()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var functionDef1 = CreateFunctionDef("Test", "int");
        var functionDef2 = CreateFunctionDef("Test", "int");
        var moduleDef = new ModuleDefBuilder()
            .WithOriginalModuleName("TestModule")
            .WithNamespaceDecl(NamespaceName.From("test"))
            .WithFunctions([functionDef1, functionDef2])
            .Build();
        functionDef1.Parent = moduleDef;
        functionDef2.Parent = moduleDef;

        // Act
        var result = visitor.GatherModuleOverloads(moduleDef);

        // Assert
        result.Should().ContainSingle();
        result.First().Value.Count.Should().Be(2);
    }

    [Fact]
    public void SubstituteModuleFunctionDefinitions_ShouldReplaceOldFunctionsWithCombinedFunction()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var functionDef1 = CreateFunctionDef("Test", "int");
        var functionDef2 = CreateFunctionDef("Test", "int");
        var moduleDef = new ModuleDefBuilder()
            .WithOriginalModuleName("TestModule")
            .WithNamespaceDecl(NamespaceName.From("test"))
            .WithFunctions([functionDef1, functionDef2])
            .Build();
        functionDef1.Parent = moduleDef;
        functionDef2.Parent = moduleDef;
        
        var combinedFunction = new OverloadedFunctionDefBuilder()
            .WithOverloadClauses(new List<FunctionDef> { functionDef1, functionDef2 }.Cast<IOverloadableFunction>().ToList())
            .WithSignature(new FunctionSignature())
            .WithName(functionDef1.Name)
            .WithParams(functionDef1.Params)
            .WithBody(functionDef1.Body)
            .WithReturnType(functionDef1.ReturnType)
            .WithIsStatic(functionDef1.IsStatic)
            .WithIsConstructor(functionDef1.IsConstructor)
            .Build();

        // Act
        visitor.SubstituteModuleFunctionDefinitions(moduleDef, new List<FunctionDef> { functionDef1, functionDef2 }, combinedFunction);

        // Assert
        moduleDef.Functions.Should().ContainSingle();
        moduleDef.Functions.First().Should().BeSameAs(combinedFunction);
    }

    [Fact]
    public void VisitModuleDef_ShouldCallGatherModuleAndReturnBaseVisitModuleDef()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var moduleDef = new ModuleDefBuilder()
            .WithOriginalModuleName("TestModule")
            .WithNamespaceDecl(NamespaceName.From("test"))
            .WithFunctions([])
            .Build();

        // Act
        var result = visitor.VisitModuleDef(moduleDef);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(moduleDef);
    }

    private new FunctionDef CreateFunctionDef(string name, string returnType)
    {
        return base.CreateFunctionDef(name, returnType);
    }
}