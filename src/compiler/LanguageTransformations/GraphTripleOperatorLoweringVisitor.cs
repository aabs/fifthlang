using System;
using ast;
using System.Collections.Generic;
using System.Globalization;

namespace compiler.LanguageTransformations;

public class GraphTripleOperatorLoweringVisitor : NullSafeRecursiveDescentVisitor
{
    private const string LoweredGraphAnnotation = "LoweredGraphExpr";
    private const string TripleSignaturesAnnotation = "TripleSignatures";

    private static void AppendSignaturesFromAnnotations(IDictionary<string, object>? annotations, HashSet<string> dedupe)
    {
        if (annotations == null)
        {
            return;
        }

        if (annotations.TryGetValue(TripleSignaturesAnnotation, out var value) && value is List<string> list)
        {
            foreach (var signature in list)
            {
                dedupe.Add(signature);
            }
        }
    }

    private static bool TryExtractLoweredGraph(Expression? operand, HashSet<string> dedupe, out Expression builder)
    {
        builder = default!;
        if (operand is not BinaryExp binary || binary.Annotations == null || !binary.Annotations.ContainsKey(LoweredGraphAnnotation))
        {
            return false;
        }

        AppendSignaturesFromAnnotations(binary.Annotations, dedupe);
        builder = binary.RHS ?? operand;
        return true;
    }

    private static Expression AppendGraphOperand(Expression builder, Expression operand, SourceLocationMetadata loc, HashSet<string> dedupe)
    {
        Expression mergeArgument = operand;
        if (operand is BinaryExp lowered && lowered.Annotations != null && lowered.Annotations.ContainsKey(LoweredGraphAnnotation))
        {
            AppendSignaturesFromAnnotations(lowered.Annotations, dedupe);
            mergeArgument = lowered.RHS ?? operand;
        }

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

        if (expr is MemberAccessExp member && member.Annotations != null && member.Annotations.ContainsKey("GraphExpr"))
        {
            return true;
        }

        return expr is BinaryExp binary && binary.Annotations != null && binary.Annotations.ContainsKey(LoweredGraphAnnotation);
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

                // Create a temporary graph variable name
                var graphVarName = "__g__";
                var graphVarRef = new VarRefExp 
                { 
                    VarName = graphVarName, 
                    Annotations = new Dictionary<string, object>(),
                    Location = loc 
                };

                Expression builder = graphVarRef;
                
                // Process left operand
                if (TryExtractLoweredGraph(lhs, dedupe, out var reusedBuilder))
                {
                    // If LHS is already a lowered graph, we need to merge it
                    builder = AppendGraphOperand(graphVarRef, lhs, loc, dedupe);
                }
                else if (leftIsGraphLike && lhs != null)
                {
                    builder = AppendGraphOperand(graphVarRef, lhs, loc, dedupe);
                }
                else if (leftIsTriple && lhs is TripleLiteralExp leftTriple)
                {
                    builder = LowerTripleToAssertChain(graphVarRef, leftTriple, dedupe);
                }

                // Process right operand
                if (rhs != null)
                {
                    if (rhs is BinaryExp rightLowered && rightLowered.Annotations != null && rightLowered.Annotations.ContainsKey(LoweredGraphAnnotation))
                    {
                        AppendSignaturesFromAnnotations(rightLowered.Annotations, dedupe);
                        builder = AppendGraphOperand(graphVarRef, rightLowered.RHS ?? rhs, loc, dedupe);
                    }
                    else if (rightIsGraphLike)
                    {
                        builder = AppendGraphOperand(graphVarRef, rhs, loc, dedupe);
                    }
                    else if (rightIsTriple && rhs is TripleLiteralExp rightTriple)
                    {
                        builder = LowerTripleToAssertChain(graphVarRef, rightTriple, dedupe);
                    }
                }

                // Mark the builder with annotations
                if (builder is MemberAccessExp member)
                {
                    var annotations = member.Annotations != null
                        ? new Dictionary<string, object>(member.Annotations)
                        : new Dictionary<string, object>();
                    annotations["GraphExpr"] = true;
                    builder = member with { Annotations = annotations };
                }

                // Create the lowered expression with special annotations
                // The LHS will be the graph initialization (CreateGraph())
                // The RHS will be the operations chain (using graph variable)
                var loweredAnnotations = result.Annotations != null
                    ? new Dictionary<string, object>(result.Annotations)
                    : new Dictionary<string, object>();
                loweredAnnotations[LoweredGraphAnnotation] = true;
                loweredAnnotations["RequiresIIFE"] = true; // Signal to Roslyn translator
                loweredAnnotations[TripleSignaturesAnnotation] = new List<string>(dedupe);
                loweredAnnotations["GraphVarName"] = graphVarName;

                var lowered = new BinaryExp
                {
                    Annotations = loweredAnnotations,
                    Location = loc,
                    LHS = MakeKgCreateGraphExpression(loc), // Graph initialization
                    RHS = builder, // Operations using graph variable
                    Operator = op
                };
                return lowered;
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
