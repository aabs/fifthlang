// T010: List expansion AST tests (expected to fail before expansion visitor implemented)
using System.Linq;
using FluentAssertions;
using Xunit;
using test_infra;
using ast;

namespace ast_tests;

public class TripleLiteralExpansionTests
{
    private ParseResult ParseHarnessed(string code) => ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.TripleExpansion));

    [Fact]
    public void T010_01_List_Object_Expands_To_Multiple_Triples()
    {
        // NOTE: Using string literals inside the list for now because PrefixedName tokens are not yet valid standalone expressions
        // outside of triple component contexts. Once prefixed names are accepted as general expressions, update to IRIs.
        const string code = "alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, [\"o1\", \"o2\"]>; return 0; }";
        var result = ParseHarnessed(code);
        result.Diagnostics.Select(d => d.Code).Should().NotContain("TRPL001", "well-formed list object should not be malformed triple");
        result.Root.Should().NotBeNull();
        var triples = FindTriples((AssemblyDef)result.Root!);
        triples.Count.Should().Be(2);
    }

    [Fact]
    public void T010_02_Empty_List_Object_Produces_TRPL004_And_No_Triples()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, []>; return 0; }";
        var result = ParseHarnessed(code);
        // DEBUG: print diagnostics to help determine whether TRPL004 is emitted
        // Console.WriteLine("Diagnostics codes: " + string.Join(",", result.Diagnostics.Select(d => d.Code)));
        // DEBUG: inspect main function RHS expression types
        if (result.Root is AssemblyDef asm)
        {
            var main = asm.Modules.SelectMany(m => m.Functions).OfType<FunctionDef>().FirstOrDefault(f => f.Name.Value == "main");
            if (main?.Body != null)
            {
                foreach (var stmt in main.Body.Statements)
                {
                    if (stmt is ExpStatement es)
                    {
                        // Console.WriteLine($"Main RHS expression type: {es.RHS?.GetType().Name}");
                        if (es.RHS is TripleLiteralExp tle)
                        {
                            // Console.WriteLine($" - Triple object type: {tle.ObjectExp?.GetType().Name}");
                            if (tle.ObjectExp is ListLiteral ll)
                            {
                                // Console.WriteLine($" - List element count: {ll.ElementExpressions?.Count}");
                            }
                        }
                        if (es.RHS is MalformedTripleExp mte)
                        {
                            // Console.WriteLine($" - Malformed triple kind: {mte.MalformedKind}; components: {mte.Components?.Count}");
                        }
                    }
                }
            }
        }
        result.Diagnostics.Select(d => d.Code).Should().Contain("TRPL004", "empty list triple object should emit TRPL004 warning");
        var triples = FindTriples((AssemblyDef)result.Root!);
        triples.Count.Should().Be(0, "empty list object expands to zero triples");
    }

    private static IList<TripleLiteralExp> FindTriples(AssemblyDef root)
    {
        return root.Modules
            .SelectMany(m => m.Functions.OfType<FunctionDef>())
            .SelectMany(DescendFunction)
            .OfType<TripleLiteralExp>()
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
        if (expr is TripleLiteralExp t)
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
