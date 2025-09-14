using ast;
using ast_model;
using ast_model.Symbols;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowers Graph Assertion Blocks. Scaffold version:
/// - Expression form: leaves as-is for later concrete lowering/codegen.
/// - Statement form: surfaces a diagnostic error when no explicit store is declared.
///   For now, since store declarations are not yet modeled in the AST, this visitor
///   conservatively throws to signal the missing store configuration.
/// </summary>
public class GraphAssertionLoweringVisitor : NullSafeRecursiveDescentVisitor
{
    public override GraphAssertionBlockExp VisitGraphAssertionBlockExp(GraphAssertionBlockExp ctx)
    {
        // Future: Rewrite to explicit construction via KG.CreateGraph() and inner assertions.
        return base.VisitGraphAssertionBlockExp(ctx);
    }

    public override GraphAssertionBlockStatement VisitGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx)
    {
        // Find the enclosing module by walking parents
        IAstThing? p = ctx;
        ModuleDef? module = null;
        while (p != null && module == null)
        {
            if (p is ModuleDef m)
            {
                module = m;
                break;
            }
            p = p.Parent;
        }

        if (module is null)
        {
            throw new CompilationException("Graph assertion block statement not within a module scope.");
        }

        // Stores are captured by the parser in module.Annotations["GraphStores"]
        if (!module.Annotations.TryGetValue("GraphStores", out var storesObj) || storesObj is not Dictionary<string, string> stores || stores.Count == 0)
        {
            throw new CompilationException(
                "Graph assertion block used as a statement requires an explicit store declaration; none found.");
        }

        // Determine default store name: prefer explicit DefaultGraphStore, else first entry
        string defaultStoreName = "";
        if (module.Annotations.TryGetValue("DefaultGraphStore", out var defStoreObj) && defStoreObj is string named && !string.IsNullOrWhiteSpace(named))
        {
            defaultStoreName = named;
        }
        else
        {
            foreach (var k in stores.Keys)
            {
                defaultStoreName = k;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(defaultStoreName))
        {
            throw new CompilationException(
                "Graph assertion block used as a statement requires a resolvable default store; none found.");
        }

        string? storeUri;
        if (!stores.TryGetValue(defaultStoreName, out storeUri) || string.IsNullOrWhiteSpace(storeUri))
        {
            throw new CompilationException(
                $"Default graph store '{defaultStoreName}' has no associated URI.");
        }

        // Annotate the statement with the resolved store for downstream codegen
        ctx.Annotations["ResolvedStoreName"] = defaultStoreName;
        ctx.Annotations["ResolvedStoreUri"] = storeUri;

        // Future: We could rewrite ctx.Content to explicit persistence calls here.
        return base.VisitGraphAssertionBlockStatement(ctx);
    }
}
