using System;
using System.Threading.Tasks;
using FluentAssertions;
using compiler;

namespace kg_smoke_tests;

public class KG_TypeInference_SmokeTests
{
    [Test]
    public async Task NumericPromotion_IntPlusDouble_ShouldPreferDoubleOverload()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(1 + 2.0);
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_num_promote.5th");
        var outPath = Path.Combine(tempDir, "kg_num_promote.exe");
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
    }

    [Test]
    public async Task StringConcat_Plus_ShouldPreferStringOverload()
    {
        var src = """
            main(): int {
                KG.CreateLiteral("abc" + 123);
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_str_concat.5th");
        var outPath = Path.Combine(tempDir, "kg_str_concat.exe");
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
    }

    [Test]
    public async Task ParamTyped_Var_To_Extcall_ShouldResolveByParamType()
    {
        var src = """
            foo(x: int): int {
                KG.CreateLiteral(x);
                return 0;
            }
            main(): int { return foo(42); }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_param_extcall.5th");
        var outPath = Path.Combine(tempDir, "kg_param_extcall.exe");
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
    }
}
