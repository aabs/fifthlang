namespace compiler;

/// <summary>
/// Represents the available compiler backends for code generation
/// </summary>
public enum CompilerBackend
{
    /// <summary>
    /// Legacy backend: Uses custom IL generation and PEEmitter
    /// </summary>
    Legacy,
    
    /// <summary>
    /// Roslyn backend: Uses Roslyn's C# compilation for code generation
    /// </summary>
    Roslyn
}
