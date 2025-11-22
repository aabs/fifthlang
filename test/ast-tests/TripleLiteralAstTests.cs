// T009: AST tests for triple literal
using System.Linq;
using FluentAssertions;
using Xunit; // Test framework
using ast;    // core AST model
using test_infra;
using compiler; // FifthParserManager
using compiler.LangProcessingPhases;

namespace ast_tests;

public class TripleLiteralAstTests
{
    [Fact]
    public void valid_triple_literal_ast_shape()
    {
        var src = @"alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, ex:o>; return 0; }";
        var result = ParseHarness.ParseString(src, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        result.Root.Should().NotBeNull();
        var triples = test_infra.ParseHarness.FindTriples(result.Root!).ToList();
        triples.Should().HaveCount(1, "one triple literal expected");
        var t = triples[0];
        t.SubjectExp.Should().BeOfType<UriLiteralExp>();
        t.PredicateExp.Should().BeOfType<UriLiteralExp>();
        t.ObjectExp.Should().NotBeNull();
        // For the simple literal case we still expect an IRI object
        t.ObjectExp.Should().BeOfType<UriLiteralExp>();
        // Subject/predicate/object already typed as UriLiteralExp; deeper content validation deferred.
    }

    [Fact]
    public void malformed_triple_too_few_components_captured()
    {
        var src = @"alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p>; return 0; }";
        var result = ParseHarness.ParseString(src, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        // We expect no well-formed Triple nodes, but a MalformedTripleExp expression present in the tree.
        test_infra.ParseHarness.FindTriples(result.Root).Should().BeEmpty();
        // Walk raw expressions to locate MalformedTripleExp
        var malformed = FindExpressions<MalformedTripleExp>(result.Root!).ToList();
        malformed.Should().HaveCount(1, "one malformed triple");
        var m = malformed[0];
        m.Components.Should().HaveCount(2, "captures the two provided components");
        m.MalformedKind.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void malformed_triple_trailing_comma_captured()
    {
        var src = @"alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, ex:o,>; return 0; }";
        var result = ParseHarness.ParseString(src, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        test_infra.ParseHarness.FindTriples(result.Root).Should().BeEmpty();
        var malformed = FindExpressions<MalformedTripleExp>(result.Root!).ToList();
        malformed.Should().HaveCount(1);
        malformed[0].Components.Should().HaveCount(3); // subject,predicate,object captured prior to dangling comma
    }

    [Fact]
    public void malformed_triple_too_many_components_captured()
    {
        var src = @"alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, ex:o, ex:x>; return 0; }";
        var result = ParseHarness.ParseString(src, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        test_infra.ParseHarness.FindTriples(result.Root).Should().BeEmpty();
        var malformed = FindExpressions<MalformedTripleExp>(result.Root!).ToList();
        malformed.Should().HaveCount(1);
        malformed[0].Components.Should().HaveCount(4);
    }

    // Variable object position test deferred until grammar supports bare identifiers in tripleObjectTerm

    private static IEnumerable<T> FindExpressions<T>(AssemblyDef root) where T : Expression
    {
        foreach (var m in root.Modules)
        {
            foreach (var f in m.Functions.OfType<FunctionDef>())
            {
                foreach (var e in WalkBlock(f.Body))
                {
                    if (e is T match) yield return match;
                }
            }
        }
    }

    private static IEnumerable<Expression> WalkBlock(BlockStatement block)
    {
        foreach (var stmt in block.Statements)
        {
            foreach (var e in WalkStatement(stmt)) yield return e;
        }
    }

    private static IEnumerable<Expression> WalkStatement(Statement stmt)
    {
        switch (stmt)
        {
            case VarDeclStatement v when v.InitialValue != null:
                foreach (var e in WalkExpression(v.InitialValue)) yield return e; break;
            case ExpStatement es:
                foreach (var e in WalkExpression(es.RHS)) yield return e; break;
            case BlockStatement inner:
                foreach (var e in WalkBlock(inner)) yield return e; break;
        }
        yield break;
    }

    private static IEnumerable<Expression> WalkExpression(Expression expr)
    {
        yield return expr;
        switch (expr)
        {
            case BinaryExp be:
                foreach (var e in WalkExpression(be.LHS)) yield return e;
                foreach (var e in WalkExpression(be.RHS)) yield return e;
                break;
            case FuncCallExp fc:
                foreach (var a in fc.InvocationArguments)
                    foreach (var e in WalkExpression(a)) yield return e;
                break;
            case ListLiteral ll:
                foreach (var a in ll.ElementExpressions)
                    foreach (var e in WalkExpression(a)) yield return e;
                break;
            case MemberAccessExp ma:
                if (ma.LHS is Expression lhs)
                    foreach (var e in WalkExpression(lhs)) yield return e;
                if (ma.RHS is Expression rhs)
                    foreach (var e in WalkExpression(rhs)) yield return e;
                break;
        }
    }
}
