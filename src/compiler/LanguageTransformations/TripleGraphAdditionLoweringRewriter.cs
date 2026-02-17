using System;
using System.Collections.Generic;
using System.Linq;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Rewriter-based lowering pass for triple/graph addition operations.
/// Replaces the old visitor-based approach with statement-hoisting via DefaultAstRewriter.
/// 
/// Lowering rules for BinaryExp with Operator.ArithmeticAdd:
/// 1) triple + triple → var g = CreateGraph(); g.Assert(t1); g.Assert(t2); result: VarRefExp(g)
/// 2) graph + triple → graphLHS.Assert(triple); result: VarRefExp(graphLHS)
/// 3) triple + graph → var g = CreateGraph(); g.Assert(triple); g.Merge(graphRHS); result: VarRefExp(g)
/// 4) graph + graph → graphLHS.Merge(graphRHS); result: VarRefExp(graphLHS)
/// 
/// When RHS is a Graph literal, expand to per-triple Assert calls.
/// </summary>
public class TripleGraphAdditionLoweringRewriter : DefaultAstRewriter
{
    private int _tmpCounter = 0;

    /// <summary>
    /// Check if expression represents a triple (literal or lowered KG.CreateTriple call)
    /// </summary>
    private static bool IsTriple(Expression? expr)
    {
        if (expr is TripleLiteralExp) return true;
        if (expr is MemberAccessExp ma && TryGetKGFunctionName(ma, out var fn)
            && string.Equals(fn, "CreateTriple", StringComparison.Ordinal))
        {
            return true;
        }
        // If we already have type info, detect variables typed as 'triple' or 'Triple'
        if (expr is VarRefExp vr && vr.Type is FifthType.TType ttype &&
            ttype.Name.Value != null && 
            (ttype.Name.Value.Equals("triple", StringComparison.OrdinalIgnoreCase) ||
             ttype.Name.Value.Equals("Triple", StringComparison.Ordinal)))
        {
            return true;
        }
        // Fallback: check if VariableDecl suggests this is a triple
        if (expr is VarRefExp vr2 && vr2.VariableDecl != null)
        {
            var tn = vr2.VariableDecl.TypeName.Value;
            if (!string.IsNullOrEmpty(tn) &&
                (tn.Equals("triple", StringComparison.OrdinalIgnoreCase) ||
                 tn.Equals("Triple", StringComparison.Ordinal)))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Try to extract the KG function name (Annotations["FunctionName"]) from a MemberAccessExp.
    /// Returns true when RHS is a FuncCallExp with a string FunctionName annotation.
    /// </summary>
    private static bool TryGetKGFunctionName(MemberAccessExp ma, out string functionName)
    {
        functionName = string.Empty;
        if (ma.RHS is FuncCallExp call && call.Annotations is not null
            && call.Annotations.TryGetValue("FunctionName", out var fnObj)
            && fnObj is string fn && !string.IsNullOrWhiteSpace(fn))
        {
            functionName = fn;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if expression is graph-like (Graph literal or typed as graph)
    /// </summary>
    private static bool IsGraph(Expression? expr)
    {
        if (expr is null)
            return false;

        // Graph literals are always graph-like
        if (expr is Graph)
            return true;

        // Check if variable reference is typed as a graph (could be "graph" or "IGraph")
        if (expr is VarRefExp varRef && varRef.Type is FifthType.TType ttype)
        {
            return ttype.Name.Value != null &&
                   (ttype.Name.Value.Equals("graph", StringComparison.OrdinalIgnoreCase) ||
                    ttype.Name.Value.Equals("IGraph", StringComparison.Ordinal));
        }

        // Fallback: check if VariableDecl suggests this is a graph
        if (expr is VarRefExp vr2 && vr2.VariableDecl != null)
        {
            var tn = vr2.VariableDecl.TypeName.Value;
            if (!string.IsNullOrEmpty(tn) &&
                (tn.Equals("graph", StringComparison.OrdinalIgnoreCase) ||
                 tn.Equals("IGraph", StringComparison.Ordinal)))
            {
                return true;
            }
        }

        // Check if this looks like a KG.CreateGraph() call
        if (expr is MemberAccessExp ma && TryGetKGFunctionName(ma, out var fn)
            && string.Equals(fn, "CreateGraph", StringComparison.Ordinal))
        {
            return true;
        }

        // Avoid blindly treating MemberAccessExp as graph; rely on type info instead.
        // Some member-access nodes (e.g., KG.CreateTriple) are NOT graphs.
        // When type info is available and indicates 'graph', consider it graph-like.
        if (expr is MemberAccessExp ma2 && ma2.Type is FifthType.TType ttype2)
        {
            return ttype2.Name.Value != null &&
                   (ttype2.Name.Value.Equals("graph", StringComparison.OrdinalIgnoreCase) ||
                    ttype2.Name.Value.Equals("IGraph", StringComparison.Ordinal));
        }

        return false;
    }

    /// <summary>
    /// Ensure an expression is available as a graph reference.
    /// Returns (graphExpr, prologue) where:
    /// - graphExpr is an expression that evaluates to a graph
    /// - prologue contains statements that must be hoisted
    /// </summary>
    private (Expression graphExpr, List<Statement> prologue) EnsureGraph(Expression expr, SourceLocationMetadata loc)
    {
        var prologue = new List<Statement>();

        // If already graph-like, return as-is
        if (IsGraph(expr))
        {
            return (expr, prologue);
        }

        // If it's a triple, create a new graph and assert it
        if (IsTriple(expr))
        {
            var tmpName = $"__graph{_tmpCounter++}";

            // var g: graph = CreateGraph()
            var tmpDecl = new VariableDecl
            {
                Name = tmpName,
                TypeName = TypeName.From("graph"),
                CollectionType = CollectionType.SingleInstance,
                Visibility = Visibility.Private,
                Location = loc,
                Annotations = new Dictionary<string, object>()
            };

            var createGraphCall = MakeCreateGraphCall(loc);
            var declStmt = new VarDeclStatement
            {
                VariableDecl = tmpDecl,
                InitialValue = createGraphCall,
                Location = loc,
                Annotations = new Dictionary<string, object>()
            };
            prologue.Add(declStmt);

            // g.Assert(<triple-expr>)
            var graphRef = new VarRefExp { VarName = tmpName, Location = loc };
            var tripleExpr = GetTripleExpression(expr, loc);
            if (tripleExpr != null)
            {
                var assertStmt = MakeAssertStatement(graphRef, tripleExpr, loc);
                prologue.Add(assertStmt);
            }

            var graphVarRef = new VarRefExp
            {
                VarName = tmpName,
                Location = loc,
                Type = new FifthType.TType { Name = TypeName.From("graph") }
            };
            return (graphVarRef, prologue);
        }

        // Not triple or graph - return as-is (caller will decide not to lower)
        return (expr, prologue);
    }

    /// <summary>
    /// Create KG.CreateGraph() call
    /// </summary>
    private static Expression MakeCreateGraphCall(SourceLocationMetadata loc)
    {
        var kgVar = new VarRefExp
        {
            VarName = "KG",
            Annotations = new Dictionary<string, object>(),
            Location = loc
        };

        var createGraphCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression>(),
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "CreateGraph",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "CreateGraph"
            },
            Location = loc,
            Parent = null
        };

        return new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = kgVar,
            RHS = createGraphCall,
            Location = loc
        };
    }

    /// <summary>
    /// Create nodes for triple components (subject, predicate, object)
    /// </summary>
    private Expression CreateTripleExpression(TripleLiteralExp triple, SourceLocationMetadata loc)
    {
        var subjNode = CreateUriNodeExpression(triple.SubjectExp, loc);
        var predNode = CreateUriNodeExpression(triple.PredicateExp, loc);
        var objNode = CreateObjectNodeExpression(triple.ObjectExp, loc);

        // KG.CreateTriple(subjNode, predNode, objNode)
        var createTripleCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { subjNode, predNode, objNode },
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "CreateTriple",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "CreateTriple"
            },
            Location = loc,
            Parent = null
        };

