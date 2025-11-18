using Antlr4.Runtime;
using ast;
using compiler.LangProcessingPhases;
using Fifth;
using FluentAssertions;

namespace ast_tests;

/// <summary>
/// Tests for multiple type parameters (T078, Phase 7 - User Story 5).
/// Verifies that multiple independent type parameters work correctly.
/// </summary>
public class MultipleTypeParamTests
{
    private static FifthParser GetParserFor(ICharStream s)
    {
        var lexer = new FifthLexer(s);
        var tokens = new CommonTokenStream(lexer);
        return new FifthParser(tokens);
    }

    [Test]
    public void Can_Parse_Class_With_Two_Type_Parameters()
    {
        // Arrange
        var classSrc = """
            class Pair<T1, T2> {
                first: T1;
                second: T2;
            }
            
            main(): int {
                return 0;
            }
            """;
        
        var s = CharStreams.fromString(classSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var classDef = module.Classes.First();
        
        classDef.TypeParameters.Should().HaveCount(2);
        classDef.TypeParameters[0].Name.Value.Should().Be("T1");
        classDef.TypeParameters[1].Name.Value.Should().Be("T2");
    }

    [Test]
    public void Can_Parse_Function_With_Three_Type_Parameters()
    {
        // Arrange
        var funcSrc = """
            triple<T1, T2, T3>(a: T1, b: T2, c: T3): int {
                return 0;
            }
            
            main(): int {
                return 0;
            }
            """;
        
        var s = CharStreams.fromString(funcSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var funcDef = module.Functions.First() as FunctionDef;
        
        funcDef.Should().NotBeNull();
        funcDef!.TypeParameters.Should().HaveCount(3);
        funcDef.TypeParameters[0].Name.Value.Should().Be("T1");
        funcDef.TypeParameters[1].Name.Value.Should().Be("T2");
        funcDef.TypeParameters[2].Name.Value.Should().Be("T3");
    }

    [Test]
    public void Can_Parse_Dictionary_With_Key_Value_Type_Parameters()
    {
        // Arrange
        var classSrc = """
            class Dictionary<TKey, TValue> {
                keys: [TKey];
                values: [TValue];
            }
            
            main(): int {
                return 0;
            }
            """;
        
        var s = CharStreams.fromString(classSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var classDef = module.Classes.First();
        
        classDef.Name.Value.Should().Be("Dictionary");
        classDef.TypeParameters.Should().HaveCount(2);
        classDef.TypeParameters[0].Name.Value.Should().Be("TKey");
        classDef.TypeParameters[1].Name.Value.Should().Be("TValue");
    }
}
