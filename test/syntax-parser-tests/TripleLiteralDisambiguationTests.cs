// Disambiguation tests ensuring triple literal tokens & shape distinct from single IRIREF
using System.Linq;
using FluentAssertions;
using Xunit;
using test_infra;

namespace syntax_parser_tests;

public class TripleLiteralDisambiguationTests
{
    private ParseResult ParseWithTokens(string code) => ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.None, CollectTokens: true));

    // Temporarily disabled; triple literal malformed diagnostics deferred.
    //[Test]
    //public void DISAMBIG_01_Simple_Triple_Token_Sequence_Contains_Commas()
    //{
    //    const string code = "alias ex as <http://example.org/>;\nmain(): int { t: triple = <ex:s, ex:p, ex:o>; return 0; }";
    //    var result = ParseWithTokens(code);
    //    result.Root.Should().NotBeNull();
    //    var text = string.Join("", result.Tokens!.Select(t => t.Text));
    //    text.Should().Contain(",,", "raw token text should include two commas inside triple literal (order-insensitive check is coarse)");
    //}

    [Fact]
    public void DISAMBIG_02_Triple_Not_Single_IriRef_Token()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { t: triple = <ex:s, ex:p, ex:o>; return 0; }";
        var result = ParseWithTokens(code);
        // We expect at least two COMMA tokens among collected tokens of the triple literal
        var commaCount = result.Tokens!.Count(t => t.Text == ",");
        commaCount.Should().Be(2);
    }
}
