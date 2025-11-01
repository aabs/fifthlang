namespace compiler.NamespaceResolution;

/// <summary>
/// Represents a single import directive binding within a module.
/// Tracks which module issued the import, which namespace was requested, and whether resolution succeeded.
/// </summary>
public record ImportDirectiveBinding
{
    /// <summary>
    /// The module that contains this import directive
    /// </summary>
    public required ModuleMetadata OwningModule { get; init; }
    
    /// <summary>
    /// The namespace name being imported (e.g., "System.Collections.Generic")
    /// </summary>
    public required string NamespaceName { get; init; }
    
    /// <summary>
    /// The resolved namespace scope, or null if the namespace is undeclared.
    /// Null triggers a warning diagnostic (WNS0001) but allows compilation to continue.
    /// </summary>
    public NamespaceScopeIndex? ResolvedScope { get; set; }
    
    /// <summary>
    /// Line number in the source file where this import appears (1-based)
    /// </summary>
    public int Line { get; init; }
    
    /// <summary>
    /// Column number in the source file where this import appears (1-based)
    /// </summary>
    public int Column { get; init; }
}
