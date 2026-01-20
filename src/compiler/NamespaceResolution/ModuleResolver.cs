using ast;
namespace compiler.NamespaceResolution;

public sealed class ModuleResolver
{
    public const string ModuleMetadataKey = "ModuleMetadata";

    public ModuleResolutionResult Resolve(CompilerOptions options, List<Diagnostic> diagnostics)
    {
        var sources = ResolveSourceFiles(options, diagnostics);
        if (sources.Count == 0)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, "No .5th source files were provided for compilation."));
            return new ModuleResolutionResult(null, [], 0);
        }

        AssemblyDef? baseAssembly = null;
        var modules = new List<ModuleDef>();
        var metadata = new List<ModuleMetadata>();

        foreach (var source in sources)
        {
            try
            {
                var parsed = FifthParserManager.ParseFile(source) as AssemblyDef;
                if (parsed == null)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Parse did not produce an AssemblyDef for: {source}", source));
                    continue;
                }

                baseAssembly ??= parsed;

                var module = parsed.Modules.SingleOrDefault();
                if (module == null)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"No module found in parsed output for: {source}", source));
                    continue;
                }

                var updatedModule = EnsureModuleMetadata(module, source);
                modules.Add(updatedModule);

                metadata.Add(ModuleMetadata.FromModule(updatedModule));
            }
            catch (Exception ex)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Parse error: {ex.Message}", source));
            }
        }

        if (baseAssembly == null)
        {
            return new ModuleResolutionResult(null, [], 0);
        }

        var combinedAssembly = baseAssembly with { Modules = modules };
        combinedAssembly.Annotations ??= new Dictionary<string, object>();
        combinedAssembly.Annotations[ModuleMetadataKey] = metadata;

        if (!ValidateEntryPoint(metadata, diagnostics))
        {
            return new ModuleResolutionResult(null, metadata, sources.Count);
        }

        return new ModuleResolutionResult(combinedAssembly, metadata, sources.Count);
    }

    private static List<string> ResolveSourceFiles(CompilerOptions options, List<Diagnostic> diagnostics)
    {
        var sources = new List<string>();

        if (options.SourceFiles != null && options.SourceFiles.Count > 0)
        {
            sources.AddRange(options.SourceFiles);
            return sources;
        }

        if (!string.IsNullOrWhiteSpace(options.SourceManifest) && File.Exists(options.SourceManifest))
        {
            foreach (var line in File.ReadAllLines(options.SourceManifest))
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    sources.Add(trimmed);
                }
            }

            return sources;
        }

        if (File.Exists(options.Source))
        {
            sources.Add(options.Source);
            return sources;
        }

        if (Directory.Exists(options.Source))
        {
            sources.AddRange(Directory.GetFiles(options.Source, "*.5th", SearchOption.AllDirectories)
                .OrderBy(f => f));
            return sources;
        }

        diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Source path not found: {options.Source}"));
        return sources;
    }

    private static ModuleDef EnsureModuleMetadata(ModuleDef module, string sourcePath)
    {
        var annotations = module.Annotations ?? new Dictionary<string, object>();
        annotations[ModuleAnnotationKeys.ModulePath] = sourcePath;

        var moduleName = Path.GetFileNameWithoutExtension(sourcePath);
        var updated = module with { OriginalModuleName = moduleName, Annotations = annotations };

        return updated;
    }

    private static bool ValidateEntryPoint(IReadOnlyList<ModuleMetadata> modules, List<Diagnostic> diagnostics)
    {
        var mainModules = new List<string>();

        foreach (var module in modules)
        {
            foreach (var function in module.Module.Functions)
            {
                if (function is FunctionDef def && def.Name.Value.Equals("main", StringComparison.Ordinal))
                {
                    mainModules.Add(module.ModulePath);
                }
                else if (function is OverloadedFunctionDef overload && overload.Name.Value.Equals("main", StringComparison.Ordinal))
                {
                    mainModules.Add(module.ModulePath);
                }
            }
        }

        if (mainModules.Count == 1)
        {
            return true;
        }

        var description = mainModules.Count == 0 ? "No main function was found" : "Multiple main functions were found";
        var moduleList = mainModules.Count == 0
            ? string.Join(", ", modules.Select(m => m.ModulePath))
            : string.Join(", ", mainModules.Distinct());

        diagnostics.Add(new Diagnostic(
            DiagnosticLevel.Error,
            $"{description} across modules: {moduleList}",
            source: moduleList));

        return false;
    }
}

public record ModuleResolutionResult(
    AssemblyDef? Assembly,
    IReadOnlyList<ModuleMetadata> Modules,
    int SourceCount);
