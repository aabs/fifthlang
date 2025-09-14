using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

/// <summary>
/// Parser tests to ensure graph assertion blocks support nesting and composition.
/// </summary>
public class GraphAssertionBlock_NestingTests
{
    [Test]
    public void GraphBlock_NestedInsideGraphBlock_ShouldParse()
    {
        var input = "<{ a = 1; <{ b = 2; }>; }>;";
        ParserTestUtils.AssertNoErrors(input, p => p.statement(),
            "Nested graph assertion blocks should parse");
    }

    [Test]
    public void GraphBlock_NestedInsideRegularBlock_ShouldParse()
    {
        var input = "{ x = 0; <{ a = 1; }>; y = 3; }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "Graph assertion blocks should parse inside regular blocks");
    }
}
