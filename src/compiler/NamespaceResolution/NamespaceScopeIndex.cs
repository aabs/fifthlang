using ast;

namespace compiler.NamespaceResolution;

/// <summary>
/// Aggregates symbols from all modules that share the same namespace name.
/// Maintains a unified symbol table and emits diagnostics for duplicate symbol declarations across modules.
/// </summary>
public class NamespaceScopeIndex
{
    /// <summary>
    /// The namespace name (empty string for global namespace)
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Modules that contribute declarations to this namespace
    /// </summary>
    public List<ModuleMetadata> ContributingModules { get; } = new();
    
    /// <summary>
    /// Unified symbol table merging symbols from all contributing modules.
    /// Key is the symbol name, value is the list of modules that declare it.
    /// </summary>
    public Dictionary<string, List<ModuleMetadata>> Symbols { get; } = new();
    
    /// <summary>
    /// Diagnostics emitted during namespace aggregation (e.g., duplicate symbol declarations)
    /// </summary>
    public List<Diagnostic> Diagnostics { get; } = new();
    
    public NamespaceScopeIndex(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// Add a module to this namespace scope and merge its symbols.
    /// Detects and reports duplicate symbol declarations across modules.
    /// </summary>
    public void AddModule(ModuleMetadata module)
    {
        ContributingModules.Add(module);
        
        // Extract symbol names from the module's declarations
        foreach (var declaration in module.Declarations)
        {
            var symbolName = GetSymbolName(declaration);
            if (symbolName == null) continue;
            
            if (!Symbols.ContainsKey(symbolName))
            {
                Symbols[symbolName] = new List<ModuleMetadata>();
            }
            
            Symbols[symbolName].Add(module);
            
            // Emit diagnostic if symbol is declared in multiple modules
            if (Symbols[symbolName].Count > 1)
            {
                var firstModule = Symbols[symbolName][0];
                Diagnostics.Add(new Diagnostic(
                    Level: DiagnosticLevel.Error,
                    Message: $"Symbol '{symbolName}' is declared in multiple modules in namespace '{Name}': {firstModule.Path} and {module.Path}",
                    Source: $"{module.Path}:0:0",
                    Code: "NS0001"
                ));
            }
        }
    }
    
    /// <summary>
    /// Extract the symbol name from a top-level declaration.
    /// Returns null if the declaration is not a named symbol (e.g., statements).
    /// </summary>
    private string? GetSymbolName(IAstThing declaration)
    {
        return declaration switch
        {
            FunctionDef func => func.Name.ToString(),  // MemberName.ToString() gives the string value
            // ClassDef classDef => classDef.Name.ToString(),  // TODO: Fix type incompatibility - Name is TypeName not MemberName
            _ => null
        };
    }
    
    /// <summary>
    /// Check if this namespace contains a symbol with the given name.
    /// </summary>
    public bool ContainsSymbol(string symbolName)
    {
        return Symbols.ContainsKey(symbolName);
    }
}
