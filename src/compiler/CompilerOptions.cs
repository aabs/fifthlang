namespace compiler;

/// <summary>
/// Immutable configuration options for the compiler
/// </summary>
/// <param name="Command">The command to execute</param>
/// <param name="Source">Source file or directory path</param>
/// <param name="Output">Output executable path</param>
/// <param name="OutputType">Output type: Exe or Library</param>
/// <param name="Args">Arguments to pass to the program when running</param>
/// <param name="KeepTemp">Whether to keep temporary files</param>
/// <param name="Diagnostics">Whether to emit diagnostic information</param>
public record CompilerOptions(
    CompilerCommand Command = CompilerCommand.Build,
    string Source = "",
    string Output = "",
    string OutputType = "Exe",
    string[] Args = null!,
    bool KeepTemp = false,
    bool Diagnostics = false,
    IReadOnlyList<string>? SourceFiles = null,
    string? SourceManifest = null,
    IReadOnlyList<string>? References = null)
{
    /// <summary>
    /// Create default options
    /// </summary>
    public CompilerOptions() : this(CompilerCommand.Build, "", "", "Exe", Array.Empty<string>(), false, false, Array.Empty<string>(), null, Array.Empty<string>())
    {
    }

    /// <summary>
    /// Validate that the options are complete and consistent
    /// </summary>
    /// <returns>Validation error message, or null if valid</returns>
    public string? Validate()
    {
        if (Command != CompilerCommand.Help
            && (SourceFiles == null || SourceFiles.Count == 0)
            && string.IsNullOrWhiteSpace(Source)
            && string.IsNullOrWhiteSpace(SourceManifest))
        {
            return "Source file or directory must be specified";
        }

        if ((Command == CompilerCommand.Build || Command == CompilerCommand.Run) && string.IsNullOrWhiteSpace(Output))
        {
            return "Output path must be specified for build and run commands";
        }

        if (!string.IsNullOrWhiteSpace(OutputType)
            && !OutputType.Equals("Exe", StringComparison.OrdinalIgnoreCase)
            && !OutputType.Equals("Library", StringComparison.OrdinalIgnoreCase))
        {
            return "Output type must be Exe or Library";
        }

        if (Command == CompilerCommand.Run && OutputType.Equals("Library", StringComparison.OrdinalIgnoreCase))
        {
            return "Run command is not supported for Library output";
        }

        if (!string.IsNullOrWhiteSpace(SourceManifest) && !File.Exists(SourceManifest))
        {
            return $"Source manifest does not exist: {SourceManifest}";
        }

        if (SourceFiles != null && SourceFiles.Count > 0)
        {
            var missing = SourceFiles.FirstOrDefault(path => !File.Exists(path));
            if (!string.IsNullOrWhiteSpace(missing))
            {
                return $"Source path does not exist: {missing}";
            }

            return null;
        }

        if (!string.IsNullOrWhiteSpace(Source) && !File.Exists(Source) && !Directory.Exists(Source))
        {
            return $"Source path does not exist: {Source}";
        }

        return null;
    }
}