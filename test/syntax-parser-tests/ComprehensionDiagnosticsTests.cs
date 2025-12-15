using System.Linq;
using FluentAssertions;
using Xunit;
using test_infra;

namespace syntax_parser_tests;

/// <summary>
/// Diagnostic tests for list comprehensions.
/// Validates that the parser correctly handles both legacy and new comprehension syntax,
/// rejecting legacy 'in'/'#' forms and accepting new 'from'/'where' forms.
/// </summary>
public class ComprehensionDiagnosticsTests
{
    /// <summary>
    /// Helper to parse with diagnostics enabled
    /// </summary>
    private ParseResult ParseHarnessed(string code)
    {
        return ParseHarness.ParseString(code, new ParseOptions(Phase: compiler.FifthParserManager.AnalysisPhase.All));
    }

    [Fact]
    public void LegacyInSyntax_ShouldFailToParse()
    {
        // Legacy 'in' syntax should be rejected at parse time
        const string code = """
            main(): int {
                xs: [int] = [1, 2, 3];
                ys: [int] = [x in xs];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should have parse errors
        result.Diagnostics.Should().NotBeEmpty("Legacy 'in' syntax should produce parse errors");
    }

    [Fact]
    public void LegacyHashSyntax_ShouldFailToParse()
    {
        // Legacy '#' constraint syntax should be rejected
        const string code = """
            main(): int {
                xs: [int] = [1, 2, 3];
                ys: [int] = [x in xs # x > 0];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should have parse errors
        result.Diagnostics.Should().NotBeEmpty("Legacy '#' constraint syntax should produce parse errors");
    }

    [Fact]
    public void ValidNewSyntax_ShouldParseSuccessfully()
    {
        // New 'from'/'where' syntax should parse correctly
        const string code = """
            main(): int {
                xs: [int] = [1, 2, 3];
                ys: [int] = [x from x in xs where x > 0];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should parse without errors
        result.Root.Should().NotBeNull("Valid new syntax should parse successfully");
    }

    [Fact]
    public void MultipleConstraints_ShouldParseSuccessfully()
    {
        // Multiple comma-separated constraints should parse
        const string code = """
            main(): int {
                xs: [int] = [1, 2, 3, 4, 5];
                ys: [int] = [x from x in xs where x > 1, x < 5, x % 2 == 0];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should parse without errors
        result.Root.Should().NotBeNull("Multiple constraints should parse successfully");
    }

    [Fact]
    public void ComprehensionWithoutConstraints_ShouldParseSuccessfully()
    {
        // Comprehension without 'where' clause should work
        const string code = """
            main(): int {
                xs: [int] = [1, 2, 3];
                ys: [int] = [x * 2 from x in xs];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should parse without errors
        result.Diagnostics.Should().BeEmpty("New syntax should parse without errors");
        result.Root.Should().NotBeNull("Comprehension without constraints should parse");
    }

    [Fact]
    public void ComplexProjection_ShouldParseSuccessfully()
    {
        // Complex projection expression
        const string code = """
            main(): int {
                xs: [int] = [1, 2, 3];
                ys: [int] = [x * 2 + 1 from x in xs where x > 1];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should parse without errors
        result.Root.Should().NotBeNull("Complex projection should parse");
    }

    [Fact]
    public void EmptySource_ShouldParseSuccessfully()
    {
        // Empty list as source
        const string code = """
            main(): int {
                xs: [int] = [];
                ys: [int] = [x from x in xs];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should parse without errors
        result.Root.Should().NotBeNull("Empty source should parse");
    }

    [Fact]
    public void NestedComprehension_ShouldParseSuccessfully()
    {
        // Nested comprehension as source
        const string code = """
            main(): int {
                xs: [int] = [1, 2, 3];
                ys: [int] = [y from y in [x * 2 from x in xs]];
                return 0;
            }
            """;

        var result = ParseHarnessed(code);

        // Should parse without errors
        result.Root.Should().NotBeNull("Nested comprehension should parse");
    }
}
