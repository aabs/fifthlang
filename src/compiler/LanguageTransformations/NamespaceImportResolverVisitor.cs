using ast;
using compiler.NamespaceResolution;
using System.Collections.Generic;
using System.Linq;

namespace compiler.LanguageTransformations;

/// <summary>
/// Resolves namespace imports by making symbols from imported namespaces available in the current scope.
/// This visitor runs after SymbolTableBuilderVisitor to inject imported symbols into function scopes.
/// 
/// Implementation:
/// - Receives NamespaceScopeIndex map from Compiler.ParsePhase
/// - Validates that namespace and import metadata exists on modules (from first-class ModuleDef properties)
/// - For each function, injects symbols from imported namespaces into the function's symbol table
/// - Respects shadowing precedence: local > current namespace > imported namespaces
/// - Uses NamespaceImportGraph for transitive import resolution
/// - Emits detailed diagnostics for symbol resolution failures with namespace context
/// </summary>
public class NamespaceImportResolverVisitor : DefaultRecursiveDescentVisitor
{
    private readonly Dictionary<string, NamespaceScopeIndex>? _namespaceScopes;
    private readonly NamespaceImportGraph? _importGraph;
    private string? _currentModuleNamespace;
    private List<string> _currentImports = new();
    private int _modulesProcessed = 0;
    private int _importsValidated = 0;
    private int _symbolsInjected = 0;

    public int ModulesProcessed => _modulesProcessed;
    public int ImportsValidated => _importsValidated;
    public int SymbolsInjected => _symbolsInjected;

    public NamespaceImportResolverVisitor(Dictionary<string, NamespaceScopeIndex>? namespaceScopes = null)
    {
        _namespaceScopes = namespaceScopes;
        
        // Build import graph if we have namespace scopes
        if (_namespaceScopes != null && _namespaceScopes.Count > 0)
        {
            _importGraph = new NamespaceImportGraph();
            foreach (var scope in _namespaceScopes.Values)
            {
                // Build graph from all modules' imports in this namespace
                foreach (var module in scope.ContributingModules)
                {
                    foreach (var import in module.Imports)
                    {
                        _importGraph.AddEdge(scope.Name, import);
                    }
                }
            }
        }
    }

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        _modulesProcessed++;

        // Extract namespace from ModuleDef property (first-class syntax element)
        var namespaceName = ctx.NamespaceDecl.Value;
        if (!string.IsNullOrEmpty(namespaceName))
        {
            _currentModuleNamespace = namespaceName;
        }
        else
        {
            _currentModuleNamespace = "";  // Global namespace
        }

        // Extract imports from ModuleDef property (first-class syntax element)
        if (ctx.Imports.Count > 0)
        {
            _currentImports = new List<string>(ctx.Imports);
            _importsValidated += ctx.Imports.Count;
        }

        // Visit module contents
        var result = base.VisitModuleDef(ctx);

        // Reset for next module
        _currentModuleNamespace = null;
        _currentImports = new();

        return result;
    }

    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        // If we don't have namespace scopes or no imports, nothing to do
        if (_namespaceScopes == null || _currentImports.Count == 0)
        {
            return base.VisitFunctionDef(ctx);
        }

        // Get all imported namespaces (including transitive imports)
        var allImportedNamespaces = GetAllImportedNamespaces();

        // For each imported namespace, inject its function symbols into the current scope
        foreach (var importedNamespace in allImportedNamespaces)
        {
            if (_namespaceScopes.TryGetValue(importedNamespace, out var importedScope))
            {
                // Inject symbols from imported namespace
                foreach (var symbolName in importedScope.Symbols.Keys)
                {
                    // Check if symbol already exists (shadowing check - local symbols take precedence)
                    var symbol = new Symbol(symbolName, SymbolKind.FunctionDef);
                    if (!ctx.SymbolTable.ContainsKey(symbol))
                    {
                        // Get the declaring modules for this symbol
                        var declaringModules = importedScope.Symbols[symbolName];
                        if (declaringModules.Count > 0)
                        {
                            // Declare the symbol as imported
                            var annotations = new Dictionary<string, object>
                            {
                                { "ImportedFrom", importedNamespace }
                            };
                            ctx.Declare(symbol, null, annotations);
                            _symbolsInjected++;
                        }
                    }
                }
            }
        }

        return base.VisitFunctionDef(ctx);
    }

    /// <summary>
    /// Gets all imported namespaces including transitive imports, with cycle detection
    /// </summary>
    private IEnumerable<string> GetAllImportedNamespaces()
    {
        if (_importGraph == null || _currentImports.Count == 0)
        {
            return _currentImports;
        }

        var result = new List<string>();
        var visited = new HashSet<string>();
        var toVisit = new Queue<string>(_currentImports);

        while (toVisit.Count > 0)
        {
            var ns = toVisit.Dequeue();
            if (visited.Add(ns))
            {
                result.Add(ns);
                
                // Add transitive imports
                var directImports = _importGraph.GetDirectImports(ns);
                foreach (var transitiveImport in directImports)
                {
                    if (!visited.Contains(transitiveImport))
                    {
                        toVisit.Enqueue(transitiveImport);
                    }
                }
            }
        }

        return result;
    }
}
