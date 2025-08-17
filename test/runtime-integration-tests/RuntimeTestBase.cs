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

    protected RuntimeTestBase()
    {
        // Create a unique temporary directory for this test
        TempDirectory = Path.Combine(Path.GetTempPath(), $"FifthRuntime_{Guid.NewGuid():N}");
        Directory.CreateDirectory(TempDirectory);
        GeneratedDirectories.Add(TempDirectory);
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
        var outputFile = Path.Combine(TempDirectory, $"{fileName}.exe");

        await File.WriteAllTextAsync(sourceFile, sourceCode);
        GeneratedFiles.Add(sourceFile);
        GeneratedFiles.Add(outputFile);

        var compiler = new Compiler();
        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: sourceFile,
            Output: outputFile,
            UseDirectPEEmission: true,
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
        var outputFile = Path.Combine(TempDirectory, $"{outputFileName}.exe");
        GeneratedFiles.Add(outputFile);

        var compiler = new Compiler();
        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: sourceFilePath,
            Output: outputFile,
            UseDirectPEEmission: true,
            Diagnostics: false);

        var result = await compiler.CompileAsync(options);

        result.Success.Should().BeTrue($"Compilation should succeed for file: {sourceFilePath}\n\nDiagnostics:\n{string.Join("\n", result.Diagnostics.Select(d => $"{d.Level}: {d.Message}"))}");
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
    protected async Task<ExecutionResult> ExecuteAsync(string executablePath, string? arguments = null, string? input = null, int timeoutMs = 10000)
    {
        File.Exists(executablePath).Should().BeTrue($"Executable should exist: {executablePath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = GetExecutableCommand(executablePath),
            Arguments = GetExecutableArguments(executablePath, arguments),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = input != null,
            UseShellExecute = false,
            CreateNoWindow = true
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