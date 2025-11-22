using FluentAssertions;
using System.Threading.Tasks;
using compiler;

namespace kg_smoke_tests;

public class KG_SaveGraph_SmokeTests
{
    [Fact]
    public async Task KG_SaveGraph_ShouldCompile()
    {
        var src = """
            main(): int {
                KG.SaveGraph(KG.CreateStore(), KG.CreateGraph());
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_savegraph_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_savegraph_smoke.exe");
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

        AssertEmittedDll(result, outPath);
    }

    [Fact]
    public async Task KG_SaveGraph_WithGraphUriString_ShouldCompile()
    {
        var src = """
            main(): int {
                KG.SaveGraph(KG.CreateStore(), KG.CreateGraph(), "http://example.org/graph");
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_savegraph_uri_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_savegraph_uri_smoke.exe");
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

        AssertEmittedDll(result, outPath);
    }

    [Fact]
    public async Task KG_SaveGraph_ToRemoteStore_ShouldCompile()
    {
        var src = """
            main(): int {
                KG.SaveGraph(KG.ConnectToRemoteStore("http://example.org/store"), KG.CreateGraph());
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_savegraph_remote_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_savegraph_remote_smoke.exe");
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

        AssertEmittedDll(result, outPath);
    }

    private static void AssertEmittedDll(CompilationResult result, string requestedOutput)
    {
        var dllPath = Path.ChangeExtension(requestedOutput, ".dll");
        result.OutputPath.Should().Be(dllPath);
        File.Exists(dllPath).Should().BeTrue();
    }
}
