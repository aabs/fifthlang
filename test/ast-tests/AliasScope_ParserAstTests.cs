using FluentAssertions;
using Antlr4.Runtime;
using ast;
using Fifth;
using compiler.LangProcessingPhases;

namespace ast_tests;

public class AliasScope_ParserAstTests
{
    [Fact]
    public void ClassWithAliasScope_ShouldParseAndRecordAliasScope()
    {
        var src = "class Person in x { Name: string; }";
        var s = CharStreams.fromString(src);
        var parser = new FifthParser(new CommonTokenStream(new FifthLexer(s)));
        var ctx = parser.class_definition();
        var v = new AstBuilderVisitor();
        var cls = v.VisitClass_definition(ctx) as ClassDef;
        cls.Should().NotBeNull();
        cls!.AliasScope.Should().Be("x");
    }
}
