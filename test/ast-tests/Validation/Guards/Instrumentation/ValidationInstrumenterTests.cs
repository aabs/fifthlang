using FluentAssertions;
using TUnit.Core;
using compiler.Validation.GuardValidation.Instrumentation;
using compiler.Validation.GuardValidation.Infrastructure;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Instrumentation;

public class ValidationInstrumenterTests
{
    [Test]
    public void Reset_ShouldClearAllMetrics()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        var group = new FunctionGroup("testFunc", 1);
        instrumenter.RecordGroupStart(group);
        instrumenter.RecordGroupCompletion(group);

        // Act
        instrumenter.Reset();

        // Assert
        instrumenter.Metrics.Should().BeEmpty();
    }

    [Test]
    public void RecordGroupStart_ShouldCreateNewMetric()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        var group = new FunctionGroup("testFunc", 2);

        // Act
        instrumenter.RecordGroupStart(group);

        // Assert
        instrumenter.Metrics.Should().HaveCount(1);
        var metric = instrumenter.Metrics[0];
        metric.FunctionName.Should().Be("testFunc");
        metric.Arity.Should().Be(2);
        metric.OverloadCount.Should().Be(0); // Initially no overloads
        metric.ElapsedMs.Should().BeNull(); // Not completed yet
    }

    [Test]
    public void RecordGroupCompletion_ShouldUpdateElapsedTime()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        var group = new FunctionGroup("testFunc", 1);
        instrumenter.RecordGroupStart(group);

        // Simulate some processing time
        Thread.Sleep(10);

        // Act
        instrumenter.RecordGroupCompletion(group);

        // Assert
        var metric = instrumenter.Metrics[0];
        metric.ElapsedMs.Should().NotBeNull();
        metric.ElapsedMs.Should().BeGreaterThan(0);
    }

    [Test]
    public void RecordOverloadCollected_ShouldIncrementOverloadCount()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        var group = new FunctionGroup("testFunc", 1);
        var overload = new MockOverloadableFunction();
        instrumenter.RecordGroupStart(group);

        // Act
        instrumenter.RecordOverloadCollected(group, overload);
        instrumenter.RecordOverloadCollected(group, overload);
        instrumenter.RecordOverloadCollected(group, overload);

        // Assert
        var metric = instrumenter.Metrics[0];
        metric.OverloadCount.Should().Be(3);
    }

    [Test]
    public void RecordGroupCompletion_WithoutStart_ShouldNotThrow()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        var group = new FunctionGroup("testFunc", 1);

        // Act & Assert
        var act = () => instrumenter.RecordGroupCompletion(group);
        act.Should().NotThrow();
    }

    [Test]
    public void MultipleGroups_ShouldTrackSeparately()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        var group1 = new FunctionGroup("func1", 1);
        var group2 = new FunctionGroup("func2", 2);

        // Act
        instrumenter.RecordGroupStart(group1);
        instrumenter.RecordGroupStart(group2);
        instrumenter.RecordOverloadCollected(group1, new MockOverloadableFunction());
        instrumenter.RecordOverloadCollected(group2, new MockOverloadableFunction());
        instrumenter.RecordOverloadCollected(group2, new MockOverloadableFunction());
        instrumenter.RecordGroupCompletion(group1);
        instrumenter.RecordGroupCompletion(group2);

        // Assert
        instrumenter.Metrics.Should().HaveCount(2);

        var metric1 = instrumenter.Metrics.First(m => m.FunctionName == "func1");
        metric1.OverloadCount.Should().Be(1);
        metric1.ElapsedMs.Should().NotBeNull();

        var metric2 = instrumenter.Metrics.First(m => m.FunctionName == "func2");
        metric2.OverloadCount.Should().Be(2);
        metric2.ElapsedMs.Should().NotBeNull();
    }
}