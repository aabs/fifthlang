using FluentAssertions;

namespace runtime_integration_tests;

public class LambdaRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task Lambda_NoCapture_AssignAndInvoke_ShouldWork()
    {
        var sourceCode = """
            main(): int {
                f : [int] -> int = fun(x: int): int { return x + 1; };
                return f(41);
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(42);
        result.StandardError.Should().BeEmpty();
    }

    [Fact]
    public async Task Lambda_CaptureOuterVar_ByValue_ShouldWork()
    {
        var sourceCode = """
            main(): int {
                y: int = 10;
                f : [int] -> int = fun(x: int): int { return x + y; };
                return f(5);
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(15);
        result.StandardError.Should().BeEmpty();
    }

    [Fact]
    public async Task Lambda_PassAsParam_AndInvokeInsideFunction_ShouldWork()
    {
        var sourceCode = """
            apply(f: [int] -> int, x: int): int {
                return f(x);
            }

            main(): int {
                f : [int] -> int = fun(z: int): int { return z + 1; };
                return apply(f, 41);
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(42);
        result.StandardError.Should().BeEmpty();
    }
}
