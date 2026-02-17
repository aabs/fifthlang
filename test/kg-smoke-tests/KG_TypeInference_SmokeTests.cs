using System;
using System.Threading.Tasks;
using FluentAssertions;
using compiler;

namespace kg_smoke_tests;

public class KG_TypeInference_SmokeTests
{
    [Fact]
    public async Task NumericPromotion_IntPlusDouble_ShouldPreferDoubleOverload()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), 1 + 2.0);
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

    [Fact]
    public async Task StringConcat_Plus_ShouldPreferStringOverload()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), "abc" + 123);
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

    [Fact]
    public async Task ParamTyped_Var_To_Extcall_ShouldResolveByParamType()
    {
        var src = """
            foo(x: int): int {
                KG.CreateLiteral(KG.CreateGraph(), x);
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

    [Fact]
    public async Task NumericPromotion_Chain_IntAndDouble_ShouldPreferDoubleOverload()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), 1 + 2.0 + 3);
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_num_chain.5th");
        var outPath = Path.Combine(tempDir, "kg_num_chain.exe");
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

    [Fact]
    public async Task UnaryNegation_ShouldPreserveDoubleType()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), -(1.5));
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_unary_neg.5th");
        var outPath = Path.Combine(tempDir, "kg_unary_neg.exe");
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

    [Fact]
    public async Task StringConcat_Chain_ShouldPreferStringOverload()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), "a" + 1 + "b");
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_str_chain.5th");
        var outPath = Path.Combine(tempDir, "kg_str_chain.exe");
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

    [Fact]
    public async Task BoolEquality_ShouldInferBoolType_ForOverloadResolution()
    {
        var src = """
            main(): int {
                KG.CreateLiteral(KG.CreateGraph(), 1 == 1);
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_bool_eq.5th");
        var outPath = Path.Combine(tempDir, "kg_bool_eq.exe");
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

    [Fact]
    public async Task LogicalNot_OnComparison_ShouldCompile()
    {
        var src = """
            main(): int {
                if (!(1 < 2)) { return 1; }
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_bool_not.5th");
        var outPath = Path.Combine(tempDir, "kg_bool_not.exe");
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

    [Fact]
    public async Task ParamTyped_Float_AddInt_ShouldPreferFloatOverload()
    {
        var src = """
            foo(x: float): int {
                KG.CreateLiteral(KG.CreateGraph(), x + 1);
                return 0;
            }
            main(): int { return foo(1.25); }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_param_float_add.5th");
        var outPath = Path.Combine(tempDir, "kg_param_float_add.exe");
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

    [Fact]
    public async Task StringConcat_WithParam_ShouldPreferStringOverload()
    {
        var src = """
            foo(x: string): int {
                KG.CreateLiteral(KG.CreateGraph(), "a" + x);
                return 0;
            }
            main(): int { return foo("b"); }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_str_param_concat.5th");
        var outPath = Path.Combine(tempDir, "kg_str_param_concat.exe");
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

    [Fact]
    public async Task NestedFuncCall_IntAddition_FlowsTypeToCallee()
    {
        var src = """
            bar(x: int): int {
                KG.CreateLiteral(KG.CreateGraph(), x);
                return 0;
            }
            foo(y: int): int {
                return bar(y + 1);
            }
            main(): int { return foo(41); }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_nested_calls.5th");
        var outPath = Path.Combine(tempDir, "kg_nested_calls.exe");
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
