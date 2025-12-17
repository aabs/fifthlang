using FluentAssertions;
using ast;
using compiler;

namespace syntax_parser_tests;

/// <summary>
/// Tests for the new list comprehension syntax with from/where keywords.
/// Tests both variable projection and object projection forms.
/// </summary>
public class ListComprehensionSyntaxTests
{
    [Fact]
    public void SimpleComprehension_WithVariableProjection_ShouldParse()
    {
        var code = """
            main(): int {
                numbers: [int] = [1, 2, 3];
                doubled: [int] = [x * 2 from x in numbers];
                return 0;
            }
            """;

        var act = () => FifthParserManager.ParseString(code);
        act.Should().NotThrow("Simple variable projection comprehension should parse");
        
        var ast = FifthParserManager.ParseString(code);
        ast.Should().NotBeNull();
    }

    [Fact]
    public void Comprehension_WithSingleWhereConstraint_ShouldParse()
    {
        var code = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                evens: [int] = [x from x in numbers where x % 2 == 0];
                return 0;
            }
            """;

        var act = () => FifthParserManager.ParseString(code);
        act.Should().NotThrow("Comprehension with single where constraint should parse");
    }

    [Fact]
    public void Comprehension_WithMultipleWhereConstraints_ShouldParse()
    {
        var code = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                filtered: [int] = [x from x in numbers where x > 2, x < 5];
                return 0;
            }
            """;

        var act = () => FifthParserManager.ParseString(code);
        act.Should().NotThrow("Comprehension with multiple where constraints should parse");
    }

    [Fact]
    public void Comprehension_WithObjectProjection_ShouldParse()
    {
        var code = """
            main(): int {
                numbers: [int] = [1, 2, 3];
                doubled: [int] = [x + x from x in numbers];
                return 0;
            }
            """;

        var act = () => FifthParserManager.ParseString(code);
        act.Should().NotThrow("Comprehension with expression projection should parse");
    }

    [Fact]
    public void Comprehension_WithComplexProjection_ShouldParse()
    {
        var code = """
            main(): int {
                numbers: [int] = [1, 2, 3];
                results: [int] = [x * 2 + 1 from x in numbers where x > 1];
                return 0;
            }
            """;

        var act = () => FifthParserManager.ParseString(code);
        act.Should().NotThrow("Comprehension with complex projection expression should parse");
    }
}
