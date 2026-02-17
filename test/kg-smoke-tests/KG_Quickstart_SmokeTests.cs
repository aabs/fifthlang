using FluentAssertions;
using System.Threading.Tasks;
using compiler;

namespace kg_smoke_tests;

public class KG_Quickstart_SmokeTests
{
    [Fact]
    public async Task Quickstart_Connect_Assert_Save_ShouldCompile()
    {
        var src = """
            main(): int {
                KG.SaveGraph(
                    KG.ConnectToRemoteStore("http://example.org/store"),
                    KG.Assert(
                        KG.CreateGraph(),
                        KG.CreateTriple(
                            KG.CreateLiteral(KG.CreateGraph(), "s"),
                            KG.CreateLiteral(KG.CreateGraph(), "p"),
                            KG.CreateLiteral(KG.CreateGraph(), "o")
                        )
                    ),
                    "http://example.org/graph"
                );
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_quickstart_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_quickstart_smoke.exe");
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

        var dllPath = Path.ChangeExtension(outPath, ".dll");
        result.OutputPath.Should().Be(dllPath);
        File.Exists(dllPath).Should().BeTrue();
    }
}
