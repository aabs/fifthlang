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
    /// Check if expression is a triple literal
    /// </summary>
    private static bool IsTriple(Expression? expr) => expr is TripleLiteralExp;

    /// <summary>
    /// Check if expression is graph-like (Graph literal, GraphAssertionBlockExp, or typed as graph)
    /// </summary>
    private static bool IsGraph(Expression? expr)
    {
        if (expr is null)
            return false;

        // Graph literals and assertion blocks are always graph-like
        if (expr is Graph || expr is GraphAssertionBlockExp)
            return true;

        // Check if variable reference is typed as a graph
        if (expr is VarRefExp varRef && varRef.Type is FifthType.TType ttype)
        {
            return ttype.Name.Value != null && 
                   ttype.Name.Value.Equals("graph", StringComparison.OrdinalIgnoreCase);
        }

        // Member access expressions that represent graph operations
        if (expr is MemberAccessExp)
            return true;

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
        if (IsTriple(expr) && expr is TripleLiteralExp triple)
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

            // g.Assert(triple)
            var graphRef = new VarRefExp { VarName = tmpName, Location = loc };
            var assertStmt = MakeAssertStatement(graphRef, triple, loc);
            prologue.Add(assertStmt);

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
    /// Create statement: graphExpr.Assert(triple)
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

        // Only lower addition if at least one operand is triple or graph-like
        if (ctx.Operator == Operator.ArithmeticAdd)
        {
            bool leftIsTriple = IsTriple(lhs);
            bool rightIsTriple = IsTriple(rhs);
            bool leftIsGraph = IsGraph(lhs);
            bool rightIsGraph = IsGraph(rhs);

            if (leftIsTriple || rightIsTriple || leftIsGraph || rightIsGraph)
            {
                // Case 1: triple + triple
                if (leftIsTriple && rightIsTriple)
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

                    // g.Assert(lhs)
                    var graphRef = new VarRefExp { VarName = tmpName, Location = loc };
                    prologue.Add(MakeAssertStatement(graphRef, (TripleLiteralExp)lhs, loc));
                    
                    // g.Assert(rhs)
                    prologue.Add(MakeAssertStatement(graphRef, (TripleLiteralExp)rhs, loc));

                    // Result: VarRefExp(g) with graph type
                    var resultVarRef = new VarRefExp 
                    { 
                        VarName = tmpName, 
                        Location = loc,
                        Type = new FifthType.TType { Name = TypeName.From("graph") }
                    };
                    return new RewriteResult(resultVarRef, prologue);
                }

                // Case 2: graph + triple
                if (leftIsGraph && rightIsTriple)
                {
                    // Ensure LHS is a graph reference
                    var (graphExpr, ensurePrologue) = EnsureGraph(lhs, loc);
                    prologue.AddRange(ensurePrologue);

                    // graphLHS.Assert(triple)
                    prologue.Add(MakeAssertStatement(graphExpr, (TripleLiteralExp)rhs, loc));

                    // Result: VarRefExp to the graph
                    return new RewriteResult(graphExpr, prologue);
                }

                // Case 3: triple + graph
                if (leftIsTriple && rightIsGraph)
                {
                    // Ensure LHS is a graph (create temp, assert lhs triple)
                    var (graphLHS, ensurePrologue) = EnsureGraph(lhs, loc);
                    prologue.AddRange(ensurePrologue);

                    // If RHS is a Graph literal, expand as per-triple Assert
                    if (rhs is Graph graphLiteral)
                    {
                        prologue.AddRange(EmitAssertTriples(graphLHS, graphLiteral, loc));
                    }
                    else
                    {
                        // Non-literal RHS: use Merge
                        prologue.Add(MakeMergeStatement(graphLHS, rhs, loc));
                    }

                    // Result: VarRefExp to graphLHS
                    return new RewriteResult(graphLHS, prologue);
                }

                // Case 4: graph + graph
                if (leftIsGraph && rightIsGraph)
                {
                    // Ensure LHS is a graph reference
                    var (graphLHS, ensurePrologue) = EnsureGraph(lhs, loc);
                    prologue.AddRange(ensurePrologue);

                    // If RHS is a Graph literal, expand as per-triple Assert
                    if (rhs is Graph graphLiteral)
                    {
                        prologue.AddRange(EmitAssertTriples(graphLHS, graphLiteral, loc));
                    }
                    else
                    {
                        // Non-literal RHS: use Merge
                        prologue.Add(MakeMergeStatement(graphLHS, rhs, loc));
                    }

                    // Result: VarRefExp to graphLHS
                    return new RewriteResult(graphLHS, prologue);
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
