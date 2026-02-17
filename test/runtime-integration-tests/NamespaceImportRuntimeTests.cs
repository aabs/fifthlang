using FluentAssertions;

namespace runtime_integration_tests;

public class NamespaceImportRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task NamespaceImports_ShouldResolveAndExecute()
    {
        var programDir = Path.Combine("TestPrograms", "NamespaceImports");
        var outputFile = Path.Combine(TempDirectory, "namespace_imports.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(programDir, outputFile, diagnostics: true);

        result.Success.Should().BeTrue("namespace imports should resolve successfully");
        result.Diagnostics.Should().NotContain(d => d.Level == compiler.DiagnosticLevel.Error);

        var exec = await ExecuteAsync(outputFile);
        exec.ExitCode.Should().Be(5, "main should return 5 when add(2, 3) is imported");
        exec.StandardError.Should().BeEmpty();
    }
}
