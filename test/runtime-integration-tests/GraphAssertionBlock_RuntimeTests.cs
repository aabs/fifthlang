using FluentAssertions;

namespace runtime_integration_tests;

public class GraphAssertionBlock_RuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task EmptyGraphBlock_WithDefaultStore_ShouldCompile()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            main(): int {
                <{ }>;
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_default_store");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task EmptyGraphBlock_WithoutStore_ShouldFailCompilation()
    {
        var src = """
            main(): int {
                <{ }>;
                return 0;
            }
            """;

        Func<Task> act = async () => await CompileSourceAsync(src, "gab_no_store");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*requires an explicit store declaration*");
    }
}
