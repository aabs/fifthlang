using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

/// <summary>
/// Parser tests for Graph Assertion Block used as a primary expression.
/// </summary>
public class GraphAssertionBlock_ExpressionTests
{
    [Test]
    public void GraphBlock_AsExpression_Assignment_ShouldParse()
    {
        var input = "{ g = <{ a = 1; b = 2; }>; }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "Graph assertion block should parse when used as an expression in assignment");
    }

    [Test]
    public void GraphBlock_AsFunctionArgument_ShouldParse()
    {
        var input = "{ foo(<{ a = 1; }>, 42); }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "Graph assertion block should parse when used as a function call argument");
    }

    [Test]
    public void GraphBlock_AsParenthesizedExpression_ShouldParse()
    {
        var input = "{ g = (<{ a = 1; }>) ; }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "Graph assertion block should parse within parentheses as primary expression");
    }
}
