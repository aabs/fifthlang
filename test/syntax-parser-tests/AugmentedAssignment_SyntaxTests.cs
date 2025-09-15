using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

public class AugmentedAssignment_SyntaxTests
{
    [Test]
    public void PlusAssign_AsStatement_ShouldParse()
    {
        var input = "{ x += y; }";
        ParserTestUtils.AssertNoErrors(input, p => p.block(),
            "+= augmented assignment should parse within a block");
    }

    [Test]
    public void PlusAssign_MissingSemicolon_ShouldFail()
    {
        var input = "{ x += y }";
        ParserTestUtils.AssertHasErrors(input, p => p.block(),
            "+= requires a terminating semicolon");
    }
}
