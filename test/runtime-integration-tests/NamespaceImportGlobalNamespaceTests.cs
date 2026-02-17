using FluentAssertions;

namespace runtime_integration_tests;

public class NamespaceImportGlobalNamespaceTests : RuntimeTestBase
{
    [Fact]
    public async Task GlobalModules_ShouldRemainGlobalWhenImportingNamespaces()
    {
        var moduleDir = Path.Combine(TempDirectory, "global_namespace");
        await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "math.5th",
            """
            namespace Utilities.Math;
            export add(a: int, b: int): int {
                return a + b;
            }
            """);
        await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "global_ops.5th",
            """
            import Utilities.Math;

            export calc(): int {
                return add(2, 3);
            }
            """);
        await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "main.5th",
            """
            main(): int {
                return calc();
            }
            """);

        var outputFile = Path.Combine(TempDirectory, "global_namespace.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(moduleDir, outputFile, diagnostics: true);
        result.Success.Should().BeTrue("global modules should remain in the global namespace");

        var exec = await ExecuteAsync(outputFile);
        exec.ExitCode.Should().Be(5, "global calc should be visible without namespace qualification");
        exec.StandardError.Should().BeEmpty();
    }
}
