using FluentAssertions;
using Xunit;
using compiler.Validation.GuardValidation.Analysis;

namespace ast_tests.Validation.Guards.Analysis;

public class IntervalSubsumptionTests
{
    [Fact]
    public void Subsumes_ClosedWithinClosed_ShouldBeTrue()
    {
        var eng = new IntervalEngine();
        var outer = Interval.Closed(1, 10);
        var inner = Interval.Closed(3, 7);
        eng.Subsumes(outer, inner).Should().BeTrue();
    }

    [Fact]
    public void Subsumes_PartialOverlap_ShouldBeFalse()
    {
        var eng = new IntervalEngine();
        var a = Interval.Closed(1, 5);
        var b = Interval.Closed(4, 10);
        eng.Subsumes(a, b).Should().BeFalse();
    }
}
