using Antlr4.Runtime;
using ast;
using ast_model.TypeSystem;
using compiler.LangProcessingPhases;
using Fifth;
using FluentAssertions;

namespace ast_tests;

/// <summary>
/// Tests for parsing and building AST for generic class definitions (T018)
/// User Story 1: Generic Collection Classes
/// </summary>
public class GenericClassAstBuilderTests
{
    private static FifthParser GetParserFor(ICharStream s)
    {
        var lexer = new FifthLexer(s);
        var tokens = new CommonTokenStream(lexer);
        return new FifthParser(tokens);
    }

    [Fact]
    public void Can_Parse_Generic_Class_With_Single_Type_Parameter()
    {
        // Arrange
        var classDefSrc = """
            class Stack<T> {
                items: [T];
            }
            """;
        var s = CharStreams.fromString(classDefSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.class_definition();
        ctx.Should().NotBeNull();
        
        var visitor = new AstBuilderVisitor();
        var classDef = visitor.Visit(ctx) as ClassDef;
        
        // Assert
        classDef.Should().NotBeNull();
        classDef!.Name.Value.Should().Be("Stack");
        classDef.TypeParameters.Should().HaveCount(1);
        classDef.TypeParameters[0].Name.Value.Should().Be("T");
    }

    [Fact]
    public void Can_Parse_Generic_Class_Without_Type_Parameters()
    {
        // Arrange
        var classDefSrc = """
            class NonGenericClass {
                value: int;
            }
            """;
        var s = CharStreams.fromString(classDefSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.class_definition();
        var visitor = new AstBuilderVisitor();
        var classDef = visitor.Visit(ctx) as ClassDef;
        
        // Assert
        classDef.Should().NotBeNull();
        classDef!.Name.Value.Should().Be("NonGenericClass");
        classDef.TypeParameters.Should().BeEmpty();
    }

    [Fact]
    public void Can_Parse_Generic_Function_With_Single_Type_Parameter()
    {
        // Arrange
        var funcDefSrc = """
            identity<T>(x: T): T {
                return x;
            }
            """;
        var s = CharStreams.fromString(funcDefSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.function_declaration();
        var visitor = new AstBuilderVisitor();
        var funcDef = visitor.Visit(ctx) as FunctionDef;
        
        // Assert
        funcDef.Should().NotBeNull();
        funcDef!.Name.Value.Should().Be("identity");
        funcDef.TypeParameters.Should().HaveCount(1);
        funcDef.TypeParameters[0].Name.Value.Should().Be("T");
    }

    [Fact]
    public void Can_Parse_Generic_Class_With_Multiple_Type_Parameters()
    {
        // Arrange
        var classDefSrc = """
            class Dictionary<TKey, TValue> {
                entries: [(TKey, TValue)];
            }
            """;
        var s = CharStreams.fromString(classDefSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.class_definition();
        var visitor = new AstBuilderVisitor();
        var classDef = visitor.Visit(ctx) as ClassDef;
        
        // Assert
        classDef.Should().NotBeNull();
        classDef!.Name.Value.Should().Be("Dictionary");
        classDef.TypeParameters.Should().HaveCount(2);
        classDef.TypeParameters[0].Name.Value.Should().Be("TKey");
        classDef.TypeParameters[1].Name.Value.Should().Be("TValue");
    }

    [Fact]
    public void Can_Parse_Generic_Function_With_Multiple_Type_Parameters()
    {
        // Arrange
        var funcDefSrc = """
            pair<T1, T2>(a: T1, b: T2): int {
                return 0;
            }
            """;
        var s = CharStreams.fromString(funcDefSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.function_declaration();
        var visitor = new AstBuilderVisitor();
        var funcDef = visitor.Visit(ctx) as FunctionDef;
        
        // Assert
        funcDef.Should().NotBeNull();
        funcDef!.Name.Value.Should().Be("pair");
        funcDef.TypeParameters.Should().HaveCount(2);
        funcDef.TypeParameters[0].Name.Value.Should().Be("T1");
        funcDef.TypeParameters[1].Name.Value.Should().Be("T2");
    }
}
