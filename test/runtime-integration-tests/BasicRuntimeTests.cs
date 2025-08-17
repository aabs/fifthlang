using FluentAssertions;
using Xunit;

namespace runtime_integration_tests;

/// <summary>
/// Tests for basic arithmetic, expressions, and simple programs
/// </summary>
public class BasicRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task SimpleReturnInt_ShouldReturnCorrectExitCode()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                return 42;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(42, "Program should return 42 as exit code");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task ArithmeticOperations_ShouldComputeCorrectly()
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
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(25, "10 + 15 should equal 25");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task SubtractionOperation_ShouldComputeCorrectly()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 50;
                y: int = 17;
                return x - y;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(33, "50 - 17 should equal 33");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task MultiplicationOperation_ShouldComputeCorrectly()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 6;
                y: int = 7;
                return x * y;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(42, "6 * 7 should equal 42");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task DivisionOperation_ShouldComputeCorrectly()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 84;
                y: int = 2;
                return x / y;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(42, "84 / 2 should equal 42");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task ModuloOperation_ShouldComputeCorrectly()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 47;
                y: int = 5;
                return x % y;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(2, "47 % 5 should equal 2");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task NestedExpressions_ShouldEvaluateCorrectly()
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
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(19, "(2 + 3) * 4 - 1 should equal 19");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task BooleanExpressions_ShouldEvaluateCorrectly()
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
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(1, "Boolean expression (10 < 20 && 20 > 15) should be true");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task VariableDeclarationAndAssignment_ShouldWorkCorrectly()
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
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(15, "5 * 2 + 5 should equal 15");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }
}