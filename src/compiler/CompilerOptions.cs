namespace compiler;

/// <summary>
/// Immutable configuration options for the compiler
/// </summary>
/// <param name="Command">The command to execute</param>
/// <param name="Sources">Source file path(s). All files are equal; the one with main() is the entry point.</param>
/// <param name="Output">Output executable path</param>
/// <param name="Args">Arguments to pass to the program when running</param>
/// <param name="KeepTemp">Whether to keep temporary files</param>
/// <param name="Diagnostics">Whether to emit diagnostic information</param>
public record CompilerOptions(
    CompilerCommand Command = CompilerCommand.Build,
    string[] Sources = null!,
    string Output = "",
    string[] Args = null!,
    bool KeepTemp = false,
    bool Diagnostics = false)
{
    /// <summary>
    /// Create default options
    /// </summary>
    public CompilerOptions() : this(CompilerCommand.Build, Array.Empty<string>(), "", Array.Empty<string>(), false, false)
    {
    }

    /// <summary>
    /// Validate that the options are complete and consistent
    /// </summary>
    /// <returns>Validation error message, or null if valid</returns>
    public string? Validate()
    {
        if (Command != CompilerCommand.Help && (Sources == null || Sources.Length == 0))
        {
            return "At least one source file must be specified";
        }

        if ((Command == CompilerCommand.Build || Command == CompilerCommand.Run) && string.IsNullOrWhiteSpace(Output))
        {
            return "Output path must be specified for build and run commands";
        }

        // Validate all source files exist
        if (Sources != null)
        {
            foreach (var source in Sources)
            {
                if (!string.IsNullOrWhiteSpace(source) && !File.Exists(source) && !Directory.Exists(source))
                {
                    return $"Source file does not exist: {source}";
                }
            }
        }

        return null;
    }
}