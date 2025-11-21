using System.Diagnostics;
using System.Runtime.InteropServices;
using compiler;
using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Base class for runtime integration tests that provides common functionality
/// for compiling and executing Fifth language programs
/// </summary>
public abstract class RuntimeTestBase : IDisposable
{
    protected readonly string TempDirectory;
    protected readonly List<string> GeneratedFiles = new();
    protected readonly List<string> GeneratedDirectories = new();
    // When true, keep generated temp directories for post-mortem debugging.
    protected readonly bool KeepTemp;

    protected RuntimeTestBase()
    {
        // Create a unique temporary directory for this test
        TempDirectory = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(TempDirectory);
        GeneratedDirectories.Add(TempDirectory);

        // Determine whether to keep temporary folders for debugging.
        // Controlled by environment variable FIFTH_KEEP_TEMP=true or presence of marker file at repo root: KEEP_FIFTH_TEMP
        var env = Environment.GetEnvironmentVariable("FIFTH_KEEP_TEMP");
        if (!string.IsNullOrEmpty(env) && env.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            KeepTemp = true;
        }
        else
        {
            // Check for marker file in repository root
            try
            {
                var repoRoot = FindRepoRoot();
                if (!string.IsNullOrEmpty(repoRoot))
                {
                    var marker = Path.Combine(repoRoot, "KEEP_FIFTH_TEMP");
                    KeepTemp = File.Exists(marker);
                }
            }
            catch
            {
                KeepTemp = false;
            }
        }
    }

    // Locate the repository root by walking upwards until the solution file is found
    private static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "fifthlang.sln"))) return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }
        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Compile a Fifth source code string to an executable
    /// </summary>
    /// <param name="sourceCode">The Fifth source code</param>
    /// <param name="fileName">Optional file name (without extension)</param>
    /// <returns>Path to the generated executable</returns>
    protected async Task<string> CompileSourceAsync(string sourceCode, string? fileName = null)
    {
        fileName ??= $"test_{Guid.NewGuid():N}";
        var sourceFile = Path.Combine(TempDirectory, $"{fileName}.5th");
        // Compiler always outputs .dll files regardless of requested extension
        var outputFile = Path.Combine(TempDirectory, $"{fileName}.dll");

        await File.WriteAllTextAsync(sourceFile, sourceCode);
        GeneratedFiles.Add(sourceFile);
        GeneratedFiles.Add(outputFile);

        var compiler = new Compiler();
        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: sourceFile,
            Output: outputFile,
            Diagnostics: true);

        var result = await compiler.CompileAsync(options);

        if (!result.Success)
        {
            var diagnosticsText = string.Join("\n", result.Diagnostics.Select(d => $"{d.Level}: {d.Message}"));
            throw new InvalidOperationException($"Compilation failed for source:\n{sourceCode}\n\nDiagnostics:\n{diagnosticsText}");
        }

        File.Exists(outputFile).Should().BeTrue("Executable should be created");

        // Generate runtime configuration file for .NET applications
        await GenerateRuntimeConfigAsync(outputFile);

        return outputFile;
    }

    /// <summary>
    /// Compile a Fifth source file to an executable
    /// </summary>
    /// <param name="sourceFilePath">Path to the .5th source file</param>
    /// <param name="outputFileName">Optional output file name (without extension)</param>
    /// <returns>Path to the generated executable</returns>
    protected async Task<string> CompileFileAsync(string sourceFilePath, string? outputFileName = null)
    {
        File.Exists(sourceFilePath).Should().BeTrue($"Source file should exist: {sourceFilePath}");

        outputFileName ??= Path.GetFileNameWithoutExtension(sourceFilePath);
        // Compiler always outputs .dll files regardless of requested extension
        var outputFile = Path.Combine(TempDirectory, $"{outputFileName}.dll");
        GeneratedFiles.Add(outputFile);

        var compiler = new Compiler();
        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: sourceFilePath,
            Output: outputFile,
            Diagnostics: false);

        var result = await compiler.CompileAsync(options);

        if (!result.Success)
        {
            var diagnosticsText = string.Join("\n", result.Diagnostics.Select(d => $"{d.Level}: {d.Message}"));
            throw new InvalidOperationException($"Compilation failed for file: {sourceFilePath}\n\nDiagnostics:\n{diagnosticsText}");
        }
        File.Exists(outputFile).Should().BeTrue("Executable should be created");

        // Generate runtime configuration file for .NET applications
        await GenerateRuntimeConfigAsync(outputFile);

        return outputFile;
    }

    /// <summary>
    /// Generate runtime configuration file for .NET executable
    /// </summary>
    /// <param name="executablePath">Path to the .exe file</param>
    private async Task GenerateRuntimeConfigAsync(string executablePath)
    {
        var runtimeConfigPath = Path.ChangeExtension(executablePath, "runtimeconfig.json");
        var runtimeConfig = new
        {
            runtimeOptions = new
            {
                tfm = "net8.0",
                framework = new
                {
                    name = "Microsoft.NETCore.App",
                    version = "8.0.0"
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(runtimeConfig, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(runtimeConfigPath, json);
        GeneratedFiles.Add(runtimeConfigPath);
    }

    /// <summary>
    /// Execute a compiled program and return the result
    /// </summary>
    /// <param name="executablePath">Path to the executable</param>
    /// <param name="arguments">Optional command line arguments</param>
    /// <param name="input">Optional standard input</param>
    /// <param name="timeoutMs">Timeout in milliseconds (default 10 seconds)</param>
    /// <returns>Execution result</returns>
    protected async Task<ExecutionResult> ExecuteAsync(string executablePath, string? arguments = null, string? input = null, int timeoutMs = 60000)
    {
        File.Exists(executablePath).Should().BeTrue($"Executable should exist: {executablePath}");

        // Prepare deps/probing so runtime can resolve Fifth.System and transitive packages
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

        var exeDir = Path.GetDirectoryName(executablePath)!;
        var runtimeConfigPath = Path.ChangeExtension(executablePath, "runtimeconfig.json");
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
                var dest = Path.Combine(exeDir, Path.GetFileName(dll));
                try { File.Copy(dll, dest, overwrite: true); } catch { /* ignore */ }
            }
        }

        // Copy dotNetRdf dependencies from NuGet cache
        var nugetPackages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        var dotNetRdfDll = Path.Combine(nugetPackages, "dotnetrdf.core", "3.4.0", "lib", "netstandard2.0", "dotNetRdf.dll");
        var vdsCommonDll = Path.Combine(nugetPackages, "vds.common", "3.0.0", "lib", "netstandard2.0", "VDS.Common.dll");
        
        if (File.Exists(dotNetRdfDll))
        {
            try { File.Copy(dotNetRdfDll, Path.Combine(exeDir, "dotNetRdf.dll"), overwrite: true); } catch { /* ignore */ }
        }
        if (File.Exists(vdsCommonDll))
        {
            try { File.Copy(vdsCommonDll, Path.Combine(exeDir, "VDS.Common.dll"), overwrite: true); } catch { /* ignore */ }
        }

        var systemDeps = Path.Combine(systemBin, "Fifth.System.deps.json");
        // To avoid cross-test contention on a shared deps.json during parallel runs,
        // copy the deps file into the per-test execution directory and use that local copy.
        string depsFileToUse = systemDeps;
        try
        {
            if (File.Exists(systemDeps))
            {
                var localDeps = Path.Combine(exeDir, Path.GetFileName(systemDeps));
                // Best-effort copy; ignore if fails as the global path will still be used
                try { File.Copy(systemDeps, localDeps, overwrite: true); depsFileToUse = localDeps; } catch { /* ignore */ }
            }
        }
        catch { /* ignore */ }
        var nugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        var startInfo = new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet" : "dotnet",
            Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? $"exec --depsfile \"{depsFileToUse}\" --runtimeconfig \"{runtimeConfigPath}\" --additionalprobingpath \"{nugetCache}\" \"{executablePath}\" {arguments ?? string.Empty}".Trim()
                : $"exec --depsfile \"{depsFileToUse}\" --runtimeconfig \"{runtimeConfigPath}\" --additionalprobingpath \"{nugetCache}\" \"{executablePath}\" {arguments ?? string.Empty}".Trim(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = input != null,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = exeDir
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new List<string>();
        var error = new List<string>();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) output.Add(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.Add(e.Data); };

        var startTime = DateTime.UtcNow;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (input != null)
        {
            await process.StandardInput.WriteAsync(input);
            process.StandardInput.Close();
        }

        try
        {
            await process.WaitForExitAsync(CancellationToken.None).WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
        }
        catch (TimeoutException)
        {
            try { process.Kill(); } catch { /* Ignore */ }
            throw new TimeoutException($"Process timed out after {timeoutMs}ms");
        }

        var endTime = DateTime.UtcNow;

        return new ExecutionResult(
            ExitCode: process.ExitCode,
            StandardOutput: string.Join(Environment.NewLine, output),
            StandardError: string.Join(Environment.NewLine, error),
            ElapsedTime: endTime - startTime
        );
    }

    /// <summary>
    /// Get the command to run an executable (platform-specific)
    /// </summary>
    private static string GetExecutableCommand(string executablePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return executablePath;
        }
        else
        {
            return "dotnet";
        }
    }

    /// <summary>
    /// Get the arguments to run an executable (platform-specific)
    /// </summary>
    private static string GetExecutableArguments(string executablePath, string? additionalArgs)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return additionalArgs ?? "";
        }
        else
        {
            var args = executablePath;
            if (!string.IsNullOrEmpty(additionalArgs))
            {
                args += " " + additionalArgs;
            }
            return args;
        }
    }

    /// <summary>
    /// Cleanup generated files and directories
    /// </summary>
    public void Dispose()
    {
        // Clean up generated files
        if (KeepTemp)
        {
            // Preserve files/directories for post-mortem. Report location on stderr for easier inspection.
            try
            {
                Console.Error.WriteLine($"[RuntimeTestBase] Preserving temp directory for debugging: {TempDirectory}");
            }
            catch { }
            return;
        }
        foreach (var file in GeneratedFiles)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        // Clean up generated directories
        foreach (var directory in GeneratedDirectories)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

/// <summary>
/// Result of executing a compiled program
/// </summary>
/// <param name="ExitCode">Process exit code</param>
/// <param name="StandardOutput">Standard output content</param>
/// <param name="StandardError">Standard error content</param>
/// <param name="ElapsedTime">Execution time</param>
public record ExecutionResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    TimeSpan ElapsedTime);