// T011: Diagnostics skeleton for triple literals (will initially fail until grammar & diagnostics implemented)
using System.Linq;
using FluentAssertions;
using Xunit;

namespace syntax_parser_tests;

public class TripleDiagnosticsTests
{
    private (object ast, string[] diagnostics) Parse(string code)
    {
        throw new System.NotImplementedException("Wire actual parser diagnostic capture for T011.");
    }

    [Fact(DisplayName = "T011_01 Arity too few -> TRPL001")]
    public void ArityTooFew()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { a: triple = <ex:s, ex:p>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL001"));
    }

    [Fact(DisplayName = "T011_02 Arity too many -> TRPL001")]
    public void ArityTooMany()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { b: triple = <ex:s, ex:p, ex:o, ex:x>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL001"));
    }

    [Fact(DisplayName = "T011_03 Trailing comma -> TRPL001")]
    public void TrailingComma()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { c: triple = <ex:s, ex:p, ex:o,>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL001"));
    }

    [Fact(DisplayName = "T011_04 Nested list -> TRPL006")]
    public void NestedList()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, [[ex:o1, ex:o2], ex:o3]>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL006"));
    }

    [Fact(DisplayName = "T011_05 Empty list -> TRPL004 warning")]
    public void EmptyListWarning()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, []>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL004"));
    }

    [Fact(DisplayName = "T011_06 Unresolved prefix -> existing unresolved-prefix diagnostic")]
    public void UnresolvedPrefix()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { t: triple = <ux:s, ex:p, ex:o>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("unresolved")); // Replace with specific code when known
    }
}
