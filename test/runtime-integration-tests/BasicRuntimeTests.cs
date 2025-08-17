using FluentAssertions;
using Xunit;

namespace runtime_integration_tests;

/// <summary>
/// Tests for basic arithmetic, expressions, and simple programs
/// NOTE: Current PE emission generates hardcoded "Hello from Fifth!" program.
/// These tests are structured to be updated when PE emission is fully implemented.
/// </summary>
public class BasicRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task SimpleReturnInt_ShouldGenerateExecutable()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                return 42;
            }
            """;

        // Act - Test that compilation succeeds and generates executable
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert - File should be created
        File.Exists(executablePath).Should().BeTrue("Executable should be generated");
        var fileInfo = new FileInfo(executablePath);
        fileInfo.Length.Should().BeGreaterThan(0, "Executable should have content");
        
        // TODO: When PE emission is fixed, expect exit code 42
        // For now, just verify it compiles successfully
    }

    [Fact]
    public async Task ArithmeticOperations_ShouldCompileSuccessfully()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 10;
                y: int = 15;
                return x + y;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Arithmetic operations should compile to executable");
        
        // TODO: When PE emission is fixed, expect exit code 25 (10 + 15)
    }

    [Fact]
    public async Task ComplexArithmeticExpressions_ShouldCompile()
    {
        // Test each arithmetic operation type
        var testCases = new[]
        {
            ("Addition", "return 10 + 15;", 25),
            ("Subtraction", "return 50 - 17;", 33),
            ("Multiplication", "return 6 * 7;", 42),
            ("Division", "return 84 / 2;", 42),
            ("Modulo", "return 47 % 5;", 2)
        };

        foreach (var (operation, expression, expectedResult) in testCases)
        {
            // Arrange
            var sourceCode = $@"
main(): int {{
    {expression}
}}";

            // Act
            var executablePath = await CompileSourceAsync(sourceCode, $"test_{operation.ToLower()}");
            
            // Assert
            File.Exists(executablePath).Should().BeTrue($"{operation} operation should compile to executable");
            
            // TODO: When PE emission is fixed, expect exit code {expectedResult}
        }
    }

    [Fact]
    public async Task NestedExpressions_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 2;
                y: int = 3;
                z: int = 4;
                return (x + y) * z - 1;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Nested expressions should compile");
        
        // TODO: When PE emission is fixed, expect exit code 19 ((2 + 3) * 4 - 1)
    }

    [Fact]
    public async Task BooleanExpressions_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 10;
                y: int = 20;
                if (x < y && y > 15) {
                    return 1;
                }
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Boolean expressions should compile");
        
        // TODO: When PE emission is fixed, expect exit code 1 (condition is true)
    }

    [Fact]
    public async Task VariableDeclarationAndAssignment_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                a: int = 5;
                b: int = a * 2;
                c: int = b + a;
                return c;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Variable declarations and assignments should compile");
        
        // TODO: When PE emission is fixed, expect exit code 15 (5 * 2 + 5)
    }

    [Fact]
    public async Task MultipleVariableTypes_ShouldCompile()
    {
        // Arrange - Test different variable types if supported
        var sourceCode = """
            main(): int {
                intVar: int = 42;
                return intVar;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Multiple variable types should compile");
        
        // TODO: When PE emission is fixed, expect exit code 42
    }
}