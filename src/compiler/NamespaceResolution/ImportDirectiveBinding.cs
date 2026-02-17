namespace compiler.NamespaceResolution;

public record ImportDirectiveBinding(
    ModuleMetadata Module,
    string NamespaceName,
    NamespaceScope? ResolvedScope);
