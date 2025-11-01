namespace compiler;

/// <summary>
/// Immutable configuration options for the compiler
/// </summary>
/// <param name="Command">The command to execute</param>
/// <param name="Source">Source file or directory path (primary source)</param>
/// <param name="AdditionalSources">Additional source files for multi-file compilation</param>
/// <param name="Output">Output executable path</param>
/// <param name="Args">Arguments to pass to the program when running</param>
/// <param name="KeepTemp">Whether to keep temporary files</param>
/// <param name="Diagnostics">Whether to emit diagnostic information</param>
public record CompilerOptions(
    CompilerCommand Command = CompilerCommand.Build,
    string Source = "",
    string[] AdditionalSources = null!,
    string Output = "",
    string[] Args = null!,
    bool KeepTemp = false,
    bool Diagnostics = false)
{
    /// <summary>
    /// Create default options
    /// </summary>
    public CompilerOptions() : this(CompilerCommand.Build, "", Array.Empty<string>(), "", Array.Empty<string>(), false, false)
    {
    }

    /// <summary>
    /// Get all source files (primary + additional)
    /// </summary>
    public IEnumerable<string> GetAllSourceFiles()
    {
        var sources = new List<string>();
        if (!string.IsNullOrWhiteSpace(Source))
        {
            sources.Add(Source);
        }
        if (AdditionalSources != null && AdditionalSources.Length > 0)
        {
            sources.AddRange(AdditionalSources);
        }
        return sources;
    }

    /// <summary>
    /// Validate that the options are complete and consistent
    /// </summary>
    /// <returns>Validation error message, or null if valid</returns>
    public string? Validate()
    {
        if (Command != CompilerCommand.Help && string.IsNullOrWhiteSpace(Source))
        {
            return "Source file or directory must be specified";
        }

        if ((Command == CompilerCommand.Build || Command == CompilerCommand.Run) && string.IsNullOrWhiteSpace(Output))
        {
            return "Output path must be specified for build and run commands";
        }

        if (!string.IsNullOrWhiteSpace(Source) && !File.Exists(Source) && !Directory.Exists(Source))
        {
            return $"Source path does not exist: {Source}";
        }

        // Validate additional sources if provided
        if (AdditionalSources != null)
        {
            foreach (var additionalSource in AdditionalSources)
            {
                if (!string.IsNullOrWhiteSpace(additionalSource) && !File.Exists(additionalSource))
                {
                    return $"Additional source file does not exist: {additionalSource}";
                }
            }
        }

        return null;
    }
}