using System.IO;
using FluentAssertions;
using TUnit;

namespace runtime_integration_tests;

public class PocSampleAvailabilityTests
{
    [Test]
    public void RoslynPoc_Sample_Is_Copied_To_Output()
    {
        // The test project MSI copies TestPrograms/**/*.5th to output
        var cwd = Directory.GetCurrentDirectory();
        var expected = Path.Combine(cwd, "roslyn-poc-simple.5th");
        File.Exists(expected).Should().BeTrue("POC sample should be present in test output so integration tests can consume it at runtime");
    }
}
