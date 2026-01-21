using FluentAssertions;

namespace runtime_integration_tests;

public class NamespaceImportShadowingTests : RuntimeTestBase
{
    [Fact]
    public async Task LocalSymbols_ShouldShadowImportedSymbols()
    {
        var moduleDir = Path.Combine(TempDirectory, "shadowing");
        await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "math.5th",
            """
            namespace Utilities.Math;
            export add(a: int, b: int): int {
                return a + b;
            }
            """);
        await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "consumer.5th",
            """
            namespace App.Core;
            import Utilities.Math;

            export add(a: int, b: int): int {
                return a - b;
            }

            main(): int {
                return add(3, 2);
            }
            """);

        var outputFile = Path.Combine(TempDirectory, "shadowing.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(moduleDir, outputFile, diagnostics: true);
        result.Success.Should().BeTrue("local shadowing should still allow compilation");

        var exec = await ExecuteAsync(outputFile);
        exec.ExitCode.Should().Be(1, "local add should shadow imported add within the consumer module");
        exec.StandardError.Should().BeEmpty();
    }
}
