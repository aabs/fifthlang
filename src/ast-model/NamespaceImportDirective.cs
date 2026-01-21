namespace ast;

public record NamespaceImportDirective : AstThing
{
    public string Namespace { get; init; } = string.Empty;
    public new SourceLocationMetadata Location { get; init; }
}

public static class ModuleAnnotationKeys
{
    public const string ImportDirectives = "ImportDirectives";
    public const string ModulePath = "ModulePath";
    public const string ResolvedImports = "ResolvedImports";
}
