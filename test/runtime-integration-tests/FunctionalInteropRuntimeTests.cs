using FluentAssertions;

namespace runtime_integration_tests;

public class FunctionalInteropRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task Functional_MapFilterFoldleft_WithLambda_ShouldRun()
    {
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4];
                doubled: [int] = map(numbers, fun(x: int): int { return x * 2; });
                evens: [int] = filter(doubled, fun(x: int): bool { return x % 2 == 0; });
                total: int = foldleft(evens, 0, fun(acc: int, x: int): int { return acc + x; });
                return total;
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode, "functional_map_filter_foldleft");
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(20);
        result.StandardError.Should().BeEmpty();
    }

    [Fact]
    public async Task Functional_Zip_WithLambda_ShouldRun()
    {
        var sourceCode = """
            main(): int {
                a: [int] = [1, 2, 3];
                b: [int] = [10, 20, 30];
                summed: [int] = zip(a, b, fun(x: int, y: int): int { return x + y; });
                return summed[2];
            }
            """;

        var executablePath = await CompileSourceAsync(sourceCode, "functional_zip");
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(33);
        result.StandardError.Should().BeEmpty();
    }
}
