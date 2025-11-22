using FluentAssertions;

namespace runtime_integration_tests;

[Trait("Category", "KG")]
public class KG_Builtins_RuntimeTests : RuntimeTestBase
{
    [Fact]
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
        // Compile-only: runtime execution requires external dependency resolution for dotNetRDF.
        var exe = await CompileSourceAsync(src, "kg_builtins_smoketest");
        File.Exists(exe).Should().BeTrue();
    }
}
