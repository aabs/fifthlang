using FluentAssertions;

namespace ast_tests;

public class NamespaceImportGraphTests
{
    [Fact]
    public void NamespaceImportGraph_ShouldHandleCycles()
    {
        var graphType = Type.GetType("compiler.NamespaceResolution.NamespaceImportGraph, compiler");
        graphType.Should().NotBeNull("NamespaceImportGraph should exist in compiler.NamespaceResolution");

        var graph = Activator.CreateInstance(graphType!);
        var addImport = graphType!.GetMethod("AddImport", new[] { typeof(string), typeof(string) });
        addImport.Should().NotBeNull("NamespaceImportGraph should expose AddImport(string, string)");

        addImport!.Invoke(graph, new object?[] { "A", "B" });
        addImport.Invoke(graph, new object?[] { "B", "C" });
        addImport.Invoke(graph, new object?[] { "C", "A" });

        var traverse = graphType.GetMethod("TraverseImports", new[] { typeof(string) });
        traverse.Should().NotBeNull("NamespaceImportGraph should expose TraverseImports(string)");

        var result = traverse!.Invoke(graph, new object?[] { "A" }) as IEnumerable<string>;
        result.Should().NotBeNull("TraverseImports should return a sequence of namespace names");

        var list = result!.ToList();
        list.Should().Contain(new[] { "B", "C" });
        list.Should().OnlyHaveUniqueItems("traversal should be cycle-safe and idempotent");
    }

    [Fact]
    public void NamespaceImportGraph_ShouldAvoidDuplicateTraversal()
    {
        var graphType = Type.GetType("compiler.NamespaceResolution.NamespaceImportGraph, compiler");
        graphType.Should().NotBeNull("NamespaceImportGraph should exist in compiler.NamespaceResolution");

        var graph = Activator.CreateInstance(graphType!);
        var addImport = graphType!.GetMethod("AddImport", new[] { typeof(string), typeof(string) });
        addImport.Should().NotBeNull();

        addImport!.Invoke(graph, new object?[] { "Root", "Utils" });
        addImport.Invoke(graph, new object?[] { "Root", "Utils" });
        addImport.Invoke(graph, new object?[] { "Root", "Utils" });

        var traverse = graphType.GetMethod("TraverseImports", new[] { typeof(string) });
        traverse.Should().NotBeNull();

        var result = traverse!.Invoke(graph, new object?[] { "Root" }) as IEnumerable<string>;
        result.Should().NotBeNull();
        result!.Should().BeEquivalentTo(new[] { "Utils" }, opts => opts.WithStrictOrdering());
    }
}
