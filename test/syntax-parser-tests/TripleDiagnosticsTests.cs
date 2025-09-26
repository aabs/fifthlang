// T011: Diagnostics skeleton for triple literals (will initially fail until grammar & diagnostics implemented)
using System.Linq;
using FluentAssertions;
using TUnit;

namespace syntax_parser_tests;

public class TripleDiagnosticsTests
{
    private (object ast, string[] diagnostics) Parse(string code)
    {
        throw new System.NotImplementedException("Wire actual parser diagnostic capture for T011.");
    }

    [Test]
    public void T011_01_Arity_Too_Few_TRPL001()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { a: triple = <ex:s, ex:p>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL001"));
    }

    [Test]
    public void T011_02_Arity_Too_Many_TRPL001()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { b: triple = <ex:s, ex:p, ex:o, ex:x>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL001"));
    }

    [Test]
    public void T011_03_Trailing_Comma_TRPL001()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { c: triple = <ex:s, ex:p, ex:o,>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL001"));
    }

    [Test]
    public void T011_04_Nested_List_TRPL006()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, [[ex:o1, ex:o2], ex:o3]>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL006"));
    }

    [Test]
    public void T011_05_Empty_List_TRPL004_Warning()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { g: graph = <ex:s, ex:p, []>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("TRPL004"));
    }

    [Test]
    public void T011_06_Unresolved_Prefix_Diagnostic()
    {
        const string code = "alias ex as <http://example.org/>;\nmain(): int { t: triple = <ux:s, ex:p, ex:o>; return 0; }";
        var (_, diags) = Parse(code);
        diags.Should().Contain(d => d.Contains("unresolved")); // Replace with specific code when known
    }
}
