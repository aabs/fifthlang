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
        var nativeRepresentation = $"{d:0.000}d";
        if (nativeRepresentation is "infinityd" or "-infinityd" or "NaNd" or "\u221ed" or "-\u221ed")
        {
            return;
        }

        var s = CharStreams.fromString(nativeRepresentation);
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        if (d < 0)
            a.Should().BeOfType<UnaryExp>();
        else
            a.Should().BeOfType<Float8LiteralExp>();
    }

    [Fact]
    public void can_parse_floats_case1()
    {
        var s = CharStreams.fromString("4.940656458e-324");
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        a.Should().BeOfType<Float4LiteralExp>();
    }

    [Fact]
    public void can_parse_floats_case2()
    {
        var s = CharStreams.fromString("0.0");
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        a.Should().BeOfType<Float4LiteralExp>();
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
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        if (d < 0)
            a.Should().BeOfType<UnaryExp>();
        else
            a.Should().BeOfType<Int32LiteralExp>();
    }

    [Fact]
    public void can_parse_int_literals_case1()
    {
        int d = -1;
        var nativeRepresentation = $"{d}";
        if (nativeRepresentation is "infinity" or "-infinity" or "NaN" or "\u221e" or "-\u221e")
        {
            return;
        }

        var s = CharStreams.fromString(nativeRepresentation);
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        if (d < 0)
            a.Should().BeOfType<UnaryExp>();
        else
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
    [InlineData("3 + 7", Operator.ArithmeticAdd)]
    [InlineData("3 - 7", Operator.ArithmeticSubtract)]
    [InlineData("3 * 7", Operator.ArithmeticMultiply)]
    [InlineData("3 / 7", Operator.ArithmeticDivide)]
    [InlineData("3 ** 7", Operator.ArithmeticPow)]
    [InlineData("3 % 7", Operator.ArithmeticMod)]
    [InlineData("3 == 7", Operator.Equal)]
    [InlineData("3 != 7", Operator.NotEqual)]
    [InlineData("3 > 7", Operator.GreaterThan)]
    [InlineData("3 < 7", Operator.LessThan)]
    [InlineData("3 <= 7", Operator.LessThanOrEqual)]
    [InlineData("3 >= 7", Operator.GreaterThanOrEqual)]
    [InlineData("3 & 7", Operator.BitwiseAnd)]
    [InlineData("3 | 7", Operator.BitwiseOr)]
    [InlineData("3 << 7", Operator.BitwiseLeftShift)]
    [InlineData("3 >> 7", Operator.BitwiseRightShift)]
    [InlineData("3 && 7", Operator.LogicalAnd)]
    [InlineData("3 || 7", Operator.LogicalOr)]
    [InlineData("3 ^ 7", Operator.ArithmeticPow)]
    [InlineData("3 ~ 7", Operator.LogicalXor)]
    public void should_handle_all_kinds_of_binary_expressions(string exp, Operator op)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<BinaryExp>();
    }


    [Theory]
    [InlineData("+ 7", Operator.ArithmeticAdd)]
    [InlineData("+7", Operator.ArithmeticAdd)]
    [InlineData("- 7", Operator.ArithmeticSubtract)]
    [InlineData("-7", Operator.ArithmeticSubtract)]
    [InlineData("! 7", Operator.LogicalNot)]
    [InlineData("!7", Operator.LogicalNot)]
    public void should_handle_all_kinds_of_unary_expressions(string exp, Operator op)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<UnaryExp>();

    }

    [Theory]
    [InlineData("a = 5;")]
    [InlineData("a = 5 * 6;")]
    public void handles_assignment_statements(string exp)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.statement();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<AssignmentStatement>();
    }

    [Theory]
    [InlineData("a: int;", false)]
    [InlineData("a: int = 5;", true)]
    public void handles_vardecl_statements(string exp, bool shouldHaveInitialiserExpression)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.statement();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<VarDeclStatement>();
        var vds = a as VarDeclStatement;
        vds.VariableDecl.Should().NotBeNull();
        vds.VariableDecl.TypeName.Should().NotBeNull();
        vds.VariableDecl.TypeName.Value.Should().Be("int");
        vds.VariableDecl.Name.Should().NotBeNull();
        vds.VariableDecl.Name.Should().Be("a");
        if (shouldHaveInitialiserExpression)
        {
            vds.InitialValue.Should().NotBeNull();
        }
    }
}
