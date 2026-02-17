using System.Linq;
using FluentAssertions;
using Xunit;
using compiler;
using Fifth;

namespace ast_tests;

public class TripleParserTests
{
    [Fact]
    public void P001_ParserRecognizes_EmptyList_TripleLiteral()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { <ex:s, ex:p, []>; return 0; }";
        var (parser, tree) = FifthParserManager.ParseStringToTree(code);
        // Walk parse tree to find any Triple_literalContext nodes
        var found = FindAnyTripleLiteral(tree);
        found.Should().BeTrue("parser should produce a tripleLiteral parse node for empty-list object");
    }

    private static bool FindAnyTripleLiteral(Antlr4.Runtime.Tree.IParseTree node)
    {
        if (node == null) return false;
        if (node.GetType().Name == "Triple_literalContext") return true;
        for (int i = 0; i < node.ChildCount; i++)
        {
            if (FindAnyTripleLiteral(node.GetChild(i))) return true;
        }
        return false;
    }
}
