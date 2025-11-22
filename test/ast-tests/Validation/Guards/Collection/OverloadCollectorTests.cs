using FluentAssertions;
using Xunit;
using ast;
using ast_generated;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation.Collection;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Collection;

public class OverloadCollectorTests
{
    [Fact]
    public void Reset_ShouldClearAllFunctionGroups()
    {
        // Arrange
        var collector = new OverloadCollector();
        var mockFunction = new MockOverloadableFunction();
        var moduleWithFunction = CreateModuleWithFunction(mockFunction);
        collector.CollectFromModule(moduleWithFunction);

        // Act
        collector.Reset();

        // Assert
        collector.FunctionGroups.Should().BeEmpty();
    }

    [Fact]
    public void CollectFromModule_WithOverloadedFunction_ShouldCreateFunctionGroup()
    {
        // Arrange
        var collector = new OverloadCollector();
        var mockFunction1 = new MockOverloadableFunction(hasConstraints: true);
        var mockFunction2 = new MockOverloadableFunction(hasConstraints: false);
        var moduleWithOverloads = CreateModuleWithOverloadedFunction([mockFunction1, mockFunction2]);

        // Act
        collector.CollectFromModule(moduleWithOverloads);

        // Assert
        collector.FunctionGroups.Should().HaveCount(1);
        var group = collector.FunctionGroups.Values.First();
        group.Overloads.Should().HaveCount(2);
        group.Name.Should().Be("testFunc");
    }

    [Fact]
    public void CollectFromClass_WithOverloadedFunction_ShouldCreateFunctionGroup()
    {
        // Arrange
        var collector = new OverloadCollector();
        var mockFunction1 = new MockOverloadableFunction(hasConstraints: true);
        var mockFunction2 = new MockOverloadableFunction(hasConstraints: false);
        var classWithOverloads = CreateClassWithOverloadedFunction([mockFunction1, mockFunction2]);

        // Act
        collector.CollectFromClass(classWithOverloads);

        // Assert
        collector.FunctionGroups.Should().HaveCount(1);
        var group = collector.FunctionGroups.Values.First();
        group.Overloads.Should().HaveCount(2);
        group.Name.Should().Be("testFunc");
    }

    [Fact]
    public void CollectFromModule_WithoutOverloadedFunctions_ShouldNotCreateGroups()
    {
        // Arrange
        var collector = new OverloadCollector();
        var module = new ModuleDefBuilder()
            .WithOriginalModuleName("TestModule")
            .WithNamespaceDecl(NamespaceName.From("Test"))
            .WithClasses([])
            .WithFunctions([])
            .WithVisibility(Visibility.Public)
            .Build();

        // Act
        collector.CollectFromModule(module);

        // Assert
        collector.FunctionGroups.Should().BeEmpty();
    }

    private ModuleDef CreateModuleWithFunction(MockOverloadableFunction function)
    {
        var functionDef = new FunctionDefBuilder()
            .WithName(MemberName.From(function.Name.Value))
            .WithParams(function.Params)
            .WithBody(function.Body)
            .WithReturnType(function.ReturnType)
            .WithIsStatic(false)
            .WithVisibility(Visibility.Public)
            .Build();

        return new ModuleDefBuilder()
            .WithOriginalModuleName("TestModule")
            .WithNamespaceDecl(NamespaceName.From("Test"))
            .WithClasses([])
            .WithFunctions([functionDef])
            .WithVisibility(Visibility.Public)
            .Build();
    }

    private ModuleDef CreateModuleWithOverloadedFunction(IOverloadableFunction[] overloads)
    {
        var overloadedFunction = new OverloadedFunctionDefBuilder()
            .WithName(MemberName.From("testFunc"))
            .WithOverloadClauses(overloads.ToList())
            .WithParams([])
            .WithBody(new BlockStatement { Statements = [] })
            .WithReturnType(new ast_model.TypeSystem.FifthType.TDotnetType(typeof(int)) { Name = ast_model.TypeSystem.TypeName.From("Int32") })
            .WithIsStatic(false)
            .WithIsConstructor(false)
            .WithVisibility(Visibility.Public)
            .Build();

        return new ModuleDefBuilder()
            .WithOriginalModuleName("TestModule")
            .WithNamespaceDecl(NamespaceName.From("Test"))
            .WithClasses([])
            .WithFunctions([overloadedFunction])
            .WithVisibility(Visibility.Public)
            .Build();
    }

    private ClassDef CreateClassWithOverloadedFunction(IOverloadableFunction[] overloads)
    {
        var overloadedFunction = new OverloadedFunctionDefinitionBuilder()
            .WithName(MemberName.From("testFunc"))
            .WithOverloadClauses(overloads.ToList())
            .WithTypeName(ast_model.TypeSystem.TypeName.From("void"))
            .WithIsReadOnly(false)
            .WithVisibility(Visibility.Public)
            .Build();

        return new ClassDefBuilder()
            .WithName(TypeName.From("TestClass"))
            .WithMemberDefs([overloadedFunction])
            .WithVisibility(Visibility.Public)
            .Build();
    }
}