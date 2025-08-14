using System.Diagnostics;

namespace compiler;

/// <summary>
/// Abstraction for running external processes
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Run a process and return the result
    /// </summary>
    /// <param name="fileName">The executable to run</param>
    /// <param name="arguments">Arguments to pass to the executable</param>
    /// <param name="workingDirectory">Working directory for the process</param>
    /// <returns>The process result</returns>
    Task<ProcessResult> RunAsync(string fileName, string arguments = "", string? workingDirectory = null);
}

/// <summary>
/// Result of running a process
/// </summary>
/// <param name="ExitCode">The process exit code</param>
/// <param name="StandardOutput">Standard output content</param>
/// <param name="StandardError">Standard error content</param>
/// <param name="ElapsedTime">Time taken to run the process</param>
public record ProcessResult(int ExitCode, string StandardOutput, string StandardError, TimeSpan ElapsedTime)
{
    /// <summary>
    /// Whether the process completed successfully
    /// </summary>
    public bool Success => ExitCode == 0;
}

/// <summary>
/// Default implementation of IProcessRunner using System.Diagnostics.Process
/// </summary>
public class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(string fileName, string arguments = "", string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        var stopwatch = Stopwatch.StartNew();
        
        using var process = new Process { StartInfo = startInfo };
        
        process.Start();
        
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        var output = await outputTask;
        var error = await errorTask;
        
        stopwatch.Stop();
        
        return new ProcessResult(process.ExitCode, output, error, stopwatch.Elapsed);
    }
}