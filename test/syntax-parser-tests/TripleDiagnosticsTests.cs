// T011: Diagnostics tests for triple literals wired to ParseHarness (assertions placeholder until TRPL codes implemented)
using System.Linq;
using FluentAssertions;
using TUnit;
using test_infra;

namespace syntax_parser_tests;

public class TripleDiagnosticsTests
{
    private ParseResult ParseHarnessed(string code) => ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.TripleDiagnostics));

    private static string[] Codes(ParseResult r) => r.Diagnostics.Select(d => d.Code).ToArray();

    [Test]
    public void T011_01_Arity_Too_Few_TRPL001_Placeholder()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { a: triple = <ex:s, ex:p>; return 0; }";
        var result = ParseHarnessed(code);
        // Placeholder: semantic arity diagnostics not yet emitted; ensure we at least parsed and captured something (maybe syntax error)
        result.Root.Should().NotBeNull();
    }

    [Test]
    public void T011_02_Arity_Too_Many_TRPL001_Placeholder()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { b: triple = <ex:s, ex:p, ex:o, ex:x>; return 0; }";
        var result = ParseHarnessed(code);
        result.Root.Should().NotBeNull();
    }

    [Test]
    public void T011_03_Trailing_Comma_TRPL001_Placeholder()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { c: triple = <ex:s, ex:p, ex:o,>; return 0; }";
        var result = ParseHarnessed(code);
        result.Root.Should().NotBeNull();
    }

    [Test]
    public void T011_04_Nested_List_TRPL006_Placeholder()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, [[ex:o1, ex:o2], ex:o3]>; return 0; }";
        var result = ParseHarnessed(code);
        result.Root.Should().NotBeNull();
        Codes(result).Should().Contain("TRPL006");
    }

    [Test]
    public void T011_05_Empty_List_TRPL004_Placeholder()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, []>; return 0; }";
        var result = ParseHarnessed(code);
        result.Root.Should().NotBeNull();
        Codes(result).Should().Contain("TRPL004");
    }

    [Test]
    public void T011_06_Unresolved_Prefix_Placeholder()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { t: triple = <ux:s, ex:p, ex:o>; return 0; }";
        var result = ParseHarnessed(code);
        result.Root.Should().NotBeNull();
    }
}
