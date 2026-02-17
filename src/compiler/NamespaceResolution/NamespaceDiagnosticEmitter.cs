namespace compiler.NamespaceResolution;

public sealed class NamespaceDiagnosticEmitter
{
    private readonly List<Diagnostic> _diagnostics;

    public NamespaceDiagnosticEmitter(List<Diagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public void EmitDuplicateSymbol(ModuleMetadata module, NamespaceScope scope, string symbolName, string existingModulePath)
    {
        var message = $"Duplicate symbol '{symbolName}' in namespace '{scope.Name}' across modules '{existingModulePath}' and '{module.ModulePath}'.";
        _diagnostics.Add(new Diagnostic(
            DiagnosticLevel.Error,
            message,
            Source: module.ModulePath,
            Code: null,
            Namespace: scope.Name));
    }

    public void EmitUndeclaredImport(ModuleMetadata module, NamespaceImportDirective directive)
    {
        var location = directive.Location;
        var line = location.Line;
        var column = location.Column;
        if (line <= 0) line = 1;
        if (column < 0) column = 0;

        var message = $"Import targets undeclared namespace: '{directive.Namespace}'";
        _diagnostics.Add(new Diagnostic(
            DiagnosticLevel.Warning,
            message,
            Source: module.ModulePath,
            Code: "WNS0001",
            Namespace: directive.Namespace,
            Line: line,
            Column: column + 1));
    }
}