        return new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc },
            RHS = createTripleCall,
            Location = loc
        };
    }

    private Expression CreateUriNodeExpression(UriLiteralExp? uriExp, SourceLocationMetadata loc)
    {
        if (uriExp is UriLiteralExp uri && uri.Value != null)
        {
            var uriLiteral = new StringLiteralExp
            {
                Annotations = new Dictionary<string, object>(),
                Location = uri.Location,
                Parent = null,
                Value = uri.Value.AbsoluteUri
            };

            var createUriNode = new FuncCallExp
            {
                InvocationArguments = new List<Expression> { uriLiteral },
                Annotations = new Dictionary<string, object>
                {
                    ["FunctionName"] = "CreateUriNode",
                    ["ExternalType"] = typeof(Fifth.System.KG),
                    ["ExternalMethodName"] = "CreateUriNode"
                },
                Location = uri.Location,
                Parent = null
            };

            return new MemberAccessExp
            {
                Annotations = new Dictionary<string, object>(),
                LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = uri.Location },
                RHS = createUriNode,
                Location = uri.Location
            };
        }

        return new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = loc };
    }

    private Expression CreateObjectNodeExpression(Expression? objExp, SourceLocationMetadata loc)
    {
        switch (objExp)
        {
            case UriLiteralExp uri:
                return CreateUriNodeExpression(uri, loc);

            case StringLiteralExp s:
                var lit = new StringLiteralExp
                {
                    Annotations = new Dictionary<string, object>(),
                    Location = s.Location,
                    Parent = null,
                    Value = s.Value
                };

                var createLiteralCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { lit },
                    Annotations = new Dictionary<string, object>
                    {
                        ["FunctionName"] = "CreateLiteralNode",
                        ["ExternalType"] = typeof(Fifth.System.KG),
                        ["ExternalMethodName"] = "CreateLiteralNode"
                    },
                    Location = s.Location,
                    Parent = null
                };

                return new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location },
                    RHS = createLiteralCall,
                    Location = s.Location
                };

            case Int32LiteralExp i32:
                var iLit = new Int32LiteralExp
                {
                    Annotations = new Dictionary<string, object>(),
                    Location = i32.Location,
                    Parent = null,
                    Value = i32.Value
                };

                var createIntLit = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { iLit },
                    Annotations = new Dictionary<string, object>
                    {
                        ["FunctionName"] = "CreateLiteralNode",
                        ["ExternalType"] = typeof(Fifth.System.KG),
                        ["ExternalMethodName"] = "CreateLiteralNode"
                    },
                    Location = i32.Location,
                    Parent = null
                };

                return new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i32.Location },
                    RHS = createIntLit,
                    Location = i32.Location
                };

            default:
                return new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = loc };
        }
    }

    /// <summary>
    /// Create statement: graphExpr.Assert(tripleLiteral)
    /// </summary>
    private ExpStatement MakeAssertStatement(Expression graphExpr, TripleLiteralExp triple, SourceLocationMetadata loc)
    {
        var tripleExpr = CreateTripleExpression(triple, loc);

        var assertCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { tripleExpr },
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "Assert",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "Assert"
            },
            Location = loc,
            Parent = null
        };

        var assertExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = graphExpr,
            RHS = assertCall,
            Location = loc
        };

        return new ExpStatement { RHS = assertExpr, Location = loc, Annotations = new Dictionary<string, object>() };
    }

    /// <summary>
    /// Create statement: graphExpr.Assert(<tripleExpr>) where tripleExpr is already a KG.CreateTriple expression
    /// </summary>
    private ExpStatement MakeAssertStatement(Expression graphExpr, Expression tripleExpr, SourceLocationMetadata loc)
    {
        var assertCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { tripleExpr },
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "Assert",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "Assert"
            },
            Location = loc,
            Parent = null
        };

        var assertExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = graphExpr,
            RHS = assertCall,
            Location = loc
        };

        return new ExpStatement { RHS = assertExpr, Location = loc, Annotations = new Dictionary<string, object>() };
    }

    /// <summary>
    /// Create statement: graphLHS.Merge(graphRHS)
    /// </summary>
    private ExpStatement MakeMergeStatement(Expression graphLHS, Expression graphRHS, SourceLocationMetadata loc)
    {
        var mergeCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { graphRHS },
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "Merge",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "Merge"
            },
            Location = loc,
            Parent = null
        };

        var mergeExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = graphLHS,
            RHS = mergeCall,
            Location = loc
        };

        return new ExpStatement { RHS = mergeExpr, Location = loc, Annotations = new Dictionary<string, object>() };
    }

    /// <summary>
    /// Create statement: graphExpr.Retract(tripleLiteral)
    /// </summary>
    private ExpStatement MakeRetractStatement(Expression graphExpr, TripleLiteralExp triple, SourceLocationMetadata loc)
    {
        var tripleExpr = CreateTripleExpression(triple, loc);

        var retractCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { tripleExpr },
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "Retract",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "Retract"
            },
            Location = loc,
            Parent = null
        };

        var retractExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = graphExpr,
            RHS = retractCall,
            Location = loc
        };

        return new ExpStatement { RHS = retractExpr, Location = loc, Annotations = new Dictionary<string, object>() };
    }

    /// <summary>
    /// Create statement: graphExpr.Retract(<tripleExpr>) where tripleExpr is already a KG.CreateTriple expression
    /// </summary>
    private ExpStatement MakeRetractStatement(Expression graphExpr, Expression tripleExpr, SourceLocationMetadata loc)
    {
        var retractCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { tripleExpr },
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "Retract",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "Retract"
            },
            Location = loc,
            Parent = null
        };

        var retractExpr = new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = graphExpr,
            RHS = retractCall,
            Location = loc
        };

        return new ExpStatement { RHS = retractExpr, Location = loc, Annotations = new Dictionary<string, object>() };
    }

    /// <summary>
    /// Emit multiple Assert statements for each triple in a graph literal
    /// </summary>
    private List<Statement> EmitAssertTriples(Expression graphExpr, Graph graphLiteral, SourceLocationMetadata loc)
    {
        var statements = new List<Statement>();

        if (graphLiteral.Triples != null)
        {
            foreach (var triple in graphLiteral.Triples)
            {
                statements.Add(MakeAssertStatement(graphExpr, triple, loc));
            }
        }

        return statements;
    }

    /// <summary>
    /// Given an expression that represents a triple (literal or KG.CreateTriple), return an expression that constructs it.
    /// Returns null if the expression does not represent a triple.
    /// </summary>
    private Expression? GetTripleExpression(Expression expr, SourceLocationMetadata loc)
    {
        if (expr is TripleLiteralExp tl)
        {
            return CreateTripleExpression(tl, loc);
        }
        if (expr is MemberAccessExp ma && TryGetKGFunctionName(ma, out var fn)
            && string.Equals(fn, "CreateTriple", StringComparison.Ordinal))
        {
            return expr;
        }
        return null;
    }

    /// <summary>
    /// Check if two expressions refer to the same variable (simple structural check).
    /// </summary>
    private static bool IsSameVariable(Expression? a, Expression? b)
    {
        if (a is VarRefExp varA && b is VarRefExp varB)
        {
            return string.Equals(varA.VarName, varB.VarName, StringComparison.Ordinal);
        }
        return false;
    }

    public override RewriteResult VisitAssignmentStatement(AssignmentStatement ctx)
    {
        // Rewrite LHS and RHS
        var lhsResult = Rewrite(ctx.LValue);
        var rhsResult = Rewrite(ctx.RValue);

        var prologue = new List<Statement>();
        prologue.AddRange(lhsResult.Prologue);
        prologue.AddRange(rhsResult.Prologue);

        var lhs = (Expression)lhsResult.Node;
        var rhs = (Expression)rhsResult.Node;

        // Special case: If the lowering of RHS produced prologue statements (e.g., Assert calls)
        // and the RHS result expression is the same as LHS (e.g., g = g), 
        // then the assignment is a no-op and we should omit it.
        // This happens when `g += triple` is desugared to `g = g + triple` and then
        // `g + triple` is lowered to `Assert(g, triple); result: g`.
        if (rhsResult.Prologue.Any() && IsSameVariable(lhs, rhs))
        {
            // The assignment is a no-op (g = g), but the prologue contains the actual operations.
            // Return an empty statement with the prologue.
            var loc = ctx.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
            var emptyStmt = new EmptyStatement
            {
                Location = loc,
                Annotations = ctx.Annotations ?? new Dictionary<string, object>()
            };
            return new RewriteResult(emptyStmt, prologue);
        }

        // Normal case: rebuild the assignment with rewritten LHS and RHS
        var rebuilt = ctx with
        {
            LValue = lhs,
            RValue = rhs
        };
        return new RewriteResult(rebuilt, prologue);
    }

    public override RewriteResult VisitBinaryExp(BinaryExp ctx)
    {
        // Rewrite children first
        var lhsResult = Rewrite(ctx.LHS);
        var rhsResult = Rewrite(ctx.RHS);

        var prologue = new List<Statement>();
        prologue.AddRange(lhsResult.Prologue);
        prologue.AddRange(rhsResult.Prologue);

        var lhs = (Expression)lhsResult.Node;
        var rhs = (Expression)rhsResult.Node;
        var loc = ctx.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);

        // Helper: detect when this binary expression is expected to yield a graph (e.g., in a graph-typed variable initializer)
        bool ExpectedGraphResult()
        {
            try
            {
                if (ctx.Parent is VarDeclStatement vds && vds.VariableDecl != null)
                {
                    var tn = vds.VariableDecl.TypeName.Value;
                    if (!string.IsNullOrEmpty(tn))
                    {
                        return tn.Equals("graph", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch { /* best-effort heuristic */ }
            return false;
        }

        // Lower addition/subtraction for graph/triple composition if any operand indicates triple/graph intent
        if (ctx.Operator == Operator.ArithmeticAdd || ctx.Operator == Operator.ArithmeticSubtract)
        {
            bool leftIsTriple = IsTriple(lhs);
            bool rightIsTriple = IsTriple(rhs);
            bool leftIsGraph = IsGraph(lhs);
            bool rightIsGraph = IsGraph(rhs);
            bool expectedGraph = ExpectedGraphResult();
            
            // Check if this binary expression came from an augmented assignment (g += x or g -= x)
            // In such cases, we're more confident this is a graph/triple operation
            bool fromAugmentedAssignment = ctx.Annotations != null && 
                                          ctx.Annotations.ContainsKey("FromAugmentedAssignment");
            
            
            // Additional heuristic: If we can't determine types, but one side is clearly a triple literal
            // or the result of KG.CreateTriple, then this is likely a graph/triple operation
            bool likelyGraphTripleOp = false;
            if (!leftIsTriple && !rightIsTriple && !leftIsGraph && !rightIsGraph)
            {
                // Check if RHS is definitely a triple (literal or CreateTriple call)
                if (rhs is TripleLiteralExp || 
                    (rhs is MemberAccessExp rhsMa && TryGetKGFunctionName(rhsMa, out var rhsFn) && 
                     string.Equals(rhsFn, "CreateTriple", StringComparison.Ordinal)))
                {
                    likelyGraphTripleOp = true;
                }
                // Check if LHS is definitely a graph (CreateGraph call)
                else if (lhs is MemberAccessExp lhsMa && TryGetKGFunctionName(lhsMa, out var lhsFn) && 
                         string.Equals(lhsFn, "CreateGraph", StringComparison.Ordinal))
                {
                    likelyGraphTripleOp = true;
                }
                // If this came from augmented assignment and we have two VarRefs, assume it's graph/triple
                // This handles cases like `g += triple` where g and triple are variables without type info
                else if (fromAugmentedAssignment && lhs is VarRefExp && rhs is VarRefExp)
                {
                    likelyGraphTripleOp = true;
                }
            }
            

            if (leftIsTriple || rightIsTriple || leftIsGraph || rightIsGraph || expectedGraph || likelyGraphTripleOp)
            {
                // We have detected graph/triple intent - proceed with lowering
                // Handle subtraction first: graph - triple => Retract
                if (ctx.Operator == Operator.ArithmeticSubtract)
                {
                    if (rightIsTriple)
                    {
                        var (graphExpr, ensurePrologue) = EnsureGraph(lhs, loc);
                        prologue.AddRange(ensurePrologue);
                        var tripleExpr = GetTripleExpression(rhs, loc);
                        if (tripleExpr is not null)
                        {
                            prologue.Add(MakeRetractStatement(graphExpr, tripleExpr, loc));
                            return new RewriteResult(graphExpr, prologue);
                        }
                        // If triple expression couldn't be recovered but LHS is graph-like,
                        // assume RHS is a triple-typed expression and emit Retract(graph, rhs)
                        if (leftIsGraph)
                        {
                            prologue.Add(MakeRetractStatement(graphExpr, rhs, loc));
                            return new RewriteResult(graphExpr, prologue);
                        }
                        // If not graph-like, fall-through to preserve subtraction
                        return new RewriteResult(ctx with { LHS = lhs, RHS = rhs }, prologue);
                    }
                    // Fallback: preserve subtraction for other types
                }

                // Addition cases
                if (ctx.Operator == Operator.ArithmeticAdd)
                {
                    // Contextual fallback: if the expected result is a graph (e.g., g: graph = a + b),
                    // ensure LHS is a graph and then merge/assert RHS accordingly, even if types aren't annotated yet.
                    if (expectedGraph && !leftIsTriple && !rightIsTriple && !leftIsGraph && !rightIsGraph)
                    {
                        var (graphLHS, ensurePrologue) = EnsureGraph(lhs, loc);
                        prologue.AddRange(ensurePrologue);

                        // Prefer merge semantics assuming RHS is a graph variable; if it's a graph literal, expand; if triple, assert.
                        if (rhs is Graph graphLiteral0)
                        {
                            prologue.AddRange(EmitAssertTriples(graphLHS, graphLiteral0, loc));
                        }
                        else if (IsTriple(rhs))
                        {
                            var tripleExpr0 = GetTripleExpression(rhs, loc) ?? rhs;
                            prologue.Add(MakeAssertStatement(graphLHS, tripleExpr0, loc));
                        }
                        else
                        {
                            // Assume RHS is a graph-typed expression and emit Merge
                            prologue.Add(MakeMergeStatement(graphLHS, rhs, loc));
                        }
                        return new RewriteResult(graphLHS, prologue);
                    }

                    // Case: triple + triple => new graph with both asserted
                    if (leftIsTriple && rightIsTriple)
                    {
                        var tmpName = $"__graph{_tmpCounter++}";

                        var tmpDecl = new VariableDecl
                        {
                            Name = tmpName,
                            TypeName = TypeName.From("graph"),
                            CollectionType = CollectionType.SingleInstance,
                            Visibility = Visibility.Private,
                            Location = loc,
                            Annotations = new Dictionary<string, object>()
                        };

                        var createGraphCall = MakeCreateGraphCall(loc);
                        var declStmt = new VarDeclStatement
                        {
                            VariableDecl = tmpDecl,
                            InitialValue = createGraphCall,
                            Location = loc,
                            Annotations = new Dictionary<string, object>()
                        };
                        prologue.Add(declStmt);

                        var graphRef = new VarRefExp { VarName = tmpName, Location = loc };
                        var leftTripleExpr = GetTripleExpression(lhs, loc);
                        var rightTripleExpr = GetTripleExpression(rhs, loc);
                        if (leftTripleExpr != null) prologue.Add(MakeAssertStatement(graphRef, leftTripleExpr, loc));
                        if (rightTripleExpr != null) prologue.Add(MakeAssertStatement(graphRef, rightTripleExpr, loc));

                        var resultVarRef = new VarRefExp
                        {
                            VarName = tmpName,
                            Location = loc,
                            Type = new FifthType.TType { Name = TypeName.From("graph") }
                        };
                        return new RewriteResult(resultVarRef, prologue);
                    }

                    // Case: <any> + triple => ensure LHS treated as graph, then Assert
                    if (rightIsTriple)
                    {
                        var (graphExpr, ensurePrologue) = EnsureGraph(lhs, loc);
                        prologue.AddRange(ensurePrologue);
                        var tripleExpr = GetTripleExpression(rhs, loc);
                        if (tripleExpr is not null)
                        {
                            prologue.Add(MakeAssertStatement(graphExpr, tripleExpr, loc));
                        }
                        else if (leftIsGraph)
                        {
                            // If triple expression couldn't be recovered but LHS is graph-like,
                            // assume RHS is a triple expression and emit Assert(graph, rhs)
                            prologue.Add(MakeAssertStatement(graphExpr, rhs, loc));
                        }
                        return new RewriteResult(graphExpr, prologue);
                    }

                    // Case: triple + graph-like => ensure LHS graph from triple, then Merge/expand RHS
                    if (leftIsTriple && rightIsGraph)
                    {
                        var (graphLHS, ensurePrologue) = EnsureGraph(lhs, loc);
                        prologue.AddRange(ensurePrologue);

                        if (rhs is Graph graphLiteral2)
                        {
                            prologue.AddRange(EmitAssertTriples(graphLHS, graphLiteral2, loc));
                        }
                        else
                        {
                            prologue.Add(MakeMergeStatement(graphLHS, rhs, loc));
                        }
                        return new RewriteResult(graphLHS, prologue);
                    }

                    // Case: <any> + graph-like => ensure LHS is graph (pass-through if already var), then Merge/expand RHS
                    if (rightIsGraph)
                    {
                        var (graphLHS, ensurePrologue) = EnsureGraph(lhs, loc);
                        prologue.AddRange(ensurePrologue);

                        if (rhs is Graph graphLiteral3)
                        {
                            prologue.AddRange(EmitAssertTriples(graphLHS, graphLiteral3, loc));
                        }
                        else
                        {
                            prologue.Add(MakeMergeStatement(graphLHS, rhs, loc));
                        }
                        return new RewriteResult(graphLHS, prologue);
                    }

                    // Additional heuristic: if LHS is graph-like and RHS is not graph-like but we're here
                    // due to graph/triple intent, treat RHS as a triple expression and Assert it.
                    if (leftIsGraph)
                    {
                        var (graphExpr, ensurePrologue) = EnsureGraph(lhs, loc);
                        prologue.AddRange(ensurePrologue);
                        prologue.Add(MakeAssertStatement(graphExpr, rhs, loc));
                        return new RewriteResult(graphExpr, prologue);
                    }
                    
                    // Fallback for likelyGraphTripleOp: when we believe this is a graph/triple operation
                    // but can't determine specific types, assume LHS is graph and RHS is triple
                    if (likelyGraphTripleOp && !leftIsTriple && !rightIsTriple && !leftIsGraph && !rightIsGraph)
                    {
                        // Assume LHS is graph-like, RHS is triple-like
                        prologue.Add(MakeAssertStatement(lhs, rhs, loc));
                        return new RewriteResult(lhs, prologue);
                    }
                }
                
                // Fallback for subtraction with likelyGraphTripleOp
                if (ctx.Operator == Operator.ArithmeticSubtract && likelyGraphTripleOp && 
                    !leftIsTriple && !rightIsTriple && !leftIsGraph && !rightIsGraph)
                {
                    prologue.Add(MakeRetractStatement(lhs, rhs, loc));
                    return new RewriteResult(lhs, prologue);
                }
            }
        }

        // Not a triple/graph addition - preserve as-is
        var rewrittenBinary = ctx with
        {
            LHS = lhs,
            RHS = rhs
        };

        return new RewriteResult(rewrittenBinary, prologue);
    }
}
