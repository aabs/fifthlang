using FluentAssertions;
using System.Threading.Tasks;
using compiler;

namespace kg_smoke_tests;

public class KG_SmokeTests
{
    [Fact]
    public async Task KG_CreateGraph_And_Run_ShouldExitZero()
    {
        var src = """
            main(): int {
                KG.CreateGraph();
                return 0;
            }
            """;

        var compiler = new Compiler();
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var srcPath = Path.Combine(tempDir, "kg_run_smoke.5th");
        var outPath = Path.Combine(tempDir, "kg_run_smoke.exe");
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

        // Compiler always outputs .dll files (cross-platform)
        var dllPath = Path.ChangeExtension(outPath, ".dll");
        File.Exists(dllPath).Should().BeTrue();

        // Generate runtimeconfig.json to run with 'dotnet'
        var runtimeConfigPath = Path.ChangeExtension(dllPath, ".runtimeconfig.json");
        var runtimeConfigJson = """
                {
                    "runtimeOptions": {
                        "tfm": "net8.0",
                        "framework": {
                            "name": "Microsoft.NETCore.App",
                            "version": "8.0.0"
                        }
                    }
                }
                """;
        await File.WriteAllTextAsync(runtimeConfigPath, runtimeConfigJson);

        // Copy dependencies next to the dll (Fifth.System and dotNetRDF)
        // Resolve repo root (where fifthlang.sln lives)
        static string FindRepoRoot()
        {
            var dir = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "fifthlang.sln"))) return dir;
                dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
            }
            return Directory.GetCurrentDirectory();
        }
        var repoRoot = FindRepoRoot();
        var compilerAsm = typeof(Compiler).Assembly.Location;
        var tfmDir = Path.GetDirectoryName(compilerAsm)!; // .../net8.0
        var configDir = Path.GetDirectoryName(tfmDir)!;    // .../Debug
        var tfm = Path.GetFileName(tfmDir);
        var config = Path.GetFileName(configDir);
        var systemBin = Path.Combine(repoRoot, "src", "fifthlang.system", "bin", config, tfm);
        if (Directory.Exists(systemBin))
        {
            foreach (var dll in Directory.EnumerateFiles(systemBin, "*.dll"))
            {
                var dest = Path.Combine(Path.GetDirectoryName(dllPath)!, Path.GetFileName(dll));
                try { File.Copy(dll, dest, overwrite: true); } catch { /* ignore */ }
            }
        }

        // Execute with explicit deps/probing to resolve dotNetRDF
        var systemDeps = Path.Combine(systemBin, "Fifth.System.deps.json");
        var nugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"exec --depsfile \"{systemDeps}\" --runtimeconfig \"{runtimeConfigPath}\" --additionalprobingpath \"{nugetCache}\" \"{dllPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(dllPath)!
        };
        using var p = System.Diagnostics.Process.Start(psi)!;
        var stdOut = await p.StandardOutput.ReadToEndAsync();
        var stdErr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        p.ExitCode.Should().Be(0, $"stdout: {stdOut}\nstderr: {stdErr}");
    }
    [Fact]
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

        // Compiler always outputs .dll files (cross-platform)
        var dllPath = Path.ChangeExtension(outPath, ".dll");
        File.Exists(dllPath).Should().BeTrue();
    }

    [Fact]
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

        // Compiler always outputs .dll files (cross-platform)
        var dllPath = Path.ChangeExtension(outPath, ".dll");
        File.Exists(dllPath).Should().BeTrue();
    }

    [Fact]
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

        // Compiler always outputs .dll files (cross-platform)
        var dllPath = Path.ChangeExtension(outPath, ".dll");
        File.Exists(dllPath).Should().BeTrue();
    }

    [Fact]
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

        // Compiler always outputs .dll files (cross-platform)
        var dllPath = Path.ChangeExtension(outPath, ".dll");
        File.Exists(dllPath).Should().BeTrue();
    }

    [Fact]
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

        // Compiler always outputs .dll files (cross-platform)
        var dllPath = Path.ChangeExtension(outPath, ".dll");
        File.Exists(dllPath).Should().BeTrue();
    }
}
