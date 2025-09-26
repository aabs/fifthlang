using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

/// <summary>
/// Parser tests to demonstrate that the invalid triple syntax in Graph Assertion Blocks
/// is currently being accepted by the parser, which is the issue that needs to be fixed.
/// These tests document the current (incorrect) behavior.
/// </summary>
public class GraphAssertionBlock_TripleSyntaxTests
{
    [Test]
    public void GraphBlock_WithTripleSyntax_CurrentlyParsesIncorrectly()
    {
        // This is the problematic syntax that currently parses but shouldn't
        var input = "<{ (:s, :p, \"hello\"); }>;";
        ParserTestUtils.AssertNoErrors(input, p => p.statement(),
            "Triple syntax like (:s, :p, \"hello\") currently parses but should not");
    }

    [Test]
    public void GraphBlock_WithTripleSyntaxInExpression_CurrentlyParsesIncorrectly()
    {
        // This also currently parses when it shouldn't
        var input = "{ var g: graph = <{ (:s, :p, 42); }>; }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "Triple syntax in expressions currently parses but should not");
    }

    [Test]
    public void GraphBlock_WithValidAssignments_ShouldParse()
    {
        // This is the correct syntax that should work
        var input = "<{ a = 1; b = \"hello\"; }>;";
        ParserTestUtils.AssertNoErrors(input, p => p.statement(),
            "Valid assignment statements should parse correctly in graph assertion blocks");
    }

    [Test]
    public void GraphBlock_WithMultipleTriples_CurrentlyParsesIncorrectly()
    {
        // Multiple triples currently parse when they shouldn't
        var input = """
            <{
                (:s, :p, "hello");
                (:s, :p2, 42);
                (:s, :p3, true);
            }>;
            """;
        ParserTestUtils.AssertNoErrors(input, p => p.statement(),
            "Multiple triple statements currently parse but should not");
    }
}