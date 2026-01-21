using FluentAssertions;

namespace runtime_integration_tests;

public class NamespaceImportIdempotencyTests : RuntimeTestBase
{
    [Fact]
    public async Task RepeatedImports_ShouldBeIdempotent()
    {
        var programDir = Path.Combine("TestPrograms", "NamespaceImports", "Idempotency");
        var outputFile = Path.Combine(TempDirectory, "idempotent_imports.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(programDir, outputFile, diagnostics: true);

        result.Success.Should().BeTrue("repeated imports should not cause duplicate symbol diagnostics");
        result.Diagnostics.Should().NotContain(d => d.Level == compiler.DiagnosticLevel.Error);

        var exec = await ExecuteAsync(outputFile);
        exec.ExitCode.Should().Be(5, "repeated imports should not alter resolved symbol results");
        exec.StandardError.Should().BeEmpty();
    }
}
