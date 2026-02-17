using FluentAssertions;
using Xunit;
using ast;
using compiler.Validation.GuardValidation.Analysis;
using compiler.Validation.GuardValidation.Infrastructure;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Analysis;

public class CompletenessAnalyzerTests
{
    [Fact]
    public void CheckForUnreachableOverloads_WithBaseFollowedByGuarded_ShouldDetectUnreachable()
    {
        // Arrange
        var analyzer = new CompletenessAnalyzer();
        var baseOverload = new MockOverloadableFunction(hasConstraints: false);
        var guardedOverload = new MockOverloadableFunction(hasConstraints: true);

        var analyzedOverloads = new List<AnalyzedOverload>
        {
            new(baseOverload, PredicateType.Base, PredicateDescriptor.Always),
            new(guardedOverload, PredicateType.Analyzable, PredicateDescriptor.Unknown)
        };

        // Act
        var unreachableOverloads = analyzer.CheckForUnreachableOverloads(analyzedOverloads);

        // Assert
        unreachableOverloads.Should().HaveCount(1);
        var (unreachableIndex, coveringIndex, unreachable, covering) = unreachableOverloads[0];
        unreachableIndex.Should().Be(2);
        coveringIndex.Should().Be(1);
        unreachable.Overload.Should().Be(guardedOverload);
        covering.Overload.Should().Be(baseOverload);
    }

    [Fact]
    public void CheckForUnreachableOverloads_WithOnlyGuardedOverloads_ShouldNotDetectUnreachable()
    {
        // Arrange
        var analyzer = new CompletenessAnalyzer();
        var guardedOverload1 = new MockOverloadableFunction(hasConstraints: true);
        var guardedOverload2 = new MockOverloadableFunction(hasConstraints: true);

        var analyzedOverloads = new List<AnalyzedOverload>
        {
            new(guardedOverload1, PredicateType.Analyzable, PredicateDescriptor.Unknown),
            new(guardedOverload2, PredicateType.Analyzable, PredicateDescriptor.Unknown)
        };

        // Act
        var unreachableOverloads = analyzer.CheckForUnreachableOverloads(analyzedOverloads);

        // Assert
        unreachableOverloads.Should().BeEmpty();
    }

    [Fact]
    public void IsComplete_WithAnalyzableAndNoUnknown_ShouldReturnFalse()
    {
        // Arrange
        var analyzer = new CompletenessAnalyzer();
        var group = new FunctionGroup("testFunc", 1);
        var analyzedOverloads = new List<AnalyzedOverload>
        {
            new(new MockOverloadableFunction(), PredicateType.Analyzable, PredicateDescriptor.Unknown)
        };

        // Act
        var result = analyzer.IsComplete(group, analyzedOverloads);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_WithUnknownPredicates_ShouldReturnFalse()
    {
        // Arrange
        var analyzer = new CompletenessAnalyzer();
        var group = new FunctionGroup("testFunc", 1);
        var analyzedOverloads = new List<AnalyzedOverload>
        {
            new(new MockOverloadableFunction(), PredicateType.Unknown, PredicateDescriptor.Unknown)
        };

        // Act
        var result = analyzer.IsComplete(group, analyzedOverloads);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateBaseOrdering_WithBaseNotLast_ShouldReturnInvalidIndex()
    {
        // Arrange
        var analyzer = new CompletenessAnalyzer();
        var baseOverload = new MockOverloadableFunction(hasConstraints: false);
        var guardedOverload = new MockOverloadableFunction(hasConstraints: true);

        var group = new FunctionGroup("testFunc", 1);
        group.AddOverload(baseOverload);
        group.AddOverload(guardedOverload);

        // Act
        var result = analyzer.ValidateBaseOrdering(group, baseOverload);

        // Assert
        result.Should().Be(2); // 1-based index of invalid subsequent overload
    }

    [Fact]
    public void ValidateBaseOrdering_WithBaseLast_ShouldReturnNull()
    {
        // Arrange
        var analyzer = new CompletenessAnalyzer();
        var guardedOverload = new MockOverloadableFunction(hasConstraints: true);
        var baseOverload = new MockOverloadableFunction(hasConstraints: false);

        var group = new FunctionGroup("testFunc", 1);
        group.AddOverload(guardedOverload);
        group.AddOverload(baseOverload);

        // Act
        var result = analyzer.ValidateBaseOrdering(group, baseOverload);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateUnknownPercentage_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var analyzer = new CompletenessAnalyzer();
        var analyzedOverloads = new List<AnalyzedOverload>
        {
            new(new MockOverloadableFunction(), PredicateType.Unknown, PredicateDescriptor.Unknown),
            new(new MockOverloadableFunction(), PredicateType.Analyzable, PredicateDescriptor.Unknown),
            new(new MockOverloadableFunction(), PredicateType.Unknown, PredicateDescriptor.Unknown),
            new(new MockOverloadableFunction(), PredicateType.Base, PredicateDescriptor.Always)
        };

        // Act
        var result = analyzer.CalculateUnknownPercentage(analyzedOverloads);

        // Assert
        result.Should().Be(50); // 2 out of 4 = 50%
    }
}