using TUnit.Core;
using FluentAssertions;

namespace ast_tests.Validation.Guards.Determinism;

public class DeterminismHashTests
{
    [Test]
    public void DiagnosticsSignature_ShouldBeDeterministicAcrossTwoRuns()
    {
        false.Should().BeTrue("T027: determinism hash test not implemented yet");
    }
}
