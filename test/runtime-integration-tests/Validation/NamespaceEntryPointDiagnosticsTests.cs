using compiler;
using FluentAssertions;

namespace runtime_integration_tests.Validation;

public class NamespaceEntryPointDiagnosticsTests : RuntimeTestBase
{
    [Fact]
    public async Task MissingMainAcrossModules_ShouldReportDiagnostic()
    {
        var moduleDir = Path.Combine(TempDirectory, "no_main");
        var fileA = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "alpha.5th",
            """
            namespace Alpha;
            export add(int a, int b): int => a + b;
            """);
        var fileB = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "beta.5th",
            """
            namespace Beta;
            export sub(int a, int b): int => a - b;
            """);

        var outputFile = Path.Combine(TempDirectory, "no_main.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(moduleDir, outputFile, diagnostics: true);

        result.Success.Should().BeFalse("builds without a main function must fail");
        result.Diagnostics.Should().Contain(d => d.Level == DiagnosticLevel.Error
            && d.Message.Contains("main", StringComparison.OrdinalIgnoreCase));
        result.Diagnostics.Should().Contain(d => d.Message.Contains(Path.GetFileName(fileA))
            && d.Message.Contains(Path.GetFileName(fileB)));
    }

    [Fact]
    public async Task MultipleMainsAcrossModules_ShouldReportDiagnostic()
    {
        var moduleDir = Path.Combine(TempDirectory, "multi_main");
        var fileA = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "alpha.5th",
            """
            namespace Alpha;
            main(): int { return 1; }
            """);
        var fileB = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "beta.5th",
            """
            namespace Beta;
            main(): int { return 2; }
            """);

        var outputFile = Path.Combine(TempDirectory, "multi_main.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(moduleDir, outputFile, diagnostics: true);

        result.Success.Should().BeFalse("multiple main functions must fail");
        result.Diagnostics.Should().Contain(d => d.Level == DiagnosticLevel.Error
            && d.Message.Contains("main", StringComparison.OrdinalIgnoreCase));
        result.Diagnostics.Should().Contain(d => d.Message.Contains(Path.GetFileName(fileA))
            && d.Message.Contains(Path.GetFileName(fileB)));
    }
}
