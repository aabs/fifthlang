using FluentAssertions;
using System.Threading.Tasks;
using compiler;

namespace kg_smoke_tests;

public class KG_Negative_SmokeTests
{
    [Test]
    public async Task SaveGraph_NoArguments_ShouldFailToCompile()
    {
        var src = """
            main(): int {
                KG.SaveGraph();
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_savegraph_noargs_neg.5th");
        var outPath = Path.Combine(tempDir, "kg_savegraph_noargs_neg.exe");
        await File.WriteAllTextAsync(srcPath, src);

        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: srcPath,
            Output: outPath,
            Args: Array.Empty<string>(),
            KeepTemp: false,
            Diagnostics: true);
        var result = await compiler.CompileAsync(options);

        result.Success.Should().BeFalse("Calling SaveGraph with no args should not resolve an overload");
    }

    [Test]
    public async Task SaveGraph_WrongSecondArgType_ShouldFailToCompile()
    {
        var src = """
            main(): int {
                KG.SaveGraph(KG.CreateStore(), 123);
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_savegraph_badarg_neg.5th");
        var outPath = Path.Combine(tempDir, "kg_savegraph_badarg_neg.exe");
        await File.WriteAllTextAsync(srcPath, src);

        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: srcPath,
            Output: outPath,
            Args: Array.Empty<string>(),
            KeepTemp: false,
            Diagnostics: true);
        var result = await compiler.CompileAsync(options);

        result.Success.Should().BeFalse("Second argument must be an IGraph; int should fail");
    }

    [Test]
    public async Task CreateTriple_WithStrings_ShouldFailToCompile()
    {
        var src = """
            main(): int {
                KG.CreateTriple("s", "p", "o");
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_createtriple_strings_neg.5th");
        var outPath = Path.Combine(tempDir, "kg_createtriple_strings_neg.exe");
        await File.WriteAllTextAsync(srcPath, src);

        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: srcPath,
            Output: outPath,
            Args: Array.Empty<string>(),
            KeepTemp: false,
            Diagnostics: true);
        var result = await compiler.CompileAsync(options);

        result.Success.Should().BeFalse("CreateTriple requires INode arguments; strings should fail");
    }

    [Test]
    public async Task SyntaxError_MissingParen_ShouldFailToCompile()
    {
        var src = """
            main(): int {
                KG.SaveGraph(KG.CreateStore(), KG.CreateGraph();
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_syntax_missing_paren_neg.5th");
        var outPath = Path.Combine(tempDir, "kg_syntax_missing_paren_neg.exe");
        await File.WriteAllTextAsync(srcPath, src);

        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: srcPath,
            Output: outPath,
            Args: Array.Empty<string>(),
            KeepTemp: false,
            Diagnostics: true);
        var result = await compiler.CompileAsync(options);

        result.Success.Should().BeFalse("Parser should reject missing parenthesis");
    }
}
