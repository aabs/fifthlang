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

    [Fact]
    public async Task Lambda_ShadowingOuterVar_ShouldFailWithDiagnostic()
    {
        // Lambda parameter shadows outer variable - should fail compilation
        var sourceCode = """
            main(): int {
                y: int = 10;
                f : [int] -> int = fun(y: int): int { return y + 1; };
                return f(5);
            }
            """;

        // Act & Assert
        var act = async () => await CompileSourceAsync(sourceCode);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("ERR_LF_SHADOWING_NOT_ALLOWED") &&
                        ex.Message.Contains("shadows an outer variable"));
    }

    [Fact]
    public async Task Lambda_AssigningCapturedVar_ShouldFailWithDiagnostic()
    {
        // Lambda assigns to captured variable - should fail compilation
        var sourceCode = """
            main(): int {
                y: int = 10;
                f : [int] -> int = fun(x: int): int {
                    y = x + 1;
                    return y;
                };
                return f(5);
            }
            """;

        // Act & Assert
        var act = async () => await CompileSourceAsync(sourceCode);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("ERR_LF_CAPTURED_VARIABLE_ASSIGNED") &&
                        ex.Message.Contains("captures are read-only"));
    }
}
