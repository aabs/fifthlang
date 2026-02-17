using FluentAssertions;

namespace runtime_integration_tests;

[Trait("Category", "Triple")]
[Trait("Category", "Mutating")]
public class TripleMutatingOperatorsTests : RuntimeTestBase
{
    [Fact]
    public async Task Graph_PlusAssign_Triple_ShouldAddTripleToGraph()
    {
        var src = """
            alias s as <http://example.org/>;
            main(): int {
                g: graph = KG.CreateGraph();
                // Test += operator: adds triple to graph
                g += <s:subject, s:predicate, s:object>;
                
                // Verify graph contains the triple
                if (g.CountTriples() != 1) {
                    return 1;
                }
                
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "graph_plusassign_triple");
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0, "g += triple should add the triple to the graph");
    }

    [Fact]
    public async Task Graph_MinusAssign_Triple_ShouldRemoveTripleFromGraph()
    {
        var src = """
            main(): int {
                g = KG.CreateGraph();
                subj = g.CreateUri("http://example.org/subject");
                pred = g.CreateUri("http://example.org/predicate");
                obj = g.CreateUri("http://example.org/object");
                triple = <subj, pred, obj>;
                
                // Add triple first
                g += triple;
                
                // Test -= operator: removes triple from graph
                g -= triple;
                
                // Verify graph is empty
                if (g.CountTriples() != 0) {
                    return 1;
                }
                
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "graph_minusassign_triple");
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0, "g -= triple should remove the triple from the graph");
    }

    [Fact]
    public async Task Graph_PlusAssign_MultipleTimes_ShouldAccumulateTriples()
    {
        var src = """
            main(): int {
                g = KG.CreateGraph();
                subj = g.CreateUri("http://example.org/subject");
                pred1 = g.CreateUri("http://example.org/predicate1");
                pred2 = g.CreateUri("http://example.org/predicate2");
                obj = g.CreateUri("http://example.org/object");
                
                triple1 = <subj, pred1, obj>;
                triple2 = <subj, pred2, obj>;
                
                // Add multiple triples
                g += triple1;
                g += triple2;
                
                // Verify graph contains both triples
                if (g.CountTriples() != 2) {
                    return 1;
                }
                
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "graph_plusassign_multiple");
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0, "g += triple (multiple times) should accumulate triples");
    }

    [Fact]
    public async Task Graph_MinusAssign_NonExistentTriple_ShouldBeIdempotent()
    {
        var src = """
            main(): int {
                g = KG.CreateGraph();
                subj = g.CreateUri("http://example.org/subject");
                pred1 = g.CreateUri("http://example.org/predicate1");
                pred2 = g.CreateUri("http://example.org/predicate2");
                obj = g.CreateUri("http://example.org/object");
                
                triple1 = <subj, pred1, obj>;
                triple2 = <subj, pred2, obj>;
                
                // Add only triple1
                g += triple1;
                
                // Try to remove triple2 (not in graph)
                g -= triple2;
                
                // Verify graph still contains triple1
                if (g.CountTriples() != 1) {
                    return 1;
                }
                
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "graph_minusassign_nonexistent");
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0, "g -= triple (non-existent) should be idempotent");
    }
}
