using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;
using compiler;

namespace runtime_integration_tests.Performance;

public class NamespaceImportPerformanceTests : RuntimeTestBase
{
    [Fact]
    public async Task NamespaceImportResolution_ShouldCompleteWithinTwoSeconds()
    {
        const int moduleCount = 40;
        const int functionsPerModule = 5;
        var sourceFiles = new List<string>();

        for (int i = 0; i < moduleCount; i++)
        {
            var source = BuildModuleSource(i, functionsPerModule);
            var path = Path.Combine(TempDirectory, $"perf_module_{i}.5th");
            await File.WriteAllTextAsync(path, source);
            GeneratedFiles.Add(path);
            sourceFiles.Add(path);
        }

        var consumerPath = Path.Combine(TempDirectory, "perf_consumer.5th");
        await File.WriteAllTextAsync(consumerPath, BuildConsumerSource(moduleCount));
        GeneratedFiles.Add(consumerPath);
        sourceFiles.Add(consumerPath);

        var outputPath = Path.Combine(TempDirectory, "perf_namespace.dll");
        GeneratedFiles.Add(outputPath);

        var fifthCompiler = new Compiler();
        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: string.Empty,
            Output: outputPath,
            Diagnostics: true,
            SourceFiles: sourceFiles);

        var result = await fifthCompiler.CompileAsync(options);
        result.Success.Should().BeTrue("namespace import performance compile should succeed");

        var timing = result.Diagnostics
            .Where(d => d.Level == DiagnosticLevel.Info)
            .Select(d => d.Message)
            .FirstOrDefault(m => m.StartsWith("Namespace resolution time:", StringComparison.Ordinal));

        timing.Should().NotBeNull("namespace resolution timing should be emitted under diagnostics");

        var match = Regex.Match(timing!, @"Namespace resolution time:\s*(\d+)ms");
        match.Success.Should().BeTrue($"unexpected timing format: {timing}");

        var elapsed = long.Parse(match.Groups[1].Value);
        elapsed.Should().BeLessOrEqualTo(2000, "namespace resolution should complete within 2 seconds");
    }

    private static string BuildModuleSource(int index, int functionsPerModule)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace perf;");
        sb.AppendLine();

        for (int i = 0; i < functionsPerModule; i++)
        {
            sb.AppendLine($"export add_{index}_{i}(a: int, b: int): int {{");
            sb.AppendLine("    return a + b;");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string BuildConsumerSource(int moduleCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("import perf;");
        sb.AppendLine();
        sb.AppendLine("main(): int {");
        sb.AppendLine("    total: int = 0;");

        for (int i = 0; i < Math.Min(5, moduleCount); i++)
        {
            sb.AppendLine($"    total = total + add_{i}_0(2, 3);");
        }

        sb.AppendLine("    return total;");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
