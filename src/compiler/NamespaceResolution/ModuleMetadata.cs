using ast;

namespace compiler.NamespaceResolution;

/// <summary>
/// Represents metadata for a single .5th module participating in compilation.
/// Captures the module's file path, declared namespace (if any), import directives, and top-level declarations.
/// </summary>
public record ModuleMetadata
{
    /// <summary>
    /// Absolute path to the .5th source file
    /// </summary>
    public required string Path { get; init; }
    
    /// <summary>
    /// The namespace declared by this module, or null if the module belongs to the global namespace.
    /// Corresponds to "namespace Foo.Bar.Baz;" declarations.
    /// </summary>
    public string? DeclaredNamespace { get; init; }
    
    /// <summary>
    /// List of namespace names imported by this module via "import" directives.
    /// These imports are file-scoped and make the imported namespace's symbols available in this module only.
    /// </summary>
    public List<string> Imports { get; init; } = new();
    
    /// <summary>
    /// Top-level declarations (functions, classes, etc.) defined in this module.
    /// These contribute to the module's declared namespace (or global if no namespace is declared).
    /// </summary>
    public List<IAstThing> Declarations { get; init; } = new();
    
    /// <summary>
    /// The parsed AST root for this module (the 'fifth' rule result from the parser).
    /// </summary>
    public IAstThing? ParsedRoot { get; init; }
}
