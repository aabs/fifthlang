using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

/// <summary>
/// Parser tests to verify that the invalid triple syntax in Graph Assertion Blocks
/// is properly rejected by the parser, which is the correct behavior.
/// This confirms the parser is working as intended and only accepts assignment statements.
/// </summary>
public class GraphAssertionBlock_TripleSyntaxTests
{
    [Test]
    public void GraphBlock_WithTripleSyntax_ShouldBeRejected()
    {
        // This triple syntax should be rejected by the parser
        var input = "<{ (:s, :p, \"hello\"); }>;";
        ParserTestUtils.AssertHasErrors(input, p => p.statement(),
            "Triple syntax like (:s, :p, \"hello\") should be rejected in graph assertion blocks");
    }

    [Test]
    public void GraphBlock_WithTripleSyntaxInExpression_ShouldBeRejected()
    {
        // This should also be rejected when used as an expression
        var input = "{ var g: graph = <{ (:s, :p, 42); }>; }";
        ParserTestUtils.AssertHasErrors(input, p => p.block(),
            "Triple syntax should be rejected in graph assertion block expressions");
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
    public void GraphBlock_WithMultipleTriples_ShouldBeRejected()
    {
        // Multiple triples should also be rejected
        var input = """
            <{
                (:s, :p, "hello");
                (:s, :p2, 42);
                (:s, :p3, true);
            }>;
            """;
        ParserTestUtils.AssertHasErrors(input, p => p.statement(),
            "Multiple triple statements should be rejected in graph assertion blocks");
    }
}