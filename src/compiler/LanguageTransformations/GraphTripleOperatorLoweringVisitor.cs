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
            Annotations = new Dictionary<string, object> 
            { 
                ["FunctionName"] = "Merge",
                ["ExternalType"] = typeof(Fifth.System.KG)
            },
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
                ["ExternalType"] = typeof(Fifth.System.KG)
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
                ["ExternalType"] = typeof(Fifth.System.KG)
            },
            Location = loc,
            Parent = null
        };
        return call;
    }

    private Expression CreateUriNodeExpression(Expression graphExpr, UriLiteralExp? uriExp, SourceLocationMetadata loc)
    {
        if (uriExp is UriLiteralExp uri)
        {
            var uriLiteral = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = uri.Location, Parent = null, Value = uri.Value.AbsoluteUri };
            var createUri = new FuncCallExp 
            { 
                InvocationArguments = new List<Expression> { graphExpr, uriLiteral }, 
                Annotations = new Dictionary<string, object> 
                { 
                    ["FunctionName"] = "CreateUri",
                    ["ExternalType"] = typeof(Fifth.System.KG)
                }, 
                Location = uri.Location, 
                Parent = null 
            };
            return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = uri.Location }, RHS = createUri, Location = uri.Location };
        }
        return new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = loc };
    }

    private Expression CreateObjectNodeExpression(Expression graphExpr, Expression? objExp, SourceLocationMetadata loc)
    {
        switch (objExp)
        {
            case StringLiteralExp s:
                var lit = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = s.Location, Parent = null, Value = s.Value };
                var createLiteralCall = new FuncCallExp 
                { 
                    InvocationArguments = new List<Expression> { graphExpr, lit }, 
                    Annotations = new Dictionary<string, object> 
                    { 
                        ["FunctionName"] = "CreateLiteral",
                        ["ExternalType"] = typeof(Fifth.System.KG)
                    }, 
                    Location = s.Location, 
                    Parent = null 
                };
                return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location }, RHS = createLiteralCall, Location = s.Location };
            case Int32LiteralExp i32:
                var iLit = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = i32.Location, Parent = null, Value = i32.Value };
                var createIntLit = new FuncCallExp 
                { 
                    InvocationArguments = new List<Expression> { graphExpr, iLit }, 
                    Annotations = new Dictionary<string, object> 
                    { 
                        ["FunctionName"] = "CreateLiteral",
                        ["ExternalType"] = typeof(Fifth.System.KG)
                    }, 
                    Location = i32.Location, 
                    Parent = null 
                };
                return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i32.Location }, RHS = createIntLit, Location = i32.Location };
            default:
                return new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = loc };
        }
    }

    private Expression LowerTripleToAssertChain(Expression createGraphExpr, TripleLiteralExp triple, HashSet<string> dedupe)
    {
        var signature = TryComputeTripleSignature(triple);
        if (signature != null && !dedupe.Add(signature))
        {
            return createGraphExpr;
        }

        var loc = triple.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
        var subjNode = CreateUriNodeExpression(createGraphExpr, triple.SubjectExp, loc);
        var predNode = CreateUriNodeExpression(createGraphExpr, triple.PredicateExp, loc);
        var objNode = CreateObjectNodeExpression(createGraphExpr, triple.ObjectExp, loc);

        // Create triple: KG.CreateTriple(subjNode,predNode,objNode)
        var createTripleCall = new FuncCallExp 
        { 
            InvocationArguments = new List<Expression> { subjNode, predNode, objNode }, 
            Annotations = new Dictionary<string, object> 
            { 
                ["FunctionName"] = "CreateTriple",
                ["ExternalType"] = typeof(Fifth.System.KG)
            }, 
            Location = triple.Location, 
            Parent = null 
        };
        var tripleExpr = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = triple.Location }, RHS = createTripleCall, Location = triple.Location };

        // Assert: <createGraphExpr>.Assert(tripleExpr)
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
        return new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = createGraphExpr, RHS = assertCall, Location = triple.Location };
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

                Expression builder;
                if (TryExtractLoweredGraph(lhs, dedupe, out var reusedBuilder))
                {
                    builder = reusedBuilder;
                }
                else
                {
                    builder = MakeKgCreateGraphExpression(loc);
                    if (leftIsGraphLike && lhs != null)
                    {
                        builder = AppendGraphOperand(builder, lhs, loc, dedupe);
                    }
                    else if (leftIsTriple && lhs is TripleLiteralExp leftTriple)
                    {
                        builder = LowerTripleToAssertChain(builder, leftTriple, dedupe);
                    }
                }

                if (rhs != null)
                {
                    if (rhs is BinaryExp rightLowered && rightLowered.Annotations != null && rightLowered.Annotations.ContainsKey(LoweredGraphAnnotation))
                    {
                        AppendSignaturesFromAnnotations(rightLowered.Annotations, dedupe);
                        builder = AppendGraphOperand(builder, rightLowered.RHS ?? rhs, loc, dedupe);
                    }
                    else if (rightIsGraphLike)
                    {
                        builder = AppendGraphOperand(builder, rhs, loc, dedupe);
                    }
                    else if (rightIsTriple && rhs is TripleLiteralExp rightTriple)
                    {
                        builder = LowerTripleToAssertChain(builder, rightTriple, dedupe);
                    }
                }

                if (builder is MemberAccessExp member)
                {
                    var annotations = member.Annotations != null
                        ? new Dictionary<string, object>(member.Annotations)
                        : new Dictionary<string, object>();
                    annotations["GraphExpr"] = true;
                    builder = member with { Annotations = annotations };
                }

                var placeholderLeft = new Int32LiteralExp
                {
                    Annotations = new Dictionary<string, object>(),
                    Location = loc,
                    Value = 0,
                    Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") }
                };

                var loweredAnnotations = result.Annotations != null
                    ? new Dictionary<string, object>(result.Annotations)
                    : new Dictionary<string, object>();
                loweredAnnotations[LoweredGraphAnnotation] = true;
                loweredAnnotations[TripleSignaturesAnnotation] = new List<string>(dedupe);

                var lowered = new BinaryExp
                {
                    Annotations = loweredAnnotations,
                    Location = loc,
                    LHS = placeholderLeft,
                    RHS = builder,
                    Operator = op
                };
                return lowered;
            }
        }

        // Subtraction semantics: graph - graph -> Difference(l,r), graph - triple -> Retract
        if (op == Operator.ArithmeticSubtract)
        {
            bool leftGraph = lhs is GraphAssertionBlockExp || lhs is VarRefExp || lhs is MemberAccessExp && lhs.Annotations != null && lhs.Annotations.ContainsKey("GraphExpr");
            bool rightGraph = rhs is GraphAssertionBlockExp || rhs is VarRefExp || rhs is MemberAccessExp && rhs.Annotations != null && rhs.Annotations.ContainsKey("GraphExpr");
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
                    Annotations = new Dictionary<string, object> 
                    { 
                        ["FunctionName"] = "CopyGraph",
                        ["ExternalType"] = typeof(Fifth.System.KG)
                    },
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
                    ? CreateUriNodeExpression(copyExpr, triple.SubjectExp, loc2)
                    : throw new InvalidOperationException("Triple subject is null");
                var predNode = triple.PredicateExp != null
                    ? CreateUriNodeExpression(copyExpr, triple.PredicateExp, loc2)
                    : throw new InvalidOperationException("Triple predicate is null");
                var objNode = CreateObjectNodeExpression(copyExpr, triple.ObjectExp, loc2);

                // Create triple: KG.CreateTriple(subjNode, predNode, objNode)
                var createTripleCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { subjNode, predNode, objNode },
                    Annotations = new Dictionary<string, object> 
                    { 
                        ["FunctionName"] = "CreateTriple",
                        ["ExternalType"] = typeof(Fifth.System.KG)
                    },
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
