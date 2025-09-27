// T010: List expansion AST tests (expected to fail before expansion visitor implemented)
using System.Linq;
using FluentAssertions;
using TUnit;
using test_infra;
using ast;

namespace ast_tests;

public class TripleLiteralExpansionTests
{
    private ParseResult ParseHarnessed(string code) => ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.TripleExpansion));

    [Test]
    public void T010_01_List_Object_Expands_Into_Multiple_TripleLiterals()
    {
        const string code = "alias ex as <http://example.org/>; main(): int { g: graph = <ex:s, ex:p, [ex:o1, ex:o2, ex:o3]>; return 0; }";
        var result = ParseHarnessed(code);
        result.Diagnostics.Should().BeEmpty();
        result.Root.Should().NotBeNull();
        // After expansion we expect 3 distinct Triple nodes (object list of length 3)
        FindTriples(result.Root!).Should().HaveCount(3);
    }

    [Test]
    public void T010_02_Nested_List_Rejected_With_TRPL006()
    {
        const string code = "alias ex as <http://example.org/>; main(): int { g: graph = <ex:s, ex:p, [[ex:o1, ex:o2], ex:o3]>; return 0; }";
        var result = ParseHarnessed(code);
        // Should remain a single unexpanded triple due to nested list error (TRPL006 produced earlier phase)
        result.Root.Should().NotBeNull();
        FindTriples(result.Root!).Should().HaveCount(1);
    }

    private static IList<Triple> FindTriples(AssemblyDef root)
    {
        return root.Modules
            .SelectMany(m => m.Functions.OfType<FunctionDef>())
            .SelectMany(DescendFunction)
            .OfType<Triple>()
            .ToList();
    }

    private static IEnumerable<ast.Expression> DescendFunction(FunctionDef f)
    {
        if (f.Body is BlockStatement b)
        {
            foreach (var e in DescendBlock(b)) yield return e;
        }
    }

    private static IEnumerable<ast.Expression> DescendBlock(BlockStatement block)
    {
        foreach (var stmt in block.Statements)
        {
            foreach (var e in DescendStatement(stmt)) yield return e;
        }
    }

    private static IEnumerable<ast.Expression> DescendStatement(Statement stmt)
    {
        switch (stmt)
        {
            case ExpStatement es:
                foreach (var e in DescendExpression(es.RHS)) yield return e;
                break;
            case VarDeclStatement vds when vds.InitialValue != null:
                foreach (var e in DescendExpression(vds.InitialValue)) yield return e;
                break;
            case BlockStatement inner:
                foreach (var e in DescendBlock(inner)) yield return e;
                break;
        }
    }

    private static IEnumerable<ast.Expression> DescendExpression(Expression expr)
    {
        if (expr is Triple t)
        {
            yield return t;
            yield break;
        }
        switch (expr)
        {
            case BinaryExp be:
                foreach (var e in DescendExpression(be.LHS)) yield return e;
                foreach (var e in DescendExpression(be.RHS)) yield return e;
                break;
            case MemberAccessExp ma:
                foreach (var e in DescendExpression(ma.LHS)) yield return e;
                if (ma.RHS is Expression rhs) foreach (var e in DescendExpression(rhs)) yield return e;
                break;
            case ListLiteral ll:
                foreach (var el in ll.ElementExpressions)
                    foreach (var e in DescendExpression(el)) yield return e;
                break;
            case FuncCallExp fc:
                foreach (var arg in fc.InvocationArguments)
                    foreach (var e in DescendExpression(arg)) yield return e;
                break;
        }
    }
}
