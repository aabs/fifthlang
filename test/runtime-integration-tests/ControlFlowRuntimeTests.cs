using FluentAssertions;
using Xunit;

namespace runtime_integration_tests;

/// <summary>
/// Tests for control flow constructs (if/else, while loops, etc.)
/// </summary>
public class ControlFlowRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task IfStatement_WhenConditionTrue_ShouldExecuteTrueBranch()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 20;
                if (x > 10) {
                    return 1;
                }
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(1, "Should execute true branch when condition is true");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task IfStatement_WhenConditionFalse_ShouldSkipTrueBranch()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 5;
                if (x > 10) {
                    return 1;
                }
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(0, "Should skip true branch when condition is false");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task IfElseStatement_WhenConditionTrue_ShouldExecuteTrueBranch()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 15;
                if (x > 10) {
                    return 1;
                } else {
                    return 2;
                }
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(1, "Should execute true branch when condition is true");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task IfElseStatement_WhenConditionFalse_ShouldExecuteElseBranch()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 5;
                if (x > 10) {
                    return 1;
                } else {
                    return 2;
                }
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(2, "Should execute else branch when condition is false");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task WhileLoop_ShouldExecuteCorrectNumberOfTimes()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                count: int = 0;
                i: int = 0;
                while (i < 5) {
                    count = count + 1;
                    i = i + 1;
                }
                return count;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(5, "While loop should execute 5 times");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task WhileLoop_WithFalseCondition_ShouldNotExecute()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                count: int = 0;
                i: int = 10;
                while (i < 5) {
                    count = count + 1;
                    i = i + 1;
                }
                return count;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(0, "While loop should not execute when condition is initially false");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task NestedIfStatements_ShouldEvaluateCorrectly()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                x: int = 15;
                y: int = 25;
                if (x > 10) {
                    if (y > 20) {
                        return 3;
                    } else {
                        return 2;
                    }
                } else {
                    return 1;
                }
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(3, "Nested if statements should evaluate correctly");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task ComplexBooleanConditions_ShouldEvaluateCorrectly()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                a: int = 10;
                b: int = 20;
                c: int = 30;
                if (a < b && b < c && (a + b) < c) {
                    return 1;
                }
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(1, "Complex boolean conditions should evaluate correctly");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task WhileLoopWithComplexCondition_ShouldWorkCorrectly()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                sum: int = 0;
                i: int = 1;
                while (i <= 10 && sum < 50) {
                    sum = sum + i;
                    i = i + 1;
                }
                return sum;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(55, "While loop with complex condition should work correctly (1+2+...+10 = 55)");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }
}