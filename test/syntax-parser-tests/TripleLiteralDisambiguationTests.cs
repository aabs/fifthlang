// Pre-grammar-change disambiguation tests (will fail or throw until tripleLiteral rule exists).
using System;
using FluentAssertions;
using TUnit;

namespace syntax_parser_tests;

public class TripleLiteralDisambiguationTests
{
    private (object ast, string[] diagnostics) Parse(string code)
    {
        throw new NotImplementedException("Parser not yet updated with tripleLiteral (disambiguation pre-check).");
    }

    [Test]
    public void DISAMBIG_01_IriRef_Alone_Remains_Valid()
    {
        const string code = "// Expect existing IRI usage unaffected once triple literal added\n// Placeholder program if needed later";
        var (_, _) = Parse(code);
    }

    [Test]
    public void DISAMBIG_02_Triple_Pattern_Shape_Distinguishable()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { t: triple = <ex:s, ex:p, ex:o>; return 0; }";
        var (_, _) = Parse(code);
        // After grammar introduction this should parse as triple literal not single IRIREF token.
    }
}
