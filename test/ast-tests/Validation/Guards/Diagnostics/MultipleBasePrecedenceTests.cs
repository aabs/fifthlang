using TUnit.Core;
using FluentAssertions;

namespace ast_tests.Validation.Guards.Diagnostics;

public class MultipleBasePrecedenceTests
{
    [Test]
    public void MultipleBase_ShouldSuppressE1001()
    {
        false.Should().BeTrue("T022: multiple base precedence not implemented yet");
    }
}
