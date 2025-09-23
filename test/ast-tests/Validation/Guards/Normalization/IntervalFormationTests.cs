using FluentAssertions;
using TUnit.Core;
using compiler.Validation.GuardValidation.Analysis;

namespace ast_tests.Validation.Guards.Normalization;

public class IntervalFormationTests
{
    [Test]
    public void Intersect_ClosedIntervals_ShouldReturnOverlap()
    {
        var eng = new IntervalEngine();
        var a = Interval.Closed(1, 10);
        var b = Interval.Closed(5, 15);
        var result = eng.Intersect(a, b);
        // Expected [5,10]
        // Will fail until implemented
        result.Min.Should().Be(5);
        result.MinInclusive.Should().BeTrue();
        result.Max.Should().Be(10);
        result.MaxInclusive.Should().BeTrue();
    }
}
