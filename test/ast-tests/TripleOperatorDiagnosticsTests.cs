using System.Linq;
using FluentAssertions;
using test_infra;

namespace ast_tests;

public class TripleOperatorDiagnosticsTests
{
    private static ParseResult Parse(string code)
    {
        var options = new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.TripleDiagnostics);
        return ParseHarness.ParseString(code, options);
    }

    [Fact]
    public void Triple_minus_graph_reports_TRPL013()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { g: graph = KG.CreateGraph(); t: triple = <ex:s, ex:p, ex:o> - g; return 0; }";
        var result = Parse(code);
        result.Diagnostics.Select(d => d.Code).Should().Contain("TRPL013");
    }

    [Fact]
    public void Triple_times_number_reports_TRPL013()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): graph { return <ex:s, ex:p, ex:o> * 2; }";
        var result = Parse(code);
        result.Diagnostics.Select(d => d.Code).Should().Contain("TRPL013");
    }

    [Fact]
    public void Logical_not_on_triple_reports_TRPL013()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): bool { return !<ex:s, ex:p, ex:o>; }";
        var result = Parse(code);
        result.Diagnostics.Select(d => d.Code).Should().Contain("TRPL013");
    }
}
