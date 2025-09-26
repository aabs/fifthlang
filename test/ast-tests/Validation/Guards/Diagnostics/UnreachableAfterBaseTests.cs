using TUnit.Core;
using FluentAssertions;

namespace ast_tests.Validation.Guards.Diagnostics;

public class UnreachableAfterBaseTests
{
    [Test]
    public void AfterBase_AnalyzableStillWarnsUnreachable()
    {
        false.Should().BeTrue("T024: unreachable-after-base warning not implemented yet");
    }
}
