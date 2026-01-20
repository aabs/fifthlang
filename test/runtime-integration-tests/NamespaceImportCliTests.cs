using System.Reflection;
using FluentAssertions;

namespace runtime_integration_tests;

public class NamespaceImportCliTests : RuntimeTestBase
{
    [Fact]
    public async Task CliShouldAcceptMultipleSourceFiles()
    {
        var moduleDir = Path.Combine(TempDirectory, "cli_modules");
        var fileA = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "math.5th",
            """
            namespace Utilities.Math;
            export add(int a, int b): int => a + b;
            """);
        var fileB = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "consumer.5th",
            """
            namespace App.Core;
            import Utilities.Math;

            main(): int {
                return add(2, 3);
            }
            """);

        var outputFile = Path.Combine(TempDirectory, "cli_multi.dll");
        GeneratedFiles.Add(outputFile);

        var args = new[]
        {
            "--command", "build",
            "--output", outputFile,
            "--source", fileA, fileB
        };

        var exitCode = await InvokeCompilerAsync(args);

        exitCode.Should().Be(0, "CLI should accept multiple source files and compile successfully");
        File.Exists(outputFile).Should().BeTrue("compiler should emit output for multi-file builds");
    }

    private static async Task<int> InvokeCompilerAsync(string[] args)
    {
        var assembly = typeof(compiler.Compiler).Assembly;
        var programType = assembly.GetType("Program");
        programType.Should().NotBeNull("compiler Program type should exist");

        var main = programType!.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        main.Should().NotBeNull("compiler Program should expose a Main entry point");

        var result = main!.Invoke(null, new object?[] { args });

        if (result is Task<int> taskWithCode)
        {
            return await taskWithCode;
        }

        if (result is Task task)
        {
            await task;
            return 0;
        }

        return result is int code ? code : 0;
    }
}
