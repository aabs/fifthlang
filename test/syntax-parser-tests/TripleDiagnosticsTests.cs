// T011: Diagnostics tests for triple literals wired to ParseHarness (assertions placeholder until TRPL codes implemented)
// Triple diagnostics temporarily disabled: structured TRPL00x detection deferred.
// See docs/issues/triple-diagnostics-refactor.md
using System.Linq;
using FluentAssertions;
using Xunit;
using test_infra;

namespace syntax_parser_tests;

public class TripleDiagnosticsTests
{
    // Initial subset of reactivated tests now that triple grammar + AST mapping exist.
    private ParseResult ParseHarnessed(string code) => ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.TripleDiagnostics));
    private static string[] Codes(ParseResult r) => r.Diagnostics.Select(d => d.Code).ToArray();

    // Former tests (disabled):
    // [Test] public void T011_01_Arity_Too_Few_TRPL001() { ... }
    // [Test] public void T011_02_Arity_Too_Many_TRPL001() { ... }
    // [Test] public void T011_03_Trailing_Comma_TRPL001() { ... }
    // [Test] public void T011_04_Nested_List_TRPL006_Placeholder() { ... }
    // [Test] public void T011_05_Empty_List_TRPL004_Placeholder() { ... }
    // [Test] public void T011_06_Unresolved_Prefix_Placeholder() { ... }

    [Fact]
    public void T011_01_Arity_Too_Few_TRPL001()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p>; return 0; }";
        var result = ParseHarnessed(code);
        Codes(result).Should().Contain("TRPL001");
    }

    [Fact]
    public void T011_02_Arity_Too_Many_TRPL001()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, ex:o, ex:x>; return 0; }";
        var result = ParseHarnessed(code);
        Codes(result).Should().Contain("TRPL001");
    }

    [Fact]
    public void T011_03_Trailing_Comma_TRPL001()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, ex:o,>; return 0; }";
        var result = ParseHarnessed(code);
        Codes(result).Should().Contain("TRPL001");
    }
}
