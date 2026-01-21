namespace compiler;

/// <summary>
/// Represents the available compiler commands
/// </summary>
public enum CompilerCommand
{
    /// <summary>
    /// Build command: Parse, transform, and compile to executable. Supports multi-file inputs and MSBuild source manifests.
    /// </summary>
    Build,

    /// <summary>
    /// Run command: Same as build (including multi-file inputs), then execute the produced binary
    /// </summary>
    Run,

    /// <summary>
    /// Lint command: Parse and apply transformations only, report issues across all provided modules
    /// </summary>
    Lint,

    /// <summary>
    /// Help command: Display usage information
    /// </summary>
    Help
}