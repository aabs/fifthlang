using ast_model.Symbols;

namespace compiler.NamespaceResolution;

public sealed class NamespaceScopeIndex
{
    private readonly Dictionary<string, NamespaceScope> _scopes = new(StringComparer.Ordinal);

    public IReadOnlyCollection<NamespaceScope> Scopes => _scopes.Values;

    public NamespaceScope GetOrCreate(string name)
    {
        if (!_scopes.TryGetValue(name, out var scope))
        {
            scope = new NamespaceScope(name);
            _scopes[name] = scope;
        }

        return scope;
    }

    public bool TryGetScope(string name, out NamespaceScope scope) => _scopes.TryGetValue(name, out scope!);

    public void AddModule(ModuleMetadata module, IEnumerable<ISymbolTableEntry> declarations, NamespaceDiagnosticEmitter diagnosticEmitter)
    {
        var scope = GetOrCreate(module.DeclaredNamespace);
        if (!scope.Modules.Contains(module))
        {
            scope.Modules.Add(module);
        }

        foreach (var entry in declarations)
        {
            var symbolName = entry.Symbol.Name;
            if (scope.Symbols.TryGetValue(symbolName, out var existing))
            {
                if (!ReferenceEquals(existing.OriginatingAstThing, entry.OriginatingAstThing)
                    && !string.Equals(existing.OriginatingAstThing?.ToString(), entry.OriginatingAstThing?.ToString(), StringComparison.Ordinal))
                {
                    if (!module.ModulePath.Equals(scope.SymbolOrigins[symbolName], StringComparison.Ordinal))
                    {
                        diagnosticEmitter.EmitDuplicateSymbol(module, scope, symbolName, scope.SymbolOrigins[symbolName]);
                    }
                }

                continue;
            }

            scope.Symbols[symbolName] = entry;
            scope.SymbolOrigins[symbolName] = module.ModulePath;
        }
    }
}

public sealed class NamespaceScope
{
    public NamespaceScope(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public List<ModuleMetadata> Modules { get; } = [];
    public Dictionary<string, ISymbolTableEntry> Symbols { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> SymbolOrigins { get; } = new(StringComparer.Ordinal);
}
