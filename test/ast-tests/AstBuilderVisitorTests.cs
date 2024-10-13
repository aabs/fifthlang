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
        var funcdefsrc = $$"""
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
    [Fact]
    public void can_build_from_function_def_with_if_statement()
    {
        var funcdefsrc = $$"""
                         foo(b: Bar, b2: Baz): Sqz
                         {
                            if(b == 1)
                            {
                                return "hello";
                            }
                            else
                            {
                                return "world";
                            };
                         }
                         """;
        var s = CharStreams.fromString(funcdefsrc);
        var p = GetParserFor(s);
        var x = p.function_declaration();
        x.Should().NotBeNull();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as FunctionDef;
        a.Should().NotBeNull();
        var stmt = a.Body.Statements[0];
        stmt.Should().NotBeNull();
        if (stmt is IfElseStatement ies)
        {
            ies.Condition.Should().BeOfType<BinaryExp>();
        }
        else
        {
            Assert.Fail();
        }
    }

    [Theory]
    [InlineData("1 + 1", Operator.ArithmeticAdd)]
    [InlineData("1 - 1", Operator.ArithmeticSubtract)]
    [InlineData("1 * 1", Operator.ArithmeticMultiply)]
    [InlineData("1 / 1", Operator.ArithmeticDivide)]
    [InlineData("1 ** 1", Operator.ArithmeticPow)]
    [InlineData("1 % 1", Operator.ArithmeticMod)]

    [InlineData("1 == 1", Operator.Equal)]
    [InlineData("1 != 1", Operator.NotEqual)]
    [InlineData("1 > 1", Operator.GreaterThan)]
    [InlineData("1 < 1", Operator.LessThan)]
    [InlineData("1 <= 1", Operator.LessThanOrEqual)]
    [InlineData("1 >= 1", Operator.GreaterThanOrEqual)]

    [InlineData("1 & 1", Operator.BitwiseAnd)]
    [InlineData("1 | 1", Operator.BitwiseOr)]
    [InlineData("1 << 1", Operator.BitwiseLeftShift)]
    [InlineData("1 >> 1", Operator.BitwiseRightShift)]
    [InlineData("1 && 1", Operator.LogicalAnd)]
    [InlineData("1 || 1", Operator.LogicalOr)]
    [InlineData("1 ^ 1", Operator.LogicalXor)]
    public void should_handle_all_kinds_of_binary_expressions(string exp, Operator op)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.exp();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as BinaryExp;
        a.Should().NotBeNull();
        a.Operator.Should().Be(op);
    }
}
