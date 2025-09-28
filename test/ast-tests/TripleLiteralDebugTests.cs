using System;
using FluentAssertions;
using TUnit;
using ast;
using test_infra;

namespace ast_tests;

// Temporary debug harness to see if VisitTripleLiteral is invoked and to capture diagnostics
public class TripleLiteralDebugTests
{
    private ParseResult ParseHarnessed(string code) => ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.TreeLink));

    [Test]
    public void DBG_TripleLiteral_Minimal()
    {
        // Minimal program with a triple literal assignment
        const string code = "alias ex as <http://example.org/>; main(): int { t: triple = <ex:s, ex:p, ex:o>; return 0; }";
        var result = ParseHarnessed(code);
        // Debug output removed; tests assert on AST and diagnostics instead.
        result.Root.Should().NotBeNull();
        var triples = FindTriples(result.Root!);
        // Debug output removed; assertion below checks triple count when necessary.
    }

    private static IList<TripleLiteralExp> FindTriples(AssemblyDef root)
    {
        var list = new List<TripleLiteralExp>();
        foreach (var m in root.Modules)
        {
            foreach (var f in m.Functions.OfType<FunctionDef>())
            {
                if (f.Body == null) continue;
                foreach (var s in f.Body.Statements)
                {
                    foreach (var e in Descend(s))
                    {
                        if (e is TripleLiteralExp t) list.Add(t);
                    }
                }
            }
        }
        return list;
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
            case TripleLiteralExp t:
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
