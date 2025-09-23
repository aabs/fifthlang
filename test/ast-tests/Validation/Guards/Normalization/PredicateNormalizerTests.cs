using FluentAssertions;
using TUnit.Core;
using ast;
using compiler.Validation.GuardValidation.Normalization;
using compiler.Validation.GuardValidation.Infrastructure;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Normalization;

public class PredicateNormalizerTests
{
    [Test]
    public void ClassifyPredicate_WithNoConstraints_ShouldReturnBase()
    {
        // Arrange
        var normalizer = new PredicateNormalizer();
        var mockFunction = new MockOverloadableFunction(hasConstraints: false);

        // Act
        var result = normalizer.ClassifyPredicate(mockFunction);

        // Assert
        result.Should().Be(PredicateType.Base);
    }

    [Test]
    public void ClassifyPredicate_WithTautology_ShouldReturnBase()
    {
        // Arrange
        var normalizer = new PredicateNormalizer();
        var mockFunction = new MockOverloadableFunction(hasConstraints: false)
            .WithConstraint(new BooleanLiteralExp { Value = true });

        // Act
        var result = normalizer.ClassifyPredicate(mockFunction);

        // Assert
        result.Should().Be(PredicateType.Base);
    }

    [Test]
    public void ClassifyPredicate_WithAnalyzableConstraint_ShouldReturnAnalyzable()
    {
        // Arrange
        var normalizer = new PredicateNormalizer();
        var equalityConstraint = new BinaryExp
        {
            Operator = Operator.Equal,
            LHS = new VarRefExp { Name = Identifier.From("x") },
            RHS = new Int32LiteralExp { Value = 42 }
        };
        var mockFunction = new MockOverloadableFunction(hasConstraints: false)
            .WithConstraint(equalityConstraint);

        // Act
        var result = normalizer.ClassifyPredicate(mockFunction);

        // Assert
        result.Should().Be(PredicateType.Analyzable);
    }

    [Test]
    public void ClassifyPredicate_WithUnknownConstraint_ShouldReturnUnknown()
    {
        // Arrange
        var normalizer = new PredicateNormalizer();
        var unknownConstraint = new FuncCallExp
        {
            FunctionDef = new VarRefExp { Name = Identifier.From("complexFunction") },
            InvocationArguments = []
        };
        var mockFunction = new MockOverloadableFunction(hasConstraints: false)
            .WithConstraint(unknownConstraint);

        // Act
        var result = normalizer.ClassifyPredicate(mockFunction);

        // Assert
        result.Should().Be(PredicateType.Unknown);
    }

    [Test]
    public void AnalyzeOverload_ShouldCreateAnalyzedOverloadWithCorrectClassification()
    {
        // Arrange
        var normalizer = new PredicateNormalizer();
        var mockFunction = new MockOverloadableFunction(hasConstraints: false);

        // Act
        var result = normalizer.AnalyzeOverload(mockFunction);

        // Assert
        result.Overload.Should().Be(mockFunction);
        result.PredicateType.Should().Be(PredicateType.Base);
        result.PredicateDescriptor.Should().Be(PredicateDescriptor.Always);
    }

    [Test]
    public void CreatePredicateDescriptor_WithBaseType_ShouldReturnAlways()
    {
        // Arrange
        var normalizer = new PredicateNormalizer();
        var mockFunction = new MockOverloadableFunction();

        // Act
        var result = normalizer.CreatePredicateDescriptor(mockFunction, PredicateType.Base);

        // Assert
        result.Should().Be(PredicateDescriptor.Always);
    }

    [Test]
    public void CreatePredicateDescriptor_WithUnknownType_ShouldReturnUnknown()
    {
        // Arrange
        var normalizer = new PredicateNormalizer();
        var mockFunction = new MockOverloadableFunction();

        // Act
        var result = normalizer.CreatePredicateDescriptor(mockFunction, PredicateType.Unknown);

        // Assert
        result.Should().Be(PredicateDescriptor.Unknown);
    }
}