using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

public class LambdaSyntaxTests
{
    [Fact]
    public void FunctionType_VariableDecl_ZeroArgs_ShouldParse()
    {
        var input = "f : [] -> int;";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.statement(), "0-arg function type should parse");
    }

    [Fact]
    public void FunctionType_VariableDecl_MultiArgs_ShouldParse()
    {
        var input = "f : [int, int] -> int;";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.statement(), "multi-arg function type should parse");
    }

    [Fact]
    public void LambdaExpression_Assignment_ShouldParse()
    {
        var input = "f : [int] -> int = fun(x: int): int { return x; };";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.statement(), "lambda expression should parse");
    }

    [Fact]
    public void FunctionDecl_FunctionTypedParam_ShouldParse()
    {
        var input = "apply(f: [int] -> int, x: int): int { return f(x); }";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.function_declaration(), "function typed parameter should parse");
    }
}
