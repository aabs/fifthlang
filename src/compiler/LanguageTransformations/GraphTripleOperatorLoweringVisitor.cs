using System;
using ast;
using System.Collections.Generic;
using System.Globalization;

namespace compiler.LanguageTransformations;

public class GraphTripleOperatorLoweringVisitor : NullSafeRecursiveDescentVisitor
{
    private static Expression AppendGraphOperand(Expression builder, Expression operand, SourceLocationMetadata loc, HashSet<string> dedupe)
    {
        Expression mergeArgument = operand;

        var mergeCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { mergeArgument },
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "Merge" },
            Location = loc,
            Parent = null
        };
        return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = builder, RHS = mergeCall, Location = loc };
    }

    private static bool IsGraphLike(Expression? expr)
    {
        if (expr is null)
        {
            return false;
        }

        // Graph assertion blocks are always graph-like
        if (expr is GraphAssertionBlockExp)
        {
            return true;
        }

        // Check if variable reference is typed as a graph
        if (expr is VarRefExp varRef)
        {
            // Only treat as graph-like if the variable is actually typed as a graph
            if (varRef.Type is FifthType.TType ttype && 
                ttype.Name.Value != null && 
                ttype.Name.Value.Equals("graph", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        // Check if it's a MemberAccessExp that represents a graph operation result
        if (expr is MemberAccessExp)
        {
            return true;
        }

        return false;
    }

    private static string? TryComputeTripleSignature(TripleLiteralExp triple)
    {
        if (triple.SubjectExp?.Value == null || triple.PredicateExp?.Value == null)
        {
            return null;
        }

        var subject = triple.SubjectExp.Value.AbsoluteUri;
        var predicate = triple.PredicateExp.Value.AbsoluteUri;
        var objSignature = EncodeTripleObject(triple.ObjectExp);
        if (objSignature == null)
        {
            return null;
        }

        return string.Concat(subject, "|", predicate, "|", objSignature);
    }

    private static string? EncodeTripleObject(Expression expression)
    {
        switch (expression)
        {
            case UriLiteralExp uri when uri.Value != null:
                return "uri:" + uri.Value.AbsoluteUri;
            case StringLiteralExp str when str.Value != null:
                return "str:" + str.Value;
            case Int32LiteralExp i32:
                return "i32:" + i32.Value.ToString(CultureInfo.InvariantCulture);
            case Int64LiteralExp i64:
                return "i64:" + i64.Value.ToString(CultureInfo.InvariantCulture);
            case Float8LiteralExp f8:
                return "f8:" + f8.Value.ToString(CultureInfo.InvariantCulture);
            case Float4LiteralExp f4:
                return "f4:" + f4.Value.ToString(CultureInfo.InvariantCulture);
            case BooleanLiteralExp b:
                return "bool:" + (b.Value ? "true" : "false");
            default:
                return null;
        }
    }

    private static MemberAccessExp MakeKgCreateGraphExpression(SourceLocationMetadata loc)
    {
        var kgVar = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc };
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
        return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = kgVar, RHS = createGraphCall, Location = loc };
    }

    private static FuncCallExp MakeDifferenceCall(Expression left, Expression right, SourceLocationMetadata loc)
    {
        var call = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { left, right },
            Annotations = new Dictionary<string, object> 
            { 
                ["FunctionName"] = "Difference",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "Difference"
            },
            Location = loc,
            Parent = null
        };
        return call;
    }

    private Expression CreateUriNodeExpression(UriLiteralExp? uriExp, SourceLocationMetadata loc)
    {
        if (uriExp is UriLiteralExp uri)
        {
            var uriLiteral = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = uri.Location, Parent = null, Value = uri.Value.AbsoluteUri };
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
            return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = uri.Location }, RHS = createUriNode, Location = uri.Location };
        }
        return new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = loc };
    }

    private Expression CreateObjectNodeExpression(Expression? objExp, SourceLocationMetadata loc)
    {
        switch (objExp)
        {
            case StringLiteralExp s:
                var lit = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = s.Location, Parent = null, Value = s.Value };
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
                return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location }, RHS = createLiteralCall, Location = s.Location };
            case Int32LiteralExp i32:
                var iLit = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = i32.Location, Parent = null, Value = i32.Value };
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
                return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i32.Location }, RHS = createIntLit, Location = i32.Location };
            default:
                return new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = loc };
        }
    }

    private Expression LowerTripleToAssertChain(Expression graphExpr, TripleLiteralExp triple, HashSet<string> dedupe)
    {
        var signature = TryComputeTripleSignature(triple);
        if (signature != null && !dedupe.Add(signature))
        {
            return graphExpr;
        }

        var loc = triple.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
        var subjNode = CreateUriNodeExpression(triple.SubjectExp, loc);
        var predNode = CreateUriNodeExpression(triple.PredicateExp, loc);
        var objNode = CreateObjectNodeExpression(triple.ObjectExp, loc);

        // Create triple: KG.CreateTriple(subjNode,predNode,objNode)
        var createTripleCall = new FuncCallExp 
        { 
            InvocationArguments = new List<Expression> { subjNode, predNode, objNode }, 
            Annotations = new Dictionary<string, object> 
            { 
                ["FunctionName"] = "CreateTriple",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "CreateTriple"
            }, 
            Location = triple.Location, 
            Parent = null 
        };
        var tripleExpr = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = triple.Location }, RHS = createTripleCall, Location = triple.Location };

        // Assert: <graphExpr>.Assert(tripleExpr)
        var assertCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { tripleExpr },
            Annotations = new Dictionary<string, object>
            {
                ["FunctionName"] = "Assert",
                ["ExternalType"] = typeof(Fifth.System.KG),
                ["ExternalMethodName"] = "Assert"
            },
            Location = triple.Location,
            Parent = null
        };
        return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = graphExpr, RHS = assertCall, Location = triple.Location };
    }

    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        var result = base.VisitBinaryExp(ctx);
        var op = result.Operator;
        var lhs = result.LHS;
        var rhs = result.RHS;

        // Addition semantics
        if (op == Operator.ArithmeticAdd)
        {
            bool leftIsGraphLike = IsGraphLike(lhs);
            bool rightIsGraphLike = IsGraphLike(rhs);
            bool leftIsTriple = lhs is TripleLiteralExp;
            bool rightIsTriple = rhs is TripleLiteralExp;

            if (leftIsGraphLike || rightIsGraphLike || leftIsTriple || rightIsTriple)
            {
                var loc = result.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
                var dedupe = new HashSet<string>(StringComparer.Ordinal);

                // Start with CreateGraph() call
                Expression builder = MakeKgCreateGraphExpression(loc);
                
                // Process left operand
                if (leftIsGraphLike && lhs != null)
                {
                    builder = AppendGraphOperand(builder, lhs, loc, dedupe);
                }
                else if (leftIsTriple && lhs is TripleLiteralExp leftTriple)
                {
                    builder = LowerTripleToAssertChain(builder, leftTriple, dedupe);
                }

                // Process right operand - chain on top of the builder from the left operand
                if (rhs != null)
                {
                    if (rightIsGraphLike)
                    {
                        builder = AppendGraphOperand(builder, rhs, loc, dedupe);
                    }
                    else if (rightIsTriple && rhs is TripleLiteralExp rightTriple)
                    {
                        builder = LowerTripleToAssertChain(builder, rightTriple, dedupe);
                    }
                }

                // Return the fully lowered call chain directly
                // Wrap in a BinaryExp for type compatibility, but with a special marker
                // The Roslyn translator will recognize this and just translate the RHS (the call chain)
                var wrapper = new BinaryExp
                {
                    Annotations = new Dictionary<string, object> { ["FullyLowered"] = true },
                    Location = loc,
                    LHS = new VarRefExp { VarName = "__dummy__", Annotations = new Dictionary<string, object>(), Location = loc },
                    RHS = builder,
                    Operator = Operator.ArithmeticAdd
                };
                return wrapper;
            }
        }

        // Subtraction semantics: graph - graph -> Difference(l,r), graph - triple -> Retract
        if (op == Operator.ArithmeticSubtract)
        {
            bool leftGraph = IsGraphLike(lhs);
            bool rightGraph = IsGraphLike(rhs);
            bool rightIsTriple = rhs is TripleLiteralExp;

            if (leftGraph && rightGraph)
            {
                var loc2 = result.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
                var diff = MakeDifferenceCall(lhs, rhs, loc2);
                // Wrap as MemberAccessExp for chaining/graph expression (KG.Difference)
                var diffExpr = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc2 }, RHS = diff, Location = loc2 };
                diffExpr.Annotations["GraphExpr"] = true;
                var placeholderLeft = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = loc2, Value = 0, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
                var lowered = new BinaryExp { Annotations = result.Annotations ?? new Dictionary<string, object>(), Location = loc2, LHS = placeholderLeft, RHS = diffExpr, Operator = op };
                lowered.Annotations["LoweredGraphExpr"] = true;
                return lowered;
            }
            else if (leftGraph && rightIsTriple && rhs is TripleLiteralExp triple)
            {
                // graph - triple: copy graph, then retract triple
                var loc2 = result.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
                
                // Create copy: KG.CopyGraph(lhs)
                var copyCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { lhs },
                    Annotations = new Dictionary<string, object> { ["FunctionName"] = "CopyGraph" },
                    Location = loc2,
                    Parent = null
                };
                var copyExpr = new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc2 },
                    RHS = copyCall,
                    Location = loc2
                };

                // Create triple nodes
                var subjNode = triple.SubjectExp != null
                    ? CreateUriNodeExpression(triple.SubjectExp, loc2)
                    : throw new InvalidOperationException("Triple subject is null");
                var predNode = triple.PredicateExp != null
                    ? CreateUriNodeExpression(triple.PredicateExp, loc2)
                    : throw new InvalidOperationException("Triple predicate is null");
                var objNode = CreateObjectNodeExpression(triple.ObjectExp, loc2);

                // Create triple: KG.CreateTriple(subjNode, predNode, objNode)
                var createTripleCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { subjNode, predNode, objNode },
                    Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateTriple" },
                    Location = loc2,
                    Parent = null
                };
                var tripleExpr = new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc2 },
                    RHS = createTripleCall,
                    Location = loc2
                };

                // Retract: copyExpr.Retract(tripleExpr)
                var retractCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { tripleExpr },
                    Annotations = new Dictionary<string, object>
                    {
                        ["FunctionName"] = "Retract",
                        ["ExternalType"] = typeof(Fifth.System.KG),
                        ["ExternalMethodName"] = "Retract"
                    },
                    Location = loc2,
                    Parent = null
                };
                var retractExpr = new MemberAccessExp
                {
                    Annotations = new Dictionary<string, object>(),
                    LHS = copyExpr,
                    RHS = retractCall,
                    Location = loc2
                };
                retractExpr.Annotations["GraphExpr"] = true;

                var placeholderLeft = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = loc2, Value = 0, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
                var lowered = new BinaryExp { Annotations = result.Annotations ?? new Dictionary<string, object>(), Location = loc2, LHS = placeholderLeft, RHS = retractExpr, Operator = op };
                lowered.Annotations["LoweredGraphExpr"] = true;
                return lowered;
            }
        }

        return result;
    }
}
