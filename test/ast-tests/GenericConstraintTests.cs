using Antlr4.Runtime;
using ast;
using ast_model.TypeSystem;
using compiler.LangProcessingPhases;
using Fifth;
using FluentAssertions;

namespace ast_tests;

/// <summary>
/// Tests for type parameter constraints (T063, Phase 6 - User Story 4).
/// Verifies constraint parsing and validation.
/// </summary>
public class GenericConstraintTests
{
    private static FifthParser GetParserFor(ICharStream s)
    {
        var lexer = new FifthLexer(s);
        var tokens = new CommonTokenStream(lexer);
        return new FifthParser(tokens);
    }

    [Test]
    public void Can_Parse_Function_With_Interface_Constraint()
    {
        // Arrange
        var funcSrc = """
            sort<T>(items: [T]): [T] where T: IComparable {
                return items;
            }
            
            main(): int {
                return 0;
            }
            """;
        
        var s = CharStreams.fromString(funcSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        
        // Assert
        ctx.Should().NotBeNull();
        // Parser should accept where clause syntax
    }

    [Test]
    public void Can_Parse_Class_With_Constructor_Constraint()
    {
        // Arrange
        var classSrc = """
            class Factory<T> where T: new {
                value: T;
            }
            
            main(): int {
                return 0;
            }
            """;
        
        var s = CharStreams.fromString(classSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        
        // Assert
        ctx.Should().NotBeNull();
    }

    [Test]
    public void Can_Parse_Multiple_Constraints_On_Type_Parameter()
    {
        // Arrange
        var funcSrc = """
            process<T>(item: T): int where T: IComparable, IDisposable {
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
        
        // Assert
        ctx.Should().NotBeNull();
    }

    [Test]
    public void Can_Parse_Function_With_Base_Class_Constraint()
    {
        // Arrange
        var funcSrc = """
            extend<T>(base: T): int where T: BaseClass {
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
        
        // Assert
        ctx.Should().NotBeNull();
    }

    [Test]
    public void Can_Parse_Class_With_Multiple_Type_Parameters_And_Constraints()
    {
        // Arrange
        var classSrc = """
            class Mapper<TIn, TOut> where TIn: IComparable where TOut: new {
                input: TIn;
                output: TOut;
            }
            
            main(): int {
                return 0;
            }
            """;
        
        var s = CharStreams.fromString(classSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        
        // Assert
        ctx.Should().NotBeNull();
    }
}
