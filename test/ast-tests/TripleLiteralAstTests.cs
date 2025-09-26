// T009: AST tests for triple literal (expected to fail before implementation)
using System.Linq;
using FluentAssertions;
using Xunit;
// Assuming existing helper namespaces (adjust if different in project):
using parser; // if there's a parser namespace
using ast;    // core AST model

namespace ast_tests;

public class TripleLiteralAstTests
{
    private (object ast, string diagnostics) Parse(string code)
    {
        // TODO: Replace with real test harness parse method used in other AST tests.
        // Intentionally throwing to ensure we wire correct harness later.
        throw new System.NotImplementedException("Wire actual parser invocation for triple literal tests (T009).");
    }

    [Fact(DisplayName = "T009_01 Simple triple literal produces TripleLiteralExp node")]
    public void SimpleTripleLiteral()
    {
        const string code = @"alias ex as <http://example.org/>;\nmain(): int { t: triple = <ex:s, ex:p, ex:o>; return 0; }";
        var (ast, diags) = Parse(code);
        // Assertions (pseudo until real AST types known):
        // 1. Find one TripleLiteralExp node
        // 2. Subject & Predicate are IRIRefExp with prefixed name tokens
        // 3. Object is IRIRefExp
        ast.Should().NotBeNull();
        diags.Should().BeEmpty("no diagnostics expected for a valid triple literal");
    }

    [Fact(DisplayName = "T009_02 Variable subject/predicate accepted when IRI-typed")]
    public void VariableSubjectPredicate()
    {
        const string code = @"alias ex as <http://example.org/>;\nmain(): int { s: iri = ex:s; p: iri = ex:p; t: triple = <s, p, ex:o>; return 0; }";
        var (ast, diags) = Parse(code);
        ast.Should().NotBeNull();
        diags.Should().BeEmpty("variable subject/predicate with IRI type should be valid");
    }
}
