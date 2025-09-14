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

    [Test]
    public async Task KG_Merge_Graphs_ShouldCompileWithIGraphParams()
    {
        var src = """
            main(): int {
                KG.Merge(KG.CreateGraph(), KG.CreateGraph());
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_merge_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_merge_smoke.exe");
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

    [Test]
    public async Task KG_CreateLiteral_WithOptionalLanguage_ShouldCompile()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), "hello");
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_literal_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_literal_smoke.exe");
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

    [Test]
    public async Task KG_CreateLiteral_WithExplicitLanguage_ShouldPreferStringOverload()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), "hello", "fr");
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_literal_fr_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_literal_fr_smoke.exe");
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

    [Test]
    public async Task KG_CreateTriple_And_Assert_ShouldCompile()
    {
        var src = """
            main(): int {
                KG.Assert(
                    KG.CreateGraph(),
                    KG.CreateTriple(
                        KG.CreateLiteral(KG.CreateGraph(), "s"),
                        KG.CreateLiteral(KG.CreateGraph(), "p"),
                        KG.CreateLiteral(KG.CreateGraph(), "o")
                    )
                );
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_assert_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_assert_smoke.exe");
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
