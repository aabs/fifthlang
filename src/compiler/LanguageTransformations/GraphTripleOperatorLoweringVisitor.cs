using ast;
using System.Collections.Generic;

namespace compiler.LanguageTransformations;

public class GraphTripleOperatorLoweringVisitor : NullSafeRecursiveDescentVisitor
{
    private static MemberAccessExp MakeKgCreateGraphExpression(SourceLocationMetadata loc)
    {
        var kgVar = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc };
        var createGraphCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression>(),
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateGraph" },
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
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "Difference" },
            Location = loc,
            Parent = null
        };
        return call;
    }

    private Expression LowerTripleToAssertChain(Expression createGraphExpr, TripleLiteralExp triple)
    {
        // This implements: KG.CreateGraph().Assert(KG.CreateTriple(KG.CreateUri(g, subj), KG.CreateUri(g, pred), objNode))
        // Build subject node creation: KG.CreateUri(g, subj)
        Expression subjNode;
        if (triple.SubjectExp is UriLiteralExp sUri)
        {
            var subjUriLiteral = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = sUri.Location, Parent = null, Value = sUri.Value.AbsoluteUri };
            var subjCreateUri = new FuncCallExp { InvocationArguments = new List<Expression> { createGraphExpr, subjUriLiteral }, Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" }, Location = sUri.Location, Parent = null };
            subjNode = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = sUri.Location }, RHS = subjCreateUri, Location = sUri.Location };
        }
        else
        {
            // Fallback: treat subject as null-ref expression
            subjNode = new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = triple.Location };
        }

        // Predicate
        Expression predNode;
        if (triple.PredicateExp is UriLiteralExp pUri)
        {
            var predUriLiteral = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = pUri.Location, Parent = null, Value = pUri.Value.AbsoluteUri };
            var predCreateUri = new FuncCallExp { InvocationArguments = new List<Expression> { createGraphExpr, predUriLiteral }, Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateUri" }, Location = pUri.Location, Parent = null };
            predNode = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = pUri.Location }, RHS = predCreateUri, Location = pUri.Location };
        }
        else
        {
            predNode = new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = triple.Location };
        }

        // Object: use CreateLiteral for string/numeric literals, otherwise null
        Expression objNode;
        switch (triple.ObjectExp)
        {
            case StringLiteralExp s:
                var lit = new StringLiteralExp { Annotations = new Dictionary<string, object>(), Location = s.Location, Parent = null, Value = s.Value };
                var createLiteralCall = new FuncCallExp { InvocationArguments = new List<Expression> { createGraphExpr, lit }, Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" }, Location = s.Location, Parent = null };
                objNode = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = s.Location }, RHS = createLiteralCall, Location = s.Location };
                break;
            case Int32LiteralExp i32:
                var iLit = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = i32.Location, Parent = null, Value = i32.Value };
                var createIntLit = new FuncCallExp { InvocationArguments = new List<Expression> { createGraphExpr, iLit }, Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateLiteral" }, Location = i32.Location, Parent = null };
                objNode = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = i32.Location }, RHS = createIntLit, Location = i32.Location };
                break;
            default:
                objNode = new VarRefExp { VarName = "null", Annotations = new Dictionary<string, object>(), Location = triple.Location };
                break;
        }

        // Create triple: KG.CreateTriple(subjNode,predNode,objNode)
        var createTripleCall = new FuncCallExp { InvocationArguments = new List<Expression> { subjNode, predNode, objNode }, Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateTriple" }, Location = triple.Location, Parent = null };
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
        var asserted = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = createGraphExpr, RHS = assertCall, Location = triple.Location };
        return asserted;
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
            // If either side is a graph expression or triple literal, lower into a new graph builder chain
            bool leftIsGraphLike = lhs is GraphAssertionBlockExp || lhs is VarRefExp || lhs is MemberAccessExp && lhs.Annotations != null && lhs.Annotations.ContainsKey("GraphExpr");
            bool rightIsGraphLike = rhs is GraphAssertionBlockExp || rhs is VarRefExp || rhs is MemberAccessExp && rhs.Annotations != null && rhs.Annotations.ContainsKey("GraphExpr");

            bool leftIsTriple = lhs is TripleLiteralExp;
            bool rightIsTriple = rhs is TripleLiteralExp;

            if (leftIsGraphLike || rightIsGraphLike || leftIsTriple || rightIsTriple)
            {
                // Start with a fresh graph
                var loc = result.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
                var createGraphExpr = MakeKgCreateGraphExpression(loc);
                Expression builder = createGraphExpr;

                // Merge/asert left
                if (leftIsGraphLike && lhs != null)
                {
                    var mergeCall = new FuncCallExp { InvocationArguments = new List<Expression> { lhs }, Annotations = new Dictionary<string, object> { ["FunctionName"] = "Merge" }, Location = result.Location, Parent = null };
                    builder = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = builder, RHS = mergeCall, Location = result.Location };
                }
                else if (leftIsTriple && lhs is TripleLiteralExp tl)
                {
                    builder = LowerTripleToAssertChain(builder, tl);
                }

                // Merge/assert right
                if (rightIsGraphLike && rhs != null)
                {
                    var mergeCall = new FuncCallExp { InvocationArguments = new List<Expression> { rhs }, Annotations = new Dictionary<string, object> { ["FunctionName"] = "Merge" }, Location = result.Location, Parent = null };
                    builder = new MemberAccessExp { Annotations = new Dictionary<string, object>(), LHS = builder, RHS = mergeCall, Location = result.Location };
                }
                else if (rightIsTriple && rhs is TripleLiteralExp tr)
                {
                    builder = LowerTripleToAssertChain(builder, tr);
                }

                // Mark as graph expr for downstream consumers
                if (builder is MemberAccessExp m) m.Annotations["GraphExpr"] = true;

                // Return a BinaryExp that carries the lowered expression in RHS. Use a placeholder LHS literal to satisfy non-nullable constraints.
                var placeholderLeft = new Int32LiteralExp { Annotations = new Dictionary<string, object>(), Location = loc, Value = 0, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
                var lowered = new BinaryExp { Annotations = result.Annotations ?? new Dictionary<string, object>(), Location = loc, LHS = placeholderLeft, RHS = builder, Operator = op };
                lowered.Annotations["LoweredGraphExpr"] = true;
                return lowered;
            }
        }

        // Subtraction semantics: graph - graph -> Difference(l,r)
        if (op == Operator.ArithmeticSubtract)
        {
            bool leftGraph = lhs is GraphAssertionBlockExp || lhs is VarRefExp || lhs is MemberAccessExp && lhs.Annotations != null && lhs.Annotations.ContainsKey("GraphExpr");
            bool rightGraph = rhs is GraphAssertionBlockExp || rhs is VarRefExp || rhs is MemberAccessExp && rhs.Annotations != null && rhs.Annotations.ContainsKey("GraphExpr");
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
        }

        return result;
    }
}
