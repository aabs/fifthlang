using FluentAssertions;
using Xunit;
using ast;
using compiler.Validation.GuardValidation.Infrastructure;

namespace ast_tests.Validation.Guards.Infrastructure;

public class PredicateDescriptorTests
{
    [Fact]
    public void Always_ShouldHaveBaseTypeAndEmptyConstraints()
    {
        // Act
        var descriptor = PredicateDescriptor.Always;

        // Assert
        descriptor.Type.Should().Be(PredicateType.Base);
        descriptor.Constraints.Should().BeEmpty();
    }

    [Fact]
    public void Unknown_ShouldHaveUnknownTypeAndEmptyConstraints()
    {
        // Act
        var descriptor = PredicateDescriptor.Unknown;

        // Assert
        descriptor.Type.Should().Be(PredicateType.Unknown);
        descriptor.Constraints.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetTypeAndConstraints()
    {
        // Arrange
        var constraints = new List<Expression>
        {
            new BooleanLiteralExp { Value = true },
            new BooleanLiteralExp { Value = false }
        };

        // Act
        var descriptor = new PredicateDescriptor(PredicateType.Analyzable, constraints);

        // Assert
        descriptor.Type.Should().Be(PredicateType.Analyzable);
        descriptor.Constraints.Should().HaveCount(2);
        descriptor.Constraints.Should().ContainInOrder(constraints);
    }
}