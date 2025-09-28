using ast;
using compiler.LanguageTransformations;
using FluentAssertions;

namespace ast_tests;

public class GraphTripleOperatorLoweringTests : VisitorTestsBase
{
    [Test]
    public void Add_graph_graph_should_lower_to_builder_merge_chain()
    {
        var left = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() } };
        var right = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() } };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty), LHS = left, RHS = right, Operator = Operator.ArithmeticAdd };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        // Expect lowering marker and RHS to be a MemberAccessExp (builder chain)
        result.Annotations.Should().ContainKey("LoweredGraphExpr");
        result.RHS.Should().NotBeNull();
        result.RHS.Should().BeOfType<MemberAccessExp>();
        var rhs = (MemberAccessExp)result.RHS!;
        rhs.Annotations.Should().ContainKey("GraphExpr");
    }

    [Test]
    public void Subtract_graph_graph_should_lower_to_difference_call()
    {
        var left = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() } };
        var right = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() } };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty), LHS = left, RHS = right, Operator = Operator.ArithmeticSubtract };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        result.Annotations.Should().ContainKey("LoweredGraphExpr");
        result.RHS.Should().NotBeNull();
        result.RHS.Should().BeOfType<MemberAccessExp>();
        var rhs = (MemberAccessExp)result.RHS!;
        rhs.RHS.Should().NotBeNull();
        rhs.RHS.Should().BeOfType<FuncCallExp>();
        var fcall = (FuncCallExp)rhs.RHS!;
        fcall.Annotations.Should().ContainKey("FunctionName");
        fcall.Annotations["FunctionName"].Should().Be("Difference");
    }

    [Test]
    public void Add_graph_triple_should_lower_to_builder_merge_chain()
    {
        var left = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() } };
        var right = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
            ObjectExp = new Int32LiteralExp { Value = 42, Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty) }
        };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty), LHS = left, RHS = right, Operator = Operator.ArithmeticAdd };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        result.Annotations.Should().ContainKey("LoweredGraphExpr");
        result.RHS.Should().NotBeNull();
        result.RHS.Should().BeOfType<MemberAccessExp>();
        var rhs = (MemberAccessExp)result.RHS!;
        rhs.Annotations.Should().ContainKey("GraphExpr");
        rhs.RHS.Should().BeOfType<FuncCallExp>();
        var fcall = (FuncCallExp)rhs.RHS!;
        fcall.Annotations["FunctionName"].Should().Be("Assert");
    }

    [Test]
    public void Add_triple_triple_should_lower_to_builder_with_two_asserts()
    {
        var left = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s1") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p1") },
            ObjectExp = new StringLiteralExp { Value = "o1", Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty) }
        };
        var right = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s2") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p2") },
            ObjectExp = new StringLiteralExp { Value = "o2", Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty) }
        };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty), LHS = left, RHS = right, Operator = Operator.ArithmeticAdd };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        result.Annotations.Should().ContainKey("LoweredGraphExpr");
        result.RHS.Should().NotBeNull();
        result.RHS.Should().BeOfType<MemberAccessExp>();
        var rhs = (MemberAccessExp)result.RHS!;
        rhs.Annotations.Should().ContainKey("GraphExpr");
        // Final RHS should be Assert (the outermost assert for the second triple)
        rhs.RHS.Should().BeOfType<FuncCallExp>();
        var fcall = (FuncCallExp)rhs.RHS!;
        fcall.Annotations["FunctionName"].Should().Be("Assert");
    }

    [Test]
    public void Subtract_triple_graph_should_not_lower()
    {
        var left = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
            ObjectExp = new Int32LiteralExp { Value = 1, Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty) }
        };
        var right = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() } };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty), LHS = left, RHS = right, Operator = Operator.ArithmeticSubtract };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        // Subtraction lowering only applies when both sides are graph-like. Expect no LoweredGraphExpr annotation here.
        result.Annotations.Should().NotContainKey("LoweredGraphExpr");
        result.Operator.Should().Be(Operator.ArithmeticSubtract);
    }

    [Test]
    public void Detailed_triple_triple_lowering_chain_structure()
    {
        var left = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s1"), Location = new SourceLocationMetadata(1, "", 1, "") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p1"), Location = new SourceLocationMetadata(1, "", 1, "") },
            ObjectExp = new StringLiteralExp { Value = "o1", Location = new SourceLocationMetadata(1, "", 1, "") },
            Location = new SourceLocationMetadata(1, "", 1, "")
        };
        var right = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s2"), Location = new SourceLocationMetadata(2, "", 2, "") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p2"), Location = new SourceLocationMetadata(2, "", 2, "") },
            ObjectExp = new StringLiteralExp { Value = "o2", Location = new SourceLocationMetadata(2, "", 2, "") },
            Location = new SourceLocationMetadata(2, "", 2, "")
        };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, "", 0, ""), LHS = left, RHS = right, Operator = Operator.ArithmeticAdd };
        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        // Top-level lowered structure: m2 (MemberAccess) -> RHS Assert, LHS -> m1
        result.Annotations.Should().ContainKey("LoweredGraphExpr");
        result.RHS.Should().BeOfType<MemberAccessExp>();
        var m2 = (MemberAccessExp)result.RHS!;
        m2.RHS.Should().BeOfType<FuncCallExp>();
        var assert2 = (FuncCallExp)m2.RHS!;
        assert2.Annotations["FunctionName"].Should().Be("Assert");

        m2.LHS.Should().BeOfType<MemberAccessExp>();
        var m1 = (MemberAccessExp)m2.LHS!;
        m1.RHS.Should().BeOfType<FuncCallExp>();
        var assert1 = (FuncCallExp)m1.RHS!;
        assert1.Annotations["FunctionName"].Should().Be("Assert");

        // Inspect inner create-triple call for first triple
        assert1.InvocationArguments.Should().NotBeNull();
        assert1.InvocationArguments.Count.Should().Be(1);
        var tripleExpr1 = assert1.InvocationArguments[0];
        tripleExpr1.Should().BeOfType<MemberAccessExp>();
        var createTriple1 = (MemberAccessExp)tripleExpr1;
        createTriple1.RHS.Should().BeOfType<FuncCallExp>();
        var createTripleCall1 = (FuncCallExp)createTriple1.RHS!;
        createTripleCall1.Annotations["FunctionName"].Should().Be("CreateTriple");
        createTripleCall1.InvocationArguments.Count.Should().Be(3);
        // Subject argument should be MemberAccessExp with CreateUri
        createTripleCall1.InvocationArguments[0].Should().BeOfType<MemberAccessExp>();
        var subjNode1 = (MemberAccessExp)createTripleCall1.InvocationArguments[0];
        subjNode1.RHS.Should().BeOfType<FuncCallExp>();
        ((FuncCallExp)subjNode1.RHS!).Annotations["FunctionName"].Should().Be("CreateUri");
        // Object argument should be MemberAccessExp with CreateLiteral
        createTripleCall1.InvocationArguments[2].Should().BeOfType<MemberAccessExp>();
        var objNode1 = (MemberAccessExp)createTripleCall1.InvocationArguments[2];
        ((FuncCallExp)objNode1.RHS!).Annotations["FunctionName"].Should().Be("CreateLiteral");
    }

    [Test]
    public void Detailed_graph_graph_merge_chain_structure()
    {
        var left = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() }, Location = new SourceLocationMetadata(10, "", 10, "") };
        var right = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() }, Location = new SourceLocationMetadata(11, "", 11, "") };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, "", 0, ""), LHS = left, RHS = right, Operator = Operator.ArithmeticAdd };
        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        result.Annotations.Should().ContainKey("LoweredGraphExpr");
        result.RHS.Should().BeOfType<MemberAccessExp>();
        var final = (MemberAccessExp)result.RHS!;
        final.RHS.Should().BeOfType<FuncCallExp>();
        var mergeFinal = (FuncCallExp)final.RHS!;
        mergeFinal.Annotations["FunctionName"].Should().Be("Merge");
        // The final merge should have the right graph as its argument
        mergeFinal.InvocationArguments.Count.Should().Be(1);
        // The merge argument should be a graph expression with the same source location as the original right
        mergeFinal.InvocationArguments[0].Should().BeOfType<GraphAssertionBlockExp>();
        var rightArg = (GraphAssertionBlockExp)mergeFinal.InvocationArguments[0];
        rightArg.Location.Should().Be(right.Location);

        // The LHS of final should itself be a MemberAccessExp representing the previous merge
        final.LHS.Should().BeOfType<MemberAccessExp>();
        var prev = (MemberAccessExp)final.LHS!;
        prev.RHS.Should().BeOfType<FuncCallExp>();
        var mergePrev = (FuncCallExp)prev.RHS!;
        mergePrev.Annotations["FunctionName"].Should().Be("Merge");
        mergePrev.InvocationArguments.Count.Should().Be(1);
        mergePrev.InvocationArguments[0].Should().BeOfType<GraphAssertionBlockExp>();
        var leftArg = (GraphAssertionBlockExp)mergePrev.InvocationArguments[0];
        leftArg.Location.Should().Be(left.Location);
    }

    [Test]
    public void Detailed_graph_triple_chain_structure()
    {
        var left = new GraphAssertionBlockExp { Content = new BlockStatement { Statements = new System.Collections.Generic.List<Statement>() }, Location = new SourceLocationMetadata(20, "", 20, "") };
        var right = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/sx"), Location = new SourceLocationMetadata(21, "", 21, "") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/px"), Location = new SourceLocationMetadata(21, "", 21, "") },
            ObjectExp = new Int32LiteralExp { Value = 7, Location = new SourceLocationMetadata(21, "", 21, "") },
            Location = new SourceLocationMetadata(21, "", 21, "")
        };

        var bin = new BinaryExp { Annotations = new System.Collections.Generic.Dictionary<string, object>(), Location = new SourceLocationMetadata(0, "", 0, ""), LHS = left, RHS = right, Operator = Operator.ArithmeticAdd };
        var visitor = new GraphTripleOperatorLoweringVisitor();
        var result = visitor.VisitBinaryExp(bin);

        result.Annotations.Should().ContainKey("LoweredGraphExpr");
        result.RHS.Should().BeOfType<MemberAccessExp>();
        var top = (MemberAccessExp)result.RHS!;
        // Top should be an Assert call (for the triple)
        top.RHS.Should().BeOfType<FuncCallExp>();
        ((FuncCallExp)top.RHS!).Annotations["FunctionName"].Should().Be("Assert");

        // The LHS of the Assert should be a MemberAccessExp which contains the previous Merge call for the left graph
        top.LHS.Should().BeOfType<MemberAccessExp>();
        var lhsBuilder = (MemberAccessExp)top.LHS!;
        lhsBuilder.RHS.Should().BeOfType<FuncCallExp>();
        ((FuncCallExp)lhsBuilder.RHS!).Annotations["FunctionName"].Should().Be("Merge");
        ((FuncCallExp)lhsBuilder.RHS!).InvocationArguments.Count.Should().Be(1);
        ((FuncCallExp)lhsBuilder.RHS!).InvocationArguments[0].Should().BeOfType<GraphAssertionBlockExp>();
        var lhsArg = (GraphAssertionBlockExp)((FuncCallExp)lhsBuilder.RHS!).InvocationArguments[0];
        lhsArg.Location.Should().Be(left.Location);
    }
}
