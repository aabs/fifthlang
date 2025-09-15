using FluentAssertions;
using Antlr4.Runtime;
using Fifth;

namespace syntax_parser_tests;

public class AliasScope_SyntaxTests
{
    [Test]
    public void ClassWithAliasScope_ShouldParse()
    {
        var src = "class Person in x { Name: string; }";
        var s = CharStreams.fromString(src);
        var lexer = new FifthLexer(s);
        var parser = new FifthParser(new CommonTokenStream(lexer));
        var ctx = parser.class_definition();
        ctx.Should().NotBeNull();
        ctx.aliasScope.Should().NotBeNull();
        ctx.aliasScope.GetText().Should().Be("x");
    }
}
