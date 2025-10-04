using ast;
using ast_model;
using ast_model.Symbols;
using System.Collections.Generic;

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
        // Expression form: lower to an expression that constructs a graph and applies assertions.
        // Build KG.CreateGraph() to materialize a graph instance we'll populate
        var kgVarForCreateGraph = new VarRefExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = ctx.Location,
            Parent = null,
            VarName = "KG"
        };
        var createGraphCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression>(),
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateGraph" },
            Location = ctx.Location,
            Parent = null
        };
        var createGraphExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = kgVarForCreateGraph,
            RHS = createGraphCall,
            Location = ctx.Location,
            Type = ctx.Type
        };

        // Now lower inner statements into a graph-building expression by chaining Assert calls.
        Expression graphBuilder = createGraphExpr;
        var innerStatements = ctx.Content is BlockStatement bs ? bs.Statements : new List<Statement>();

        foreach (var s in innerStatements)
        {
            if (s is AssertionStatement asrt)
            {
                // Translate assertion to: KG.Assert(<graph>, KG.CreateTriple(...))
                // Subject
                Expression subjNode;
                if (asrt.Assertion?.SubjectExp is UriLiteralExp sUri)
                {
                    var subjUriLiteral = new StringLiteralExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        Location = sUri.Location,
                        Parent = null,
                        Value = sUri.Value.AbsoluteUri
                    };
                    var subjCreateUriCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, subjUriLiteral },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" },
                        Location = sUri.Location,
                        Parent = null
                    };
                    subjNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = sUri.Location },
                        RHS = subjCreateUriCall,
                        Location = sUri.Location
                    };
                }
                else
                {
                    subjNode = new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = s.Location };
                }

                // Predicate
                Expression predNode;
                if (asrt.Assertion?.PredicateExp is UriLiteralExp pUri)
                {
                    var predUriLiteral = new StringLiteralExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        Location = pUri.Location,
                        Parent = null,
                        Value = pUri.Value.AbsoluteUri
                    };
                    var predCreateUriCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, predUriLiteral },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" },
                        Location = pUri.Location,
                        Parent = null
                    };
                    predNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = pUri.Location },
                        RHS = predCreateUriCall,
                        Location = pUri.Location
                    };
                }
                else
                {
                    predNode = new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = s.Location };
                }

                // Object
                Expression objNode;
                if (asrt.Assertion?.ObjectExp is StringLiteralExp strLit)
                {
                    var lit = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = strLit.Location, Parent = null, Value = strLit.Value };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = strLit.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = strLit.Location },
                        RHS = createLiteralCall,
                        Location = strLit.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Int32LiteralExp i32)
                {
                    var lit = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = i32.Location, Parent = null, Value = i32.Value, Type = i32.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = i32.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i32.Location },
                        RHS = createLiteralCall,
                        Location = i32.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Int16LiteralExp i16)
                {
                    var lit = new Int16LiteralExp { Annotations = new Dictionary<string, object>(), Location = i16.Location, Parent = null, Value = i16.Value, Type = i16.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = i16.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i16.Location },
                        RHS = createLiteralCall,
                        Location = i16.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Int8LiteralExp i8)
                {
                    var lit = new Int8LiteralExp { Annotations = new Dictionary<string, object>(), Location = i8.Location, Parent = null, Value = i8.Value, Type = i8.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = i8.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i8.Location },
                        RHS = createLiteralCall,
                        Location = i8.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Float16LiteralExp decLit)
                {
                    var lit = new Float16LiteralExp { Annotations = new Dictionary<string, object>(), Location = decLit.Location, Parent = null, Value = decLit.Value, Type = decLit.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = decLit.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = decLit.Location },
                        RHS = createLiteralCall,
                        Location = decLit.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is CharLiteralExp ch)
                {
                    var lit = new CharLiteralExp { Annotations = new Dictionary<string, object>(), Location = ch.Location, Parent = null, Value = ch.Value, Type = ch.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = ch.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = ch.Location },
                        RHS = createLiteralCall,
                        Location = ch.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is BooleanLiteralExp bl)
                {
                    var boolLit = new BooleanLiteralExp { Annotations = new Dictionary<string, object>(), Location = bl.Location, Parent = null, Value = bl.Value, Type = bl.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, boolLit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = bl.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = bl.Location },
                        RHS = createLiteralCall,
                        Location = bl.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is UriLiteralExp oUri)
                {
                    var objUriLiteral = new StringLiteralExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        Location = oUri.Location,
                        Parent = null,
                        Value = oUri.Value.AbsoluteUri
                    };
                    var objCreateUriCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, objUriLiteral },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" },
                        Location = oUri.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = oUri.Location },
                        RHS = objCreateUriCall,
                        Location = oUri.Location
                    };
                }
                else
                {
                    // Fallback: attempt to use expression as-is
                    objNode = asrt.Assertion?.ObjectExp ?? new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = s.Location };
                }

                // KG.CreateTriple(subjNode, predNode, objNode)
                var createTripleCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { subjNode, predNode, objNode },
                    Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateTriple" },
                    Location = s.Location,
                    Parent = null
                };
                var createTripleExpr = new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location },
                    RHS = createTripleCall,
                    Location = s.Location
                };

                // KG.Assert(graphBuilder, triple)
                var assertCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { graphBuilder, createTripleExpr },
                    Annotations = new Dictionary<string, object> { ["FunctionName"] = "Assert" },
                    Location = s.Location,
                    Parent = null
                };
                graphBuilder = new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location },
                    RHS = assertCall,
                    Location = s.Location
                };
            }
            else
            {
                // Non-assertion statements are ignored in graph-building context for now
            }
        }

        // Annotate the GraphAssertionBlockExp with the lowered expression so downstream passes (e.g. IL lowering)
        // can consume it without requiring structural AST replacement. Use pattern-match to avoid
        // dereference warnings against the init-only Annotations property.
        if (ctx.Annotations is Dictionary<string, object> ann)
        {
            ann["GraphExpr"] = graphBuilder;
        }

        // Return the original node (annotated) so traversal continues; IL lowering will consult the annotation.
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

        // Build KG.ConnectToRemoteStore(uri)
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

        // Build KG.CreateGraph() to materialize a graph instance we'll populate
        var kgVarForCreateGraph = new VarRefExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = ctx.Location,
            Parent = null,
            VarName = "KG"
        };
        var createGraphCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression>(),
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateGraph" },
            Location = ctx.Location,
            Parent = null
        };
        var createGraphExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = kgVarForCreateGraph,
            RHS = createGraphCall,
            Location = ctx.Location,
            Type = ctx.Type
        };

        // Now lower inner statements into a graph-building expression by chaining Assert calls.
        Expression graphBuilder = createGraphExpr;
        var innerStatements = ctx.Content?.Content?.Statements ?? new List<Statement>();

        foreach (var s in innerStatements)
        {
            if (s is AssertionStatement asrt)
            {
                // Translate assertion to: KG.Assert(<graph>, KG.CreateTriple(KG.CreateUri(<graph>, subj), KG.CreateUri(<graph>, pred), ObjNode))
                // Subject
                Expression subjNode;
                if (asrt.Assertion?.SubjectExp is UriLiteralExp sUri)
                {
                    var subjUriLiteral = new StringLiteralExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        Location = sUri.Location,
                        Parent = null,
                        Value = sUri.Value.AbsoluteUri
                    };
                    var subjCreateUriCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, subjUriLiteral },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" },
                        Location = sUri.Location,
                        Parent = null
                    };
                    subjNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = sUri.Location },
                        RHS = subjCreateUriCall,
                        Location = sUri.Location
                    };
                }
                else
                {
                    // Fallback: emit null to trip runtime if unsupported
                    subjNode = new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = s.Location };
                }

                // Predicate
                Expression predNode;
                if (asrt.Assertion?.PredicateExp is UriLiteralExp pUri)
                {
                    var predUriLiteral = new StringLiteralExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        Location = pUri.Location,
                        Parent = null,
                        Value = pUri.Value.AbsoluteUri
                    };
                    var predCreateUriCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, predUriLiteral },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" },
                        Location = pUri.Location,
                        Parent = null
                    };
                    predNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = pUri.Location },
                        RHS = predCreateUriCall,
                        Location = pUri.Location
                    };
                }
                else
                {
                    predNode = new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = s.Location };
                }

                // Object node: support strings, numerics, bool, uris; else try expression as-is
                Expression objNode;
                if (asrt.Assertion?.ObjectExp is StringLiteralExp str)
                {
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, new StringLiteralExp { Value = str.Value, Annotations = new Dictionary<string, object>(), Location = str.Location } },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = str.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = str.Location },
                        RHS = createLiteralCall,
                        Location = str.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Float8LiteralExp d64)
                {
                    var lit = new Float8LiteralExp { Annotations = new Dictionary<string, object>(), Location = d64.Location, Parent = null, Value = d64.Value, Type = d64.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = d64.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = d64.Location },
                        RHS = createLiteralCall,
                        Location = d64.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Float4LiteralExp f32)
                {
                    var lit = new Float4LiteralExp { Annotations = new Dictionary<string, object>(), Location = f32.Location, Parent = null, Value = f32.Value, Type = f32.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = f32.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = f32.Location },
                        RHS = createLiteralCall,
                        Location = f32.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Int32LiteralExp i32)
                {
                    var intLit = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = i32.Location, Parent = null, Value = i32.Value, Type = i32.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, intLit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = i32.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i32.Location },
                        RHS = createLiteralCall,
                        Location = i32.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Int64LiteralExp i64)
                {
                    var lit = new Int64LiteralExp { Annotations = new Dictionary<string, object>(), Location = i64.Location, Parent = null, Value = i64.Value, Type = i64.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = i64.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i64.Location },
                        RHS = createLiteralCall,
                        Location = i64.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is UnsignedInt32LiteralExp ui32)
                {
                    var lit = new UnsignedInt32LiteralExp { Annotations = new Dictionary<string, object>(), Location = ui32.Location, Parent = null, Value = ui32.Value, Type = ui32.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = ui32.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = ui32.Location },
                        RHS = createLiteralCall,
                        Location = ui32.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is UnsignedInt64LiteralExp ui64)
                {
                    var lit = new UnsignedInt64LiteralExp { Annotations = new Dictionary<string, object>(), Location = ui64.Location, Parent = null, Value = ui64.Value, Type = ui64.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = ui64.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = ui64.Location },
                        RHS = createLiteralCall,
                        Location = ui64.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is UnsignedInt16LiteralExp ui16)
                {
                    var lit = new UnsignedInt16LiteralExp { Annotations = new Dictionary<string, object>(), Location = ui16.Location, Parent = null, Value = ui16.Value, Type = ui16.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = ui16.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = ui16.Location },
                        RHS = createLiteralCall,
                        Location = ui16.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is UnsignedInt8LiteralExp ui8)
                {
                    var lit = new UnsignedInt8LiteralExp { Annotations = new Dictionary<string, object>(), Location = ui8.Location, Parent = null, Value = ui8.Value, Type = ui8.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = ui8.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = ui8.Location },
                        RHS = createLiteralCall,
                        Location = ui8.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Int16LiteralExp i16)
                {
                    var lit = new Int16LiteralExp { Annotations = new Dictionary<string, object>(), Location = i16.Location, Parent = null, Value = i16.Value, Type = i16.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = i16.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i16.Location },
                        RHS = createLiteralCall,
                        Location = i16.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Int8LiteralExp i8)
                {
                    var lit = new Int8LiteralExp { Annotations = new Dictionary<string, object>(), Location = i8.Location, Parent = null, Value = i8.Value, Type = i8.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = i8.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i8.Location },
                        RHS = createLiteralCall,
                        Location = i8.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is Float16LiteralExp decLit)
                {
                    var lit = new Float16LiteralExp { Annotations = new Dictionary<string, object>(), Location = decLit.Location, Parent = null, Value = decLit.Value, Type = decLit.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = decLit.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = decLit.Location },
                        RHS = createLiteralCall,
                        Location = decLit.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is CharLiteralExp ch)
                {
                    var lit = new CharLiteralExp { Annotations = new Dictionary<string, object>(), Location = ch.Location, Parent = null, Value = ch.Value, Type = ch.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, lit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = ch.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = ch.Location },
                        RHS = createLiteralCall,
                        Location = ch.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is BooleanLiteralExp bl)
                {
                    var boolLit = new BooleanLiteralExp { Annotations = new Dictionary<string, object>(), Location = bl.Location, Parent = null, Value = bl.Value, Type = bl.Type };
                    var createLiteralCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, boolLit },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" },
                        Location = bl.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = bl.Location },
                        RHS = createLiteralCall,
                        Location = bl.Location
                    };
                }
                else if (asrt.Assertion?.ObjectExp is UriLiteralExp oUri)
                {
                    var objUriLiteral = new StringLiteralExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        Location = oUri.Location,
                        Parent = null,
                        Value = oUri.Value.AbsoluteUri
                    };
                    var objCreateUriCall = new FuncCallExp
                    {
                        InvocationArguments = new List<Expression> { graphBuilder, objUriLiteral },
                        Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" },
                        Location = oUri.Location,
                        Parent = null
                    };
                    objNode = new MemberAccessExp
                    {
                        Annotations = new Dictionary<string, object>(),
                        LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = oUri.Location },
                        RHS = objCreateUriCall,
                        Location = oUri.Location
                    };
                }
                else
                {
                    // Fallback: attempt to use expression as-is
                    objNode = asrt.Assertion?.ObjectExp ?? new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = s.Location };
                }

                // KG.CreateTriple(subjNode, predNode, objNode)
                var createTripleCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { subjNode, predNode, objNode },
                    Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateTriple" },
                    Location = s.Location,
                    Parent = null
                };
                var createTripleExpr = new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location },
                    RHS = createTripleCall,
                    Location = s.Location
                };

                // KG.Assert(graphBuilder, triple)
                var assertCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { graphBuilder, createTripleExpr },
                    Annotations = new Dictionary<string, object> { ["FunctionName"] = "Assert" },
                    Location = s.Location,
                    Parent = null
                };
                graphBuilder = new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location },
                    RHS = assertCall,
                    Location = s.Location
                };
            }
            else
            {
                // Non-assertion statements are ignored in graph-building context for now
            }
        }

        // Finally: KG.SaveGraph(KG.ConnectToRemoteStore(uri), graphBuilder)
        var kgVarForSave = new VarRefExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = ctx.Location,
            Parent = null,
            VarName = "KG"
        };
        var saveCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { connectExpr, graphBuilder },
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
