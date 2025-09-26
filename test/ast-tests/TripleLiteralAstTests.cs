// T009: AST tests for triple literal (expected to fail before implementation)
using System.Linq;
using FluentAssertions;
using TUnit; // Test framework
using ast;    // core AST model
using test_infra;

namespace ast_tests;

public class TripleLiteralAstTests
{
    private ParseResult ParseHarnessed(string code) => ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.TreeLink));

    [Test]
    public void T009_01_SimpleTripleLiteral_Produces_TripleLiteralExp_Node()
    {
        const string code = @"alias ex as <http://example.org/>;\nmain(): int { t: triple = <ex:s, ex:p, ex:o>; return 0; }";
        var result = ParseHarnessed(code);
        result.Diagnostics.Should().BeEmpty();
        result.Root.Should().NotBeNull();
        var triples = FindTriples(result.Root!);
        triples.Should().HaveCount(1);
    }

    [Test]
    public void T009_02_Variable_Subject_Predicate_Accepted_When_Iri_Typed()
    {
        const string code = @"alias ex as <http://example.org/>;\nmain(): int { s: iri = ex:s; p: iri = ex:p; t: triple = <s, p, ex:o>; return 0; }";
        var result = ParseHarnessed(code);
        result.Diagnostics.Should().BeEmpty();
        var triples = FindTriples(result.Root!);
        triples.Should().HaveCount(1);
    }

    private static IList<Triple> FindTriples(AssemblyDef root)
    {
        return root.Modules
            .SelectMany(m => m.Functions.OfType<FunctionDef>())
            .SelectMany(f => Descend(f.Body))
            .OfType<Triple>()
            .ToList();
    }

    private static IEnumerable<ast.Expression> Descend(object? node)
    {
        if (node is null) yield break;
        switch (node)
        {
            case BlockStatement b:
                foreach (var s in b.Statements) foreach (var e in Descend(s)) yield return e; break;
            case ExpStatement es:
                foreach (var e in Descend(es.RHS)) yield return e; break;
            case VarDeclStatement vds:
                if (vds.InitialValue != null) foreach (var e in Descend(vds.InitialValue)) yield return e; break;
            case Triple t:
                yield return t; break;
            case BinaryExp be:
                foreach (var e in Descend(be.LHS)) yield return e; foreach (var e in Descend(be.RHS)) yield return e; break;
            case MemberAccessExp ma:
                foreach (var e in Descend(ma.LHS)) yield return e; foreach (var e in Descend(ma.RHS)) yield return e; break;
            case ListLiteral ll:
                foreach (var el in ll.ElementExpressions) foreach (var e in Descend(el)) yield return e; break;
            case FuncCallExp fc:
                foreach (var arg in fc.InvocationArguments) foreach (var e in Descend(arg)) yield return e; break;
        }
    }
}
