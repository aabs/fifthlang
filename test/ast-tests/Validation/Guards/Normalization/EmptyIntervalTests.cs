using FluentAssertions;
using Xunit;
using compiler.Validation.GuardValidation.Analysis;

namespace ast_tests.Validation.Guards.Normalization;

public class EmptyIntervalTests
{
    [Fact]
    public void IsEmpty_InvertedBounds_ShouldBeTrue()
    {
        var eng = new IntervalEngine();
        var inv = new Interval(10, true, 5, true);
        eng.IsEmpty(inv).Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_OpenTouching_ShouldBeTrue()
    {
        var eng = new IntervalEngine();
        var openTouch = new Interval(5, false, 5, false);
        eng.IsEmpty(openTouch).Should().BeTrue();
    }
}
