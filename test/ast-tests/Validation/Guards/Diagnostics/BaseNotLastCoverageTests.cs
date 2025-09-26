using TUnit.Core;
using FluentAssertions;

namespace ast_tests.Validation.Guards.Diagnostics;

public class BaseNotLastCoverageTests
{
    [Test]
    public void BaseNotLast_ShouldStillAllowE1001()
    {
        false.Should().BeTrue("T023: base-not-last coverage gating not implemented yet");
    }
}
