using FluentAssertions;

namespace runtime_integration_tests;

[Category("KG")]
public class KG_Builtins_RuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task KG_CreateGraph_And_ConnectToRemoteStore_ShouldCompileAndRun()
    {
        var src = """
            main(): int {
                // Create a graph via built-in and ensure it's non-null
                if (KG.CreateGraph() == null) { return 1; }
                // Connect to a remote store (no actual network call during compile) and ensure it's non-null
                if (KG.ConnectToRemoteStore("http://example.org/store") == null) { return 1; }
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "kg_builtins_smoketest");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }
}
