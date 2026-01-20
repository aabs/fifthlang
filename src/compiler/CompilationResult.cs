namespace compiler;

/// <summary>
/// Represents a diagnostic message from the compilation process
/// </summary>
/// <param name="Level">The severity level of the diagnostic</param>
/// <param name="Message">The diagnostic message</param>
/// <param name="Source">Optional source information</param>
/// <param name="Code">Optional stable diagnostic code (e.g., TRPL001)</param>
public record Diagnostic(
    DiagnosticLevel Level,
    string Message,
    string? Source = null,
    string? Code = null,
    string? Namespace = null,
    int? Line = null,
    int? Column = null);

// Alias used by the translator surface for clarity in translator-specific API types.
// This intentionally reuses the existing compiler.Diagnostic shape so diagnostics
// are comparable across phases of the toolchain.
public record DiagnosticRecord(
    DiagnosticLevel Level,
    string Message,
    string? Source = null,
    string? Code = null,
    string? Namespace = null,
    int? Line = null,
    int? Column = null);

/// <summary>
/// Diagnostic severity levels
/// </summary>
public enum DiagnosticLevel
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Result of a compilation operation
/// </summary>
/// <param name="Success">Whether the compilation was successful</param>
/// <param name="ExitCode">The exit code to return</param>
/// <param name="Diagnostics">Collection of diagnostic messages</param>
/// <param name="OutputPath">Path to the generated executable (if successful)</param>
/// <param name="ILPath">Path to the generated IL file (if kept)</param>
/// <param name="ElapsedTime">Total compilation time</param>
public record CompilationResult(
    bool Success,
    int ExitCode,
    IReadOnlyList<Diagnostic> Diagnostics,
    string? OutputPath = null,
    string? ILPath = null,
    TimeSpan? ElapsedTime = null)
{
    /// <summary>
    /// Create a successful result
    /// </summary>
    public static CompilationResult Successful(string? outputPath = null, string? ilPath = null, TimeSpan? elapsed = null)
    {
        return new CompilationResult(true, 0, Array.Empty<Diagnostic>(), outputPath, ilPath, elapsed);
    }

    /// <summary>
    /// Create a successful result with diagnostics
    /// </summary>
    public static CompilationResult Successful(IEnumerable<Diagnostic> diagnostics, string? outputPath = null, string? ilPath = null, TimeSpan? elapsed = null)
    {
        return new CompilationResult(true, 0, diagnostics.ToArray(), outputPath, ilPath, elapsed);
    }

    /// <summary>
    /// Create a failed result with an error message
    /// </summary>
    public static CompilationResult Failed(int exitCode, string errorMessage, string? source = null)
    {
        var diagnostic = new Diagnostic(DiagnosticLevel.Error, errorMessage, source, null);
        return new CompilationResult(false, exitCode, new[] { diagnostic });
    }

    /// <summary>
    /// Create a failed result with multiple diagnostics
    /// </summary>
    public static CompilationResult Failed(int exitCode, IEnumerable<Diagnostic> diagnostics)
    {
        return new CompilationResult(false, exitCode, diagnostics.ToArray());
    }
}