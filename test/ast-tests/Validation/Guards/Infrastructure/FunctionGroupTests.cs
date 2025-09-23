using FluentAssertions;
using TUnit.Core;
using compiler.Validation.GuardValidation.Infrastructure;

namespace ast_tests.Validation.Guards.Infrastructure;

public class FunctionGroupTests
{
    [Test]
    public void Constructor_ShouldSetNameAndArity()
    {
        // Arrange & Act
        var group = new FunctionGroup("testFunc", 3);

        // Assert
        group.Name.Should().Be("testFunc");
        group.Arity.Should().Be(3);
        group.Overloads.Should().BeEmpty();
    }

    [Test]
    public void HasAnyGuards_WithNoOverloads_ShouldReturnFalse()
    {
        // Arrange
        var group = new FunctionGroup("testFunc", 0);

        // Act & Assert
        group.HasAnyGuards().Should().BeFalse();
    }

    [Test]
    public void HasAnyGuards_WithUnguardedOverloads_ShouldReturnFalse()
    {
        // Arrange
        var group = new FunctionGroup("testFunc", 1);
        var mockOverload = new MockOverloadableFunction(hasConstraints: false);
        group.AddOverload(mockOverload);

        // Act & Assert
        group.HasAnyGuards().Should().BeFalse();
    }

    [Test]
    public void HasAnyGuards_WithGuardedOverloads_ShouldReturnTrue()
    {
        // Arrange
        var group = new FunctionGroup("testFunc", 1);
        var mockOverload = new MockOverloadableFunction(hasConstraints: true);
        group.AddOverload(mockOverload);

        // Act & Assert
        group.HasAnyGuards().Should().BeTrue();
    }

    [Test]
    public void GetBaseOverloads_ShouldReturnOnlyUnguardedOverloads()
    {
        // Arrange
        var group = new FunctionGroup("testFunc", 1);
        var guardedOverload = new MockOverloadableFunction(hasConstraints: true);
        var baseOverload1 = new MockOverloadableFunction(hasConstraints: false);
        var baseOverload2 = new MockOverloadableFunction(hasConstraints: false);

        group.AddOverload(guardedOverload);
        group.AddOverload(baseOverload1);
        group.AddOverload(baseOverload2);

        // Act
        var baseOverloads = group.GetBaseOverloads();

        // Assert
        baseOverloads.Should().HaveCount(2);
        baseOverloads.Should().Contain(baseOverload1);
        baseOverloads.Should().Contain(baseOverload2);
        baseOverloads.Should().NotContain(guardedOverload);
    }
}