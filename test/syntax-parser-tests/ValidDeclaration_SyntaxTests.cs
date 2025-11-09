using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

public class ValidDeclaration_SyntaxTests
{
    [Test]
    public void VariableDeclaration_ShouldSucceed()
    {
        var input = "x : int = 1;";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.statement(),
            "Colon-form variable declaration should parse");
    }

    [Test]
    public void StoreDeclaration_ShouldSucceed()
    {
        var input = "home : store = sparql_store(<http://example.com/>);";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.statement(),
            "Colon-form store declaration should parse");
    }

    [Test]
    public void GraphDeclaration_ShouldSucceed()
    {
        var input = "g : graph = KG.CreateGraph();";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.statement(),
            "Colon-form graph declaration should parse");
    }
}
