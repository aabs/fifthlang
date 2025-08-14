namespace compiler;

/// <summary>
/// Represents the available compiler commands
/// </summary>
public enum CompilerCommand
{
    /// <summary>
    /// Build command: Parse, transform, generate IL, and assemble to executable
    /// </summary>
    Build,
    
    /// <summary>
    /// Run command: Same as build, then execute the produced binary
    /// </summary>
    Run,
    
    /// <summary>
    /// Lint command: Parse and apply transformations only, report issues
    /// </summary>
    Lint,
    
    /// <summary>
    /// Help command: Display usage information
    /// </summary>
    Help
}