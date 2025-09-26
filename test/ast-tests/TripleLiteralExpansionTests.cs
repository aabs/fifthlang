// T010: List expansion AST tests (expected to fail before expansion visitor implemented)
using System.Linq;
using FluentAssertions;
using Xunit;

namespace ast_tests;

public class TripleLiteralExpansionTests
{
    private (object ast, string diagnostics) Parse(string code)
    {
        throw new System.NotImplementedException("Wire parser + expansion pipeline (T010).");
    }

    [Fact(DisplayName = "T010_01 List object expands into multiple triple literals (pre-lowering)")]
    public void ListExpansionProducesMultipleTripleNodes()
    {
        const string code = @"alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, [ex:o1, ex:o2, ex:o3]>; return 0; }";
        var (ast, diags) = Parse(code);
        ast.Should().NotBeNull();
        // Expect diagnostics empty for valid expansion case.
        diags.Should().BeEmpty();
        // TODO: Assert 3 TripleLiteralExp nodes produced (placeholder, depends on final AST API)
    }

    [Fact(DisplayName = "T010_02 Nested list rejected with TRPL006")]
    public void NestedListRejected()
    {
        const string code = @"alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, [[ex:o1, ex:o2], ex:o3]>; return 0; }";
        var (ast, diags) = Parse(code);
        ast.Should().NotBeNull();
        diags.Should().NotBeEmpty();
        // TODO: Assert contains TRPL006 once diagnostic hooking available.
    }
}
