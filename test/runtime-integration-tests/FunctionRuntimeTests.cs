using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Tests for function definitions, calls, parameters, and overloading
/// </summary>
public class FunctionRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task SimpleFunctionCall_ShouldReturnCorrectValue()
    {
        // Arrange
        var sourceCode = """
            add(a: int, b: int): int {
                return a + b;
            }

            main(): int {
                return add(10, 15);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(25, "Function should return sum of parameters");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task MultipleParameterFunction_ShouldHandleAllParameters()
    {
        // Arrange
        var sourceCode = """
            calculate(a: int, b: int, c: int): int {
                // compute sum first to avoid relying on parser/operator-precedence behavior
                sum: int = a + b;
                return sum * c;
            }

            main(): int {
                return calculate(3, 7, 4);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(40, "Function should correctly handle multiple parameters: (3 + 7) * 4 = 40");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task RecursiveFunction_ShouldComputeFactorial()
    {
        // Arrange
        var sourceCode = """
            factorial(n: int): int {
                if (n <= 1) {
                    return 1;
                }
                return n * factorial(n - 1);
            }

            main(): int {
                return factorial(5);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(120, "Factorial of 5 should be 120");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task RecursiveFunction_ShouldComputeFibonacci()
    {
        // Arrange
        var sourceCode = """
            fibonacci(n: int): int {
                if (n <= 1) {
                    return n;
                }
                return fibonacci(n - 1) + fibonacci(n - 2);
            }

            main(): int {
                return fibonacci(7);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(13, "7th Fibonacci number should be 13");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task FunctionWithLocalVariables_ShouldManageScope()
    {
        // Arrange
        var sourceCode = """
            complex_calculation(x: int): int {
                temp1: int = x * 2;
                temp2: int = temp1 + 5;
                temp3: int = temp2 * 3;
                return temp3 - x;
            }

            main(): int {
                return complex_calculation(10);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(65, "Complex calculation should work: ((10*2+5)*3) - 10 = 65");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task NestedFunctionCalls_ShouldEvaluateCorrectly()
    {
        // Arrange
        var sourceCode = """
            double_value(x: int): int {
                return x * 2;
            }

            add_five(x: int): int {
                return x + 5;
            }

            main(): int {
                return double_value(add_five(10));
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(30, "Nested function calls should work: double_value(add_five(10)) = double_value(15) = 30");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task OverloadedFunction_ShouldSelectCorrectOverload()
    {
        // Arrange
        var sourceCode = """
            foo(i: int | i <= 15): int {
                return 1;
            }

            foo(i: int | i > 15): int {
                return 2;
            }

            foo(i: int): int {
                return 0; // fallback base case
            }

            main(): int {
                return foo(10) + foo(20);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(3, "Overloaded functions should select correct versions: foo(10)=1, foo(20)=2, total=3");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task OverloadedFunctionWithComplexConstraints_ShouldWork()
    {
        // Arrange
        var sourceCode = """
            categorize(age: int | age < 18): int {
                return 1; // child
            }

            categorize(age: int | age >= 18 && age < 65): int {
                return 2; // adult
            }

            categorize(age: int | age >= 65): int {
                return 3; // senior
            }

            categorize(age: int): int {
                return 0; // fallback base case
            }

            main(): int {
                return categorize(16) + categorize(30) + categorize(70);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(6, "Complex overload constraints should work: 1 + 2 + 3 = 6");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }

    [Fact]
    public async Task FunctionWithComplexControlFlow_ShouldExecuteCorrectly()
    {
        // Arrange
        var sourceCode = """
            find_max(a: int, b: int, c: int): int {
                max: int = a;
                if (b > max) {
                    max = b;
                }
                if (c > max) {
                    max = c;
                }
                return max;
            }

            main(): int {
                return find_max(15, 42, 23);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(42, "Function should find maximum value among three numbers");
        result.StandardError.Should().BeEmpty("No errors should occur");
    }
}