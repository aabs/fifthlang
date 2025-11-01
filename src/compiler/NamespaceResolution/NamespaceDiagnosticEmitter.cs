namespace compiler.NamespaceResolution;

/// <summary>
/// Emits structured diagnostics for namespace resolution issues.
/// All diagnostics include module path and namespace identifiers per the contract.
/// </summary>
public class NamespaceDiagnosticEmitter
{
    private readonly List<Diagnostic> _diagnostics = new();

    /// <summary>
    /// Get all diagnostics emitted so far.
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    /// <summary>
    /// Emit warning WNS0001: Import targets undeclared namespace.
    /// </summary>
    /// <param name="moduleFile">Source module path where the import appears</param>
    /// <param name="namespaceName">The namespace being imported</param>
    /// <param name="line">Line number of the import directive (1-based)</param>
    /// <param name="column">Column number of the import directive (1-based)</param>
    public void EmitUndeclaredNamespaceWarning(string moduleFile, string namespaceName, int line, int column)
    {
        _diagnostics.Add(new Diagnostic(
            Level: DiagnosticLevel.Warning,
            Message: $"Import targets undeclared namespace: '{namespaceName}'",
            Source: $"{moduleFile}:{line}:{column}",
            Code: "WNS0001"
        ));
    }

    /// <summary>
    /// Emit error for duplicate symbol declarations across modules in the same namespace.
    /// </summary>
    /// <param name="symbolName">The duplicate symbol name</param>
    /// <param name="namespaceName">The namespace containing the duplicate</param>
    /// <param name="firstModule">Path to the first module declaring the symbol</param>
    /// <param name="secondModule">Path to the second module declaring the symbol</param>
    public void EmitDuplicateSymbolError(string symbolName, string namespaceName, string firstModule, string secondModule)
    {
        var nsDisplay = string.IsNullOrEmpty(namespaceName) ? "global namespace" : $"namespace '{namespaceName}'";
        _diagnostics.Add(new Diagnostic(
            Level: DiagnosticLevel.Error,
            Message: $"Symbol '{symbolName}' is declared in multiple modules in {nsDisplay}: {firstModule} and {secondModule}",
            Source: secondModule,
            Code: "NS0001"
        ));
    }

    /// <summary>
    /// Emit error for multiple entry points across the module set.
    /// </summary>
    /// <param name="entryPoints">List of modules containing main() functions</param>
    public void EmitMultipleEntryPointsError(List<string> entryPoints)
    {
        var modules = string.Join(", ", entryPoints);
        _diagnostics.Add(new Diagnostic(
            Level: DiagnosticLevel.Error,
            Message: $"Multiple 'main' functions found across modules: {modules}. Only one entry point is allowed.",
            Source: entryPoints.FirstOrDefault() ?? string.Empty,
            Code: "NS0002"
        ));
    }

    /// <summary>
    /// Emit error for no entry point found.
    /// </summary>
    public void EmitNoEntryPointError()
    {
        _diagnostics.Add(new Diagnostic(
            Level: DiagnosticLevel.Error,
            Message: "No 'main' function found. An entry point is required.",
            Source: string.Empty,
            Code: "NS0003"
        ));
    }

    /// <summary>
    /// Clear all diagnostics.
    /// </summary>
    public void Clear()
    {
        _diagnostics.Clear();
    }
}
