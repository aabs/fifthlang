using System.Diagnostics;

namespace compiler.NamespaceResolution;

/// <summary>
/// Resolves and loads modules from CLI arguments or MSBuild manifests.
/// Combines multiple sources into a unified set of ModuleMetadata instances.
/// </summary>
public class ModuleResolver
{
    private readonly NamespaceDiagnosticEmitter _diagnosticEmitter;

    public ModuleResolver(NamespaceDiagnosticEmitter diagnosticEmitter)
    {
        _diagnosticEmitter = diagnosticEmitter;
    }

    /// <summary>
    /// Resolve modules from CLI-provided file paths.
    /// </summary>
    /// <param name="sourceFiles">List of .5th file paths</param>
    /// <returns>List of ModuleMetadata instances</returns>
    public List<ModuleMetadata> ResolveFromCliFiles(IEnumerable<string> sourceFiles)
    {
        var fileList = sourceFiles.ToList();
        if (fileList.Count == 0)
        {
            return new List<ModuleMetadata>();
        }

        // Extract metadata from each file
        var modules = ModuleMetadataExtractor.ExtractFromFiles(fileList);
        
        // Validate single entry point
        ValidateEntryPoint(modules);

        return modules;
    }

    /// <summary>
    /// Resolve modules from MSBuild item list (future enhancement).
    /// For now, returns empty list as MSBuild support is not yet implemented.
    /// </summary>
    public List<ModuleMetadata> ResolveFromMSBuildManifest(string projectFile)
    {
        // TODO: Implement MSBuild manifest reading in T022
        // This would parse the .csproj and extract .5th files from item lists
        return new List<ModuleMetadata>();
    }

    /// <summary>
    /// Validate that exactly one entry point (main function) exists across all modules.
    /// Emits diagnostics if zero or multiple entry points are found.
    /// </summary>
    private void ValidateEntryPoint(List<ModuleMetadata> modules)
    {
        var modulesWithMain = new List<string>();

        foreach (var module in modules)
        {
            // Check if any function is named "main"
            foreach (var declaration in module.Declarations)
            {
                if (declaration is ast.FunctionDef func && func.Name.ToString() == "main")
                {
                    modulesWithMain.Add(module.Path);
                    break;
                }
            }
        }

        if (modulesWithMain.Count == 0)
        {
            _diagnosticEmitter.EmitNoEntryPointError();
        }
        else if (modulesWithMain.Count > 1)
        {
            _diagnosticEmitter.EmitMultipleEntryPointsError(modulesWithMain);
        }
    }

    /// <summary>
    /// Build namespace scopes from modules and resolve imports.
    /// </summary>
    /// <param name="modules">List of module metadata</param>
    /// <returns>Dictionary mapping namespace names to their scope indices</returns>
    public Dictionary<string, NamespaceScopeIndex> BuildNamespaceScopes(List<ModuleMetadata> modules)
    {
        var stopwatch = Stopwatch.StartNew();
        var namespaceScopes = new Dictionary<string, NamespaceScopeIndex>();

        // Group modules by namespace
        foreach (var module in modules)
        {
            var namespaceName = module.DeclaredNamespace ?? string.Empty;  // Empty string = global namespace
            
            if (!namespaceScopes.ContainsKey(namespaceName))
            {
                namespaceScopes[namespaceName] = new NamespaceScopeIndex(namespaceName);
            }

            namespaceScopes[namespaceName].AddModule(module);
        }

        // Collect diagnostics from namespace aggregation
        foreach (var scope in namespaceScopes.Values)
        {
            foreach (var diagnostic in scope.Diagnostics)
            {
                _diagnosticEmitter.Diagnostics.ToList().Add(diagnostic);
            }
        }

        // Resolve imports for each module
        ResolveImports(modules, namespaceScopes);

        stopwatch.Stop();
        Console.WriteLine($"[Namespace Resolution] Completed in {stopwatch.ElapsedMilliseconds}ms");

        return namespaceScopes;
    }

    /// <summary>
    /// Resolve import directives by linking them to namespace scopes.
    /// Emits WNS0001 warnings for undeclared namespaces.
    /// </summary>
    private void ResolveImports(List<ModuleMetadata> modules, Dictionary<string, NamespaceScopeIndex> namespaceScopes)
    {
        foreach (var module in modules)
        {
            foreach (var importName in module.Imports)
            {
                if (!namespaceScopes.ContainsKey(importName))
                {
                    // Emit warning WNS0001: undeclared namespace import
                    _diagnosticEmitter.EmitUndeclaredNamespaceWarning(
                        moduleFile: module.Path,
                        namespaceName: importName,
                        line: 1,  // TODO: Track actual line numbers from parse tree
                        column: 1
                    );
                }
                // Import is valid - symbols from the namespace are now available to this module
                // (Symbol resolution happens in later compilation phases)
            }
        }
    }
}
