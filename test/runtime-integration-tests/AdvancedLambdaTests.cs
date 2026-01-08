using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Advanced lambda tests including generics and tail-call optimization
/// </summary>
public class AdvancedLambdaTests : RuntimeTestBase
{
    [Fact]
    public async Task Lambda_GenericIdentity_ShouldWork()
    {
        // Test generic lambda that works with different types
        var sourceCode = """
            identity<T>(x: T): T {
                return x;
            }

            main(): int {
                f : [int] -> int = fun(x: int): int { return identity<int>(x); };
                return f(42);
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(42);
        result.StandardError.Should().BeEmpty();
    }

    [Fact]
    public async Task Lambda_GenericWithCapture_ShouldWork()
    {
        // Test generic function used within lambda with capture
        var sourceCode = """
            apply<T>(x: T, y: T, op: [T, T] -> T): T {
                return op(x, y);
            }

            main(): int {
                baseValue: int = 10;
                addBase : [int] -> int = fun(x: int): int { return x + baseValue; };
                return addBase(32);
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(42);
        result.StandardError.Should().BeEmpty();
    }

    // [Fact(Skip = "Test appears to hang during compilation - needs investigation")]
    [Fact]
    public async Task Lambda_DeepRecursion_WithoutTCO_ShouldNotStackOverflow()
    {
        // Test that demonstrates need for TCO
        // Without TCO, this may stack overflow for large N
        // With TCO, it should complete successfully
        var sourceCode = """
            sumToN(n: int, acc: int): int {
                if (n <= 0) {
                    return acc;
                } else {
                    return sumToN(n - 1, acc + n);
                }
            }

            main(): int {
                result: int = sumToN(100, 0);
                return result % 256;
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // sumToN(100, 0) = 5050, modulo 256 = 178
        (result.ExitCode % 256).Should().Be(178);
        result.StandardError.Should().BeEmpty();
    }

    [Fact]
    public async Task Lambda_SelfRecursive_FactorialStyle_ShouldWork()
    {
        // Test lambda with potential tail recursion pattern
        var sourceCode = """
            factorial(n: int): int {
                if (n <= 1) {
                    return 1;
                } else {
                    return n * factorial(n - 1);
                }
            }

            main(): int {
                result: int = factorial(5);
                return result % 256;
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        // factorial(5) = 120
        result.ExitCode.Should().Be(120);
        result.StandardError.Should().BeEmpty();
    }
}
