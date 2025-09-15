using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

/// <summary>
/// Parser tests for Graph Assertion Block used as a standalone statement.
/// Expected syntax: <{ ... }>; inside any statement context.
/// </summary>
public class GraphAssertionBlock_StatementTests
{
    [Test]
    public void GraphBlock_AsStandaloneStatement_ShouldParse()
    {
        var input = "{ <{ a = 1; }>; }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "Graph assertion block should parse as a standalone statement");
    }

    [Test]
    public void GraphBlock_AsStatementInsideBlock_ShouldParse()
    {
        var input = "{ <{ a = 1; }>; }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "Graph assertion block should parse as a statement within a block");
    }

    [Test]
    public void GraphBlock_Statement_MissingSemicolon_ShouldFail()
    {
        var input = "{ <{ a = 1; }> }";
        ParserTestUtils.AssertHasErrors(input, p => p.block(),
            "Graph assertion block as a statement requires a terminating semicolon");
    }
}
