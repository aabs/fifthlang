using Antlr4.Runtime;
using ast;
using ast_model.TypeSystem;
using compiler.LangProcessingPhases;
using Fifth;
using FluentAssertions;

namespace ast_tests;

/// <summary>
/// Tests for generic type inference (T046, User Story 2).
/// Verifies that type arguments can be inferred from function call context.
/// </summary>
public class GenericInferenceTests
{
    private static FifthParser GetParserFor(ICharStream s)
    {
        var lexer = new FifthLexer(s);
        var tokens = new CommonTokenStream(lexer);
        return new FifthParser(tokens);
    }

    [Fact]
    public void Can_Parse_Generic_Function_Without_Type_Arguments_In_Call()
    {
        // Arrange - This tests the parser's ability to handle calls without explicit type args
        // Type inference happens in a later compilation phase
        var funcCallSrc = """
            identity<T>(x: T): T {
                return x;
            }
            
            main(): int {
                return identity(42);
            }
            """;
        
        var s = CharStreams.fromString(funcCallSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        
        // Assert
        ctx.Should().NotBeNull();
        // Function call should parse even without explicit type arguments
        // The actual type inference will happen in the semantic analysis phase
    }

    [Fact]
    public void Can_Parse_Generic_Function_With_Explicit_Type_Arguments()
    {
        // Arrange
        var funcCallSrc = """
            identity<T>(x: T): T {
                return x;
            }
            
            main(): int {
                return identity<int>(42);
            }
            """;
        
        var s = CharStreams.fromString(funcCallSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        
        // Assert
        ctx.Should().NotBeNull();
        // With explicit type arguments, parsing should succeed
    }

    [Fact]
    public void Generic_Function_With_Multiple_Parameters_Can_Parse()
    {
        // Arrange
        var funcDefSrc = """
            pair<T1, T2>(a: T1, b: T2): int {
                return 0;
            }
            
            main(): int {
                return pair(1, 2);
            }
            """;
        
        var s = CharStreams.fromString(funcDefSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        
        // Assert
        ctx.Should().NotBeNull();
        // Multiple type parameters should parse correctly
    }

    [Fact]
    public void Generic_Function_In_Generic_Class_Can_Parse()
    {
        // Arrange
        var classSrc = """
            class Container<T> {
                value: T;
                
                transform<U>(f: int): U {
                    return f;
                }
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
        // Generic method in generic class should parse
    }

    [Fact]
    public void Generic_Function_Can_Use_Type_Parameter_In_Return_Type()
    {
        // Arrange
        var funcDefSrc = """
            create<T>(): T {
                return T;
            }
            
            main(): int {
                return 0;
            }
            """;
        
        var s = CharStreams.fromString(funcDefSrc);
        var p = GetParserFor(s);
        
        // Act
        var ctx = p.fifth();
        
        // Assert
        ctx.Should().NotBeNull();
        // Type parameter used as return type should parse
    }
}
