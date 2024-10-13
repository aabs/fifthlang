using Antlr4.Runtime;
using ast;
using compiler.LangProcessingPhases;
using Fifth;
using FluentAssertions;
using FsCheck.Xunit;

namespace ast_tests;
public class AstBuilderVisitorTests
{
    private static FifthParser GetParserFor(ICharStream source)
    {
        var lexer = new FifthLexer(source);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ThrowingErrorListener<int>());

        var parser = new FifthParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ThrowingErrorListener<IToken>());
        return parser;
    }

    [Property]
    public void can_parse_double_literals(double d)
    {
        var nativeRepresentation = $"{d}";
        if (nativeRepresentation is "infinity" or "-infinity" or "NaN" or "\u221e" or "-\u221e")
        {
            return;
        }

        var s = CharStreams.fromString(nativeRepresentation);
        var p = GetParserFor(s);
        var actual = p.exp();
        if (actual is not FifthParser.Exp_doubleContext)
        {
            Assert.Fail();
        }

        var v = new AstBuilderVisitor();
        var a = v.VisitExp_double((FifthParser.Exp_doubleContext)actual);
        a.Should().NotBeNull();
        a.Should().BeOfType<Float8LiteralExp>();
    }

    [Fact]
    public void can_parse_double_literals_case1()
    {
        var s = CharStreams.fromString("4.940656458e-324");
        var p = GetParserFor(s);
        var actual = p.exp();
        if (actual is FifthParser.Exp_doubleContext)
        {
            var v = new AstBuilderVisitor();
            var a = v.VisitExp_double((FifthParser.Exp_doubleContext)actual);
            a.Should().NotBeNull();
            a.Should().BeOfType<Float8LiteralExp>();
        }
    }

    [Property]
    public void can_parse_int_literals(int d)
    {
        var nativeRepresentation = $"{d}";
        if (nativeRepresentation is "infinity" or "-infinity" or "NaN" or "\u221e" or "-\u221e")
        {
            return;
        }

        var s = CharStreams.fromString(nativeRepresentation);
        var p = GetParserFor(s);
        var actual = p.exp();
        var v = new AstBuilderVisitor();
        var a = v.VisitExp_int((FifthParser.Exp_intContext)actual);
        a.Should().NotBeNull();
        a.Should().BeOfType<Int32LiteralExp>();
    }

    [Fact]
    public void can_build_from_function_def()
    {
        var funcdefsrc=$$"""
                         foo(b: Bar, b2: Baz): Sqz{}
                         """;
        var s = CharStreams.fromString(funcdefsrc);
        var p = GetParserFor(s);
        var x = p.function_declaration();
        x.Should().NotBeNull();
        if (x is FifthParser.Function_declarationContext ctx)
        {
            var v = new AstBuilderVisitor();
            var a = v.Visit(ctx) as FunctionDef;
            a.Should().NotBeNull();
        }
    }
}
