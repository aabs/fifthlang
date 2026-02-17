using compiler;
using FluentAssertions;

namespace runtime_integration_tests.Validation;

public class NamespaceDuplicateSymbolTests : RuntimeTestBase
{
    [Fact]
    public async Task DuplicateSymbolsAcrossModules_ShouldReportDiagnostics()
    {
        var moduleDir = Path.Combine(TempDirectory, "dup_symbols");
        var fileA = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "math_a.5th",
            """
            namespace Utilities.Math;
            export add(a: int, b: int): int {
                return a + b;
            }
            """);
        var fileB = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "math_b.5th",
            """
            namespace Utilities.Math;
            export add(a: int, b: int): int {
                return a + b;
            }
            """);

        var outputFile = Path.Combine(TempDirectory, "dup_symbols.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(moduleDir, outputFile, diagnostics: true);

        result.Success.Should().BeFalse("duplicate symbols in a shared namespace should fail compilation");
        result.Diagnostics.Should().NotBeEmpty();
        result.Diagnostics.Should().Contain(d => d.Level == DiagnosticLevel.Error
            && d.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
        result.Diagnostics.Should().Contain(d => d.Message.Contains(Path.GetFileName(fileA))
            && d.Message.Contains(Path.GetFileName(fileB)));
    }
}
