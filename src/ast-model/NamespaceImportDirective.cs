namespace ast;

public record NamespaceImportDirective(string Namespace, SourceLocationMetadata Location);

public static class ModuleAnnotationKeys
{
    public const string ImportDirectives = "ImportDirectives";
    public const string ModulePath = "ModulePath";
}
