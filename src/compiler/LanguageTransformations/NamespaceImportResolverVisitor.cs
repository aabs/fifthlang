using ast;
using ast_model.Symbols;
using compiler.NamespaceResolution;

namespace compiler.LanguageTransformations;

public class NamespaceImportResolverVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic>? _diagnostics;

    public static long LastElapsedMilliseconds { get; private set; }

    public NamespaceImportResolverVisitor(List<Diagnostic>? diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public override AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var diagnostics = _diagnostics ?? [];
        var emitter = new NamespaceDiagnosticEmitter(diagnostics);

        var modules = ResolveModuleMetadata(ctx);
        var index = new NamespaceScopeIndex();
        var updatedModules = new List<ModuleMetadata>();

        foreach (var module in modules)
        {
            var declarations = module.Module.SymbolTable.All().ToList();
            var updated = module with { Declarations = declarations };
            index.AddModule(updated, declarations, emitter);
            updatedModules.Add(updated);
        }

        var importGraph = BuildImportGraph(updatedModules);

        foreach (var module in updatedModules)
        {
            var resolvedNamespaces = new HashSet<string>(StringComparer.Ordinal);
            ApplyImports(module, index, importGraph, emitter, resolvedNamespaces);

            if (module.Module.Annotations != null)
            {
                module.Module.Annotations[ModuleAnnotationKeys.ResolvedImports] = resolvedNamespaces.ToList();
            }
        }

        ctx.Annotations[ModuleResolver.ModuleMetadataKey] = updatedModules;

        stopwatch.Stop();
        LastElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        return base.VisitAssemblyDef(ctx);
    }

    private static List<ModuleMetadata> ResolveModuleMetadata(AssemblyDef ctx)
    {
        if (ctx.Annotations != null
            && ctx.Annotations.TryGetValue(ModuleResolver.ModuleMetadataKey, out var value)
            && value is IReadOnlyList<ModuleMetadata> metadata)
        {
            return metadata.ToList();
        }

        return ctx.Modules.Select(ModuleMetadata.FromModule).ToList();
    }

    private static NamespaceImportGraph BuildImportGraph(IEnumerable<ModuleMetadata> modules)
    {
        var graph = new NamespaceImportGraph();
        foreach (var module in modules)
        {
            foreach (var import in module.Imports)
            {
                if (!string.IsNullOrWhiteSpace(module.DeclaredNamespace))
                {
                    graph.AddImport(module.DeclaredNamespace, import.Namespace);
                }
            }
        }

        return graph;
    }

    private static void ApplyImports(ModuleMetadata module, NamespaceScopeIndex index, NamespaceImportGraph graph, NamespaceDiagnosticEmitter emitter, HashSet<string> resolvedNamespaces)
    {
        var processed = new HashSet<string>(StringComparer.Ordinal);
        var warned = new HashSet<string>(StringComparer.Ordinal);

        foreach (var import in module.Imports)
        {
            if (!processed.Add(import.Namespace))
            {
                continue;
            }

            var namespacesToApply = new HashSet<string>(StringComparer.Ordinal) { import.Namespace };
            foreach (var transitive in graph.TraverseImports(import.Namespace))
            {
                namespacesToApply.Add(transitive);
            }

            foreach (var ns in namespacesToApply)
            {
                if (!index.TryGetScope(ns, out var scope))
                {
                    if (warned.Add(ns))
                    {
                        emitter.EmitUndeclaredImport(module, import);
                    }

                    continue;
                }

                resolvedNamespaces.Add(ns);
                ImportSymbols(module.Module, scope);
            }
        }
    }

    private static void ImportSymbols(ModuleDef module, NamespaceScope scope)
    {
        foreach (var entry in scope.Symbols.Values)
        {
            var existingByName = module.SymbolTable.ResolveByName(entry.Symbol.Name);
            if (existingByName != null)
            {
                if (existingByName is SymbolTableEntry existingEntry
                    && !existingEntry.IsImported
                    && !existingEntry.IsLocalShadow)
                {
                    module.SymbolTable[existingEntry.Symbol] = existingEntry with { IsLocalShadow = true };
                }

                continue;
            }

            var importedEntry = new SymbolTableEntry
            {
                Symbol = entry.Symbol,
                OriginatingAstThing = entry.OriginatingAstThing,
                Annotations = new Dictionary<string, object>(entry.Annotations),
                QualifiedName = entry.QualifiedName,
                IsImported = true,
                IsLocalShadow = false
            };

            module.SymbolTable[entry.Symbol] = importedEntry;
        }
    }


}
