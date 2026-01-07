using FluentAssertions;

namespace runtime_integration_tests;

public class HofTests : RuntimeTestBase
{
    [Fact]
    public async Task Hof_ReturningLambda_ShouldWork()
    {
        var sourceCode = """
            makeAdder(y: int): [int] -> int {
                return fun(x: int): int { return x + y; };
            }

            main(): int {
                f: [int] -> int = makeAdder(10);
                return f(32);
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(42);
        result.StandardError.Should().BeEmpty();
    }

    [Fact]
    public async Task Hof_ComposeAndReturnLambda_ShouldWork()
    {
        var sourceCode = """
            inc(x: int): int { return x + 1; }
            dbl(x: int): int { return x * 2; }

            compose(f: [int] -> int, g: [int] -> int): [int] -> int {
                return fun(x: int): int {
                    return f(g(x));
                };
            }

            main(): int {
                f: [int] -> int = fun(z: int): int { return inc(z); };
                g: [int] -> int = fun(z: int): int { return dbl(z); };

                h: [int] -> int = compose(f, g);
                return h(20);
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(41);
        result.StandardError.Should().BeEmpty();
    }
}
