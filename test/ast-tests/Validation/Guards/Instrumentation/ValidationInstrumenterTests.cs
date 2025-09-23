using FluentAssertions;
using TUnit.Core;
using compiler.Validation.GuardValidation.Instrumentation;

namespace ast_tests.Validation.Guards.Instrumentation;

public class ValidationInstrumenterTests
{
    [Test]
    public void Reset_ShouldClearAllMetrics()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        instrumenter.StartPhase("test");
        instrumenter.EndPhase("test");
        instrumenter.RecordFunctionGroupMetrics(5, 10);

        // Act
        instrumenter.Reset();

        // Assert
        var metrics = instrumenter.GetMetrics();
        metrics.TotalTime.Should().Be(TimeSpan.Zero);
        metrics.FunctionGroupsProcessed.Should().Be(0);
        metrics.OverloadsAnalyzed.Should().Be(0);
        instrumenter.PhaseTimings.Should().BeEmpty();
    }

    [Test]
    public void StartPhase_ShouldBeginTiming()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();

        // Act
        instrumenter.StartPhase("testPhase");
        Thread.Sleep(10);
        instrumenter.EndPhase("testPhase");

        // Assert
        var metrics = instrumenter.GetMetrics();
        metrics.TotalTime.Should().BeGreaterThan(TimeSpan.Zero);
        instrumenter.PhaseTimings.Should().ContainKey("testPhase");
        instrumenter.PhaseTimings["testPhase"].Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Test]
    public void EndPhase_ShouldRecordElapsedTime()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();
        instrumenter.StartPhase("testPhase");
        Thread.Sleep(10);

        // Act
        instrumenter.EndPhase("testPhase");

        // Assert
        var metrics = instrumenter.GetMetrics();
        metrics.TotalTime.Should().BeGreaterThan(TimeSpan.Zero);
        instrumenter.PhaseTimings.Should().ContainKey("testPhase");
    }

    [Test]
    public void RecordFunctionGroupMetrics_ShouldUpdateCounts()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();

        // Act
        instrumenter.RecordFunctionGroupMetrics(3, 7);

        // Assert
        var metrics = instrumenter.GetMetrics();
        metrics.FunctionGroupsProcessed.Should().Be(3);
        metrics.OverloadsAnalyzed.Should().Be(7);
    }

    [Test]
    public void EndPhase_WithoutStart_ShouldNotThrow()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();

        // Act & Assert
        var act = () => instrumenter.EndPhase("nonExistentPhase");
        act.Should().NotThrow();
    }

    [Test]
    public void MultiplePhases_ShouldTrackSeparately()
    {
        // Arrange
        var instrumenter = new ValidationInstrumenter();

        // Act
        instrumenter.StartPhase("phase1");
        Thread.Sleep(5);
        instrumenter.EndPhase("phase1");

        instrumenter.StartPhase("phase2");
        Thread.Sleep(5);
        instrumenter.EndPhase("phase2");

        instrumenter.RecordFunctionGroupMetrics(2, 4);

        // Assert
        var metrics = instrumenter.GetMetrics();
        metrics.FunctionGroupsProcessed.Should().Be(2);
        metrics.OverloadsAnalyzed.Should().Be(4);
        instrumenter.PhaseTimings.Should().ContainKey("phase1");
        instrumenter.PhaseTimings.Should().ContainKey("phase2");
        metrics.PhaseTimings.Should().ContainKey("phase1");
        metrics.PhaseTimings.Should().ContainKey("phase2");
    }
}