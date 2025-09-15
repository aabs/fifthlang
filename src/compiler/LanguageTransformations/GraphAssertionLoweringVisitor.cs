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
    private ModuleDef? _currentModule;

    private static ModuleDef? FindEnclosingModule(IAstThing node)
    {
        var current = node.Parent;
        while (current is not null)
        {
            if (current is ModuleDef m)
                return m;
            current = current.Parent;
        }
        return null;
    }

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        var prev = _currentModule;
        _currentModule = ctx;
        try
        {
            return base.VisitModuleDef(ctx);
        }
        finally
        {
            _currentModule = prev;
        }
    }

    public override GraphAssertionBlockExp VisitGraphAssertionBlockExp(GraphAssertionBlockExp ctx)
    {
        // Expression form: strict no-op. Do not create a new record instance
        // to preserve reference identity for downstream passes/tests.
        // Future: Rewrite to explicit construction via KG.CreateGraph() and inner assertions.
        return ctx;
    }

    public override GraphAssertionBlockStatement VisitGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx)
    {
        var module = _currentModule ?? FindEnclosingModule(ctx);
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

        // Annotate the statement with the resolved store for downstream passes (e.g., linkage)
        ctx.Annotations["ResolvedStoreName"] = defaultStoreName;
        ctx.Annotations["ResolvedStoreUri"] = storeUri;

        // Lowering: Rewrite the statement-form graph block into an explicit persistence call
        // using the built-in KG helpers: KG.SaveGraph(KG.ConnectToRemoteStore(uri), <graph-exp>);
        // Note: The expression form currently yields a fresh graph via KG.CreateGraph().

        var kgVarForConnect = new VarRefExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = ctx.Location,
            Parent = null,
            VarName = "KG"
        };

        var uriLiteral = new StringLiteralExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = ctx.Location,
            Parent = null,
            Value = storeUri
        };

        var connectCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { uriLiteral },
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "ConnectToRemoteStore" },
            Location = ctx.Location,
            Parent = null,
        };

        var connectExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = kgVarForConnect,
            RHS = connectCall,
            Location = ctx.Location,
            Type = ctx.Type
        };

        var kgVarForSave = new VarRefExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = ctx.Location,
            Parent = null,
            VarName = "KG"
        };

        var saveCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { connectExpr, ctx.Content },
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "SaveGraph" },
            Location = ctx.Location,
            Parent = null,
        };

        var saveExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = kgVarForSave,
            RHS = saveCall,
            Location = ctx.Location,
            Type = ctx.Type
        };

        var lowered = new ExpStatement
        {
            Annotations = new Dictionary<string, object>(),
            RHS = saveExpr,
            Location = ctx.Location,
            Type = ctx.Type
        };

        // Return a dummy node; caller will replace this in parent context if needed.
        // Since our visitor signature requires returning GraphAssertionBlockStatement,
        // we attach a marker so a subsequent normalization pass (or downstream usage)
        // can consume the lowered ExpStatement. To keep things simple, we stash the
        // lowered node in annotations and let ParserManager use the mutated tree.
        // However, we can also replace the node in-place by leveraging parent pointers.

        // Replace current node in parent block if possible
        if (ctx.Parent is BlockStatement bs)
        {
            for (int i = 0; i < bs.Statements.Count; i++)
            {
                if (ReferenceEquals(bs.Statements[i], ctx))
                {
                    bs.Statements[i] = lowered;
                    break;
                }
            }
        }

        // Return ctx unchanged (already replaced in parent block)
        return ctx;
    }

    public override ExpStatement VisitExpStatement(ExpStatement ctx)
    {
        // Allow base traversal first (no-op for ExpStatement)
        var result = base.VisitExpStatement(ctx);

        // Upgrade '+=' desugaring fallback: if SaveGraph was given KG.CreateStore(),
        // replace it with KG.ConnectToRemoteStore(defaultUri) using module annotations.
        var module = _currentModule ?? FindEnclosingModule(ctx);
        if (module == null) return result;

        // Ensure we have store annotations
        if (!module.Annotations.TryGetValue("GraphStores", out var storesObj)
            || storesObj is not Dictionary<string, string> stores
            || stores.Count == 0)
        {
            return result;
        }

        var defaultStoreName = string.Empty;
        if (module.Annotations.TryGetValue("DefaultGraphStore", out var defStoreObj)
            && defStoreObj is string defName
            && !string.IsNullOrWhiteSpace(defName))
        {
            defaultStoreName = defName;
        }
        else
        {
            // Fallback: first declared store
            foreach (var k in stores.Keys) { defaultStoreName = k; break; }
        }
        if (string.IsNullOrWhiteSpace(defaultStoreName)) return result;
        if (!stores.TryGetValue(defaultStoreName, out var storeUri) || string.IsNullOrWhiteSpace(storeUri)) return result;

        // Pattern match: KG.SaveGraph(<arg0>, <arg1>)
        if (ctx.RHS is MemberAccessExp ma && ma.LHS is VarRefExp kgVar && string.Equals(kgVar.VarName, "KG", StringComparison.Ordinal)
            && ma.RHS is FuncCallExp saveCall
            && saveCall.Annotations != null
            && saveCall.Annotations.TryGetValue("FunctionName", out var fnObj)
            && fnObj is string fn && string.Equals(fn, "SaveGraph", StringComparison.Ordinal))
        {
            var args = saveCall.InvocationArguments ?? new List<Expression>();
            if (args.Count >= 1)
            {
                // Detect KG.CreateStore() used as the first argument
                if (args[0] is MemberAccessExp firstMa
                    && firstMa.LHS is VarRefExp kgVar2 && string.Equals(kgVar2.VarName, "KG", StringComparison.Ordinal)
                    && firstMa.RHS is FuncCallExp createStoreCall
                    && createStoreCall.Annotations != null
                    && createStoreCall.Annotations.TryGetValue("FunctionName", out var csFnObj)
                    && csFnObj is string csFn && string.Equals(csFn, "CreateStore", StringComparison.Ordinal))
                {
                    // Build KG.ConnectToRemoteStore(storeUri)
                    var uriLiteral = new StringLiteralExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        Location = ctx.Location,
                        Parent = null,
                        Value = storeUri
                    };
                    var connectCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { uriLiteral },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "ConnectToRemoteStore" },
                        Location = ctx.Location,
                        Parent = null,
                    };
                    var connectExpr = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = ctx.Location },
                        RHS = connectCall,
                        Location = ctx.Location,
                        Type = ctx.Type
                    };

                    // Rewrite arg0
                    args[0] = connectExpr;
                    saveCall.InvocationArguments = args;
                }
            }
        }

        return result;
    }
}
