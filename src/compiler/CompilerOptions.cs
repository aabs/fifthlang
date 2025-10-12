namespace compiler;

/// <summary>
/// Immutable configuration options for the compiler
/// </summary>
/// <param name="Command">The command to execute</param>
/// <param name="Source">Source file or directory path</param>
/// <param name="Output">Output executable path</param>
/// <param name="Args">Arguments to pass to the program when running</param>
/// <param name="KeepTemp">Whether to keep temporary IL files</param>
/// <param name="Diagnostics">Whether to emit diagnostic information</param>
/// <param name="Backend">Backend to use for code generation (legacy or roslyn)</param>
public record CompilerOptions(
    CompilerCommand Command = CompilerCommand.Build,
    string Source = "",
    string Output = "",
    string[] Args = null!,
    bool KeepTemp = false,
    bool Diagnostics = false,
    CompilerBackend Backend = CompilerBackend.Legacy)
{
    /// <summary>
    /// Create default options
    /// </summary>
    public CompilerOptions() : this(CompilerCommand.Build, "", "", Array.Empty<string>(), false, false, CompilerBackend.Legacy)
    {
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

        return null;
    }
}