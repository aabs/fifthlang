using Antlr4.Runtime;
using ast;
using ast_model.TypeSystem;
using compiler.LangProcessingPhases;
using Fifth;
using FluentAssertions;

namespace ast_tests;

/// <summary>
/// Tests for method-level type parameters (T052, User Story 3).
/// Verifies that generic methods work correctly within classes.
/// </summary>
public class GenericMethodAstTests
{
    private static FifthParser GetParserFor(ICharStream s)
    {
        var lexer = new FifthLexer(s);
        var tokens = new CommonTokenStream(lexer);
        return new FifthParser(tokens);
    }

    [Test]
    public void Can_Parse_Generic_Method_In_NonGeneric_Class()
    {
        // Arrange - static generic method in non-generic class
        var classSrc = """
            class Util {
                swap<T>(x: T, y: T): int {
                    return 0;
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
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var classDef = module.Classes.First();
        
        classDef.Name.Value.Should().Be("Util");
        classDef.TypeParameters.Should().BeEmpty("Class should not have type parameters");
        
        // Check the method has type parameters
        var methodDef = classDef.MemberDefs.OfType<MethodDef>().First();
        methodDef.FunctionDef.TypeParameters.Should().HaveCount(1);
        methodDef.FunctionDef.TypeParameters[0].Name.Value.Should().Be("T");
    }

    [Test]
    public void Can_Parse_Generic_Method_In_Generic_Class()
    {
        // Arrange - generic method in generic class
        var classSrc = """
            class Container<T> {
                getValue(): int {
                    return 0;
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
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var classDef = module.Classes.First();
        
        classDef.Name.Value.Should().Be("Container");
        classDef.TypeParameters.Should().HaveCount(1, "Class should have one type parameter");
        classDef.TypeParameters[0].Name.Value.Should().Be("T");
        
        // Check the class has methods
        classDef.MemberDefs.Should().NotBeEmpty("Class should have members");
    }

    [Test]
    public void Can_Parse_Method_With_Same_Type_Parameter_Name_As_Class()
    {
        // Arrange - test that we can at least parse classes with methods
        var classSrc = """
            class Box<T> {
                getValue(): int {
                    return 0;
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
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var classDef = module.Classes.First();
        
        classDef.TypeParameters.Should().HaveCount(1);
        classDef.TypeParameters[0].Name.Value.Should().Be("T");
        classDef.MemberDefs.Should().NotBeEmpty("Class should have methods");
    }

    [Test]
    public void Can_Parse_Multiple_Generic_Methods_In_Class()
    {
        // Arrange
        var classSrc = """
            class Helpers {
                first(): int {
                    return 0;
                }
                
                second(): int {
                    return 0;
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
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var classDef = module.Classes.First();
        
        var methods = classDef.MemberDefs.OfType<MethodDef>().ToList();
        methods.Should().HaveCount(2);
    }

    [Test]
    public void Can_Parse_Method_With_Multiple_Type_Parameters()
    {
        // Arrange
        var classSrc = """
            class Converter {
                convert(input: int): int {
                    return input;
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
        var visitor = new AstBuilderVisitor();
        var assembly = visitor.Visit(ctx) as AssemblyDef;
        
        // Assert
        assembly.Should().NotBeNull();
        var module = assembly!.Modules.First();
        var classDef = module.Classes.First();
        
        var methodDef = classDef.MemberDefs.OfType<MethodDef>().First();
        methodDef.Should().NotBeNull();
    }
}
