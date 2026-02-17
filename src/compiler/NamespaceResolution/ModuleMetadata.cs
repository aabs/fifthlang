using ast;
using ast_model.Symbols;

namespace compiler.NamespaceResolution;

public record ModuleMetadata(
    string ModulePath,
    string DeclaredNamespace,
    ModuleDef Module,
    IReadOnlyList<NamespaceImportDirective> Imports,
    IReadOnlyList<ISymbolTableEntry> Declarations)
{
    public bool IsGlobalNamespace => string.IsNullOrWhiteSpace(DeclaredNamespace);

    public static ModuleMetadata FromModule(ModuleDef module)
    {
        var modulePath = module.Annotations != null
            && module.Annotations.TryGetValue(ModuleAnnotationKeys.ModulePath, out var pathObj)
                ? pathObj?.ToString() ?? module.OriginalModuleName
                : module.OriginalModuleName;

        var declaredNamespace = module.NamespaceDecl.ToString() ?? string.Empty;
        if (string.Equals(declaredNamespace, "anonymous", StringComparison.OrdinalIgnoreCase))
        {
            declaredNamespace = string.Empty;
        }

        var imports = module.Annotations != null
            && module.Annotations.TryGetValue(ModuleAnnotationKeys.ImportDirectives, out var importsObj)
            && importsObj is IReadOnlyList<NamespaceImportDirective> importList
                ? importList
                : module.Annotations != null
                    && module.Annotations.TryGetValue(ModuleAnnotationKeys.ImportDirectives, out var listObj)
                    && listObj is List<NamespaceImportDirective> importList2
                        ? importList2
                        : Array.Empty<NamespaceImportDirective>();

        var declarations = module.SymbolTable?.All().ToList() ?? new List<ISymbolTableEntry>();

        return new ModuleMetadata(
            modulePath,
            declaredNamespace,
            module,
            imports,
            declarations);
    }
}
