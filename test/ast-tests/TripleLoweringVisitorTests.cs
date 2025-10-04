using ast;
using compiler.LanguageTransformations;
using FluentAssertions;
using System.Collections.Generic;

namespace ast_tests;

public class TripleLoweringVisitorTests : VisitorTestsBase
{
    [Test]
    public void Graph_plus_triple_produces_merge_then_assert()
    {
        var leftGraph = new GraphAssertionBlockExp
        {
            Annotations = new Dictionary<string, object>(),
            Content = new BlockStatement { Statements = new List<Statement>() },
            Location = new SourceLocationMetadata(1, string.Empty, 1, string.Empty)
        };
        var rightTriple = MakeTriple("http://example.org/right", "http://example.org/name", "right-value");

        var expression = new BinaryExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty),
            LHS = leftGraph,
            RHS = rightTriple,
            Operator = Operator.ArithmeticAdd
        };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var lowered = visitor.VisitBinaryExp(expression);

        lowered.Annotations.Should().ContainKey("LoweredGraphExpr");
        lowered.Annotations.Should().ContainKey("TripleSignatures");
        lowered.Annotations["TripleSignatures"].Should().BeOfType<List<string>>().Which.Should().ContainSingle();

        lowered.RHS.Should().BeOfType<MemberAccessExp>();
        var finalChain = (MemberAccessExp)lowered.RHS!;
        finalChain.RHS.Should().BeOfType<FuncCallExp>();
        var assertCall = (FuncCallExp)finalChain.RHS!;
        assertCall.Annotations["FunctionName"].Should().Be("Assert");

        finalChain.LHS.Should().BeOfType<MemberAccessExp>();
        var mergeChain = (MemberAccessExp)finalChain.LHS!;
        mergeChain.RHS.Should().BeOfType<FuncCallExp>();
        var mergeCall = (FuncCallExp)mergeChain.RHS!;
        mergeCall.Annotations["FunctionName"].Should().Be("Merge");
    }

    [Test]
    public void Triple_plus_triple_deduplicates_identical_literals()
    {
        var left = MakeTriple("http://example.org/dup", "http://example.org/name", "duplicate");
        var right = MakeTriple("http://example.org/dup", "http://example.org/name", "duplicate");

        var expression = new BinaryExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty),
            LHS = left,
            RHS = right,
            Operator = Operator.ArithmeticAdd
        };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var lowered = visitor.VisitBinaryExp(expression);

        lowered.Annotations.Should().ContainKey("LoweredGraphExpr");
        lowered.RHS.Should().BeOfType<MemberAccessExp>();
        var chain = (MemberAccessExp)lowered.RHS!;
        lowered.Annotations.Should().ContainKey("TripleSignatures");
        lowered.Annotations["TripleSignatures"].Should().BeOfType<List<string>>().Which.Should().ContainSingle("http://example.org/dup|http://example.org/name|str:duplicate");

        lowered.RHS.Should().BeOfType<MemberAccessExp>();
        var finalChain = (MemberAccessExp)lowered.RHS!;
        FinalAssert(finalChain).Annotations["FunctionName"].Should().Be("Assert");
    }

    [Test]
    public void Distinct_triple_literals_are_all_asserted()
    {
        var left = MakeTriple("http://example.org/a", "http://example.org/p", "foo");
        var right = MakeTriple("http://example.org/b", "http://example.org/p", "bar");

        var expression = new BinaryExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = new SourceLocationMetadata(0, string.Empty, 0, string.Empty),
            LHS = left,
            RHS = right,
            Operator = Operator.ArithmeticAdd
        };

        var visitor = new GraphTripleOperatorLoweringVisitor();
        var lowered = visitor.VisitBinaryExp(expression);

        lowered.Annotations.Should().ContainKey("LoweredGraphExpr");
        lowered.RHS.Should().BeOfType<MemberAccessExp>();
        var chain = (MemberAccessExp)lowered.RHS!;
        lowered.Annotations.Should().ContainKey("TripleSignatures");
        lowered.Annotations["TripleSignatures"].Should().BeOfType<List<string>>()
            .Which.Should().BeEquivalentTo(new[]
            {
                "http://example.org/a|http://example.org/p|str:foo",
                "http://example.org/b|http://example.org/p|str:bar"
            });

        lowered.RHS.Should().BeOfType<MemberAccessExp>();
        var finalChain = (MemberAccessExp)lowered.RHS!;
        var assertCall = FinalAssert(finalChain);
        assertCall.Annotations["FunctionName"].Should().Be("Assert");
        var previous = (MemberAccessExp)finalChain.LHS!;
        FinalAssert(previous).Annotations["FunctionName"].Should().Be("Assert");
    }

    private static TripleLiteralExp MakeTriple(string subject, string predicate, string obj)
    {
        return new TripleLiteralExp
        {
            Annotations = new Dictionary<string, object>(),
            Location = new SourceLocationMetadata(1, string.Empty, 1, string.Empty),
            SubjectExp = new UriLiteralExp
            {
                Annotations = new Dictionary<string, object>(),
                Location = new SourceLocationMetadata(1, string.Empty, 1, string.Empty),
                Value = new System.Uri(subject)
            },
            PredicateExp = new UriLiteralExp
            {
                Annotations = new Dictionary<string, object>(),
                Location = new SourceLocationMetadata(1, string.Empty, 1, string.Empty),
                Value = new System.Uri(predicate)
            },
            ObjectExp = new StringLiteralExp
            {
                Annotations = new Dictionary<string, object>(),
                Location = new SourceLocationMetadata(1, string.Empty, 1, string.Empty),
                Value = obj
            }
        };
    }

    private static FuncCallExp FinalAssert(MemberAccessExp member)
    {
        member.RHS.Should().BeOfType<FuncCallExp>();
        return (FuncCallExp)member.RHS!;
    }
}
