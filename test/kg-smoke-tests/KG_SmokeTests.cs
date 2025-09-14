using FluentAssertions;
using System.Threading.Tasks;
using compiler;

namespace kg_smoke_tests;

public class KG_SmokeTests
{
    [Test]
    public async Task KG_CreateGraph_And_ConnectToRemoteStore_ShouldCompileAndRun()
    {
        var src = """
            main(): int {
                if (KG.CreateGraph() == null) { return 1; }
                if (KG.ConnectToRemoteStore("http://example.org/store") == null) { return 1; }
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_smoke.exe");
        await File.WriteAllTextAsync(srcPath, src);

        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: srcPath,
            Output: outPath,
            Args: Array.Empty<string>(),
            KeepTemp: false,
            Diagnostics: true);
        var result = await compiler.CompileAsync(options);
        result.Success.Should().BeTrue($"Compilation should succeed. Diagnostics:\n{string.Join("\n", result.Diagnostics.Select(d => $"{d.Level}: {d.Message}"))}");

        File.Exists(outPath).Should().BeTrue();
    }
}
