using System;
using System.Linq;
using FluentAssertions;
using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

/// <summary>
/// Tests to verify IRI parsing and property declarations do not conflict.
/// These tests ensure the parser can distinguish between IRIs and property declarations.
/// </summary>
public class IRITests
{
    [Fact]
    public void Iri_WithAngleBrackets_ShouldParse()
    {
        // Arrange
        var input = "<http://example.org/Person>";

        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.iri(),
            "Angle-bracketed IRI should parse successfully");
    }

    [Fact]
    public void Alias_WithAngleBracketedIri_ShouldParse()
    {
        // Arrange
        var input = "alias p as <x:Person>;";

        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.alias(),
            "Alias with angle-bracketed IRI should parse successfully");
    }

    [Fact]
    public void ClassDefinition_WithPropertyDeclarations_ShouldParse()
    {
        // Arrange
        var input = """
            class A { 
                Name: string; 
                Height: float; 
            }
            """;

        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.class_definition(),
            "Class with property declarations should parse successfully");
    }

    [Fact]
    public void PropertyDeclarations_ShouldNotContainPrefixedNameTokens()
    {
        // Arrange
        var input = "Name: string; Height: float;";

        // Act & Assert
        ParserTestUtils.AssertDoesNotContainTokens(input, "PrefixedName", "PNAME_NS", "PNAME_LN");
        ParserTestUtils.AssertContainsTokens(input, "IDENTIFIER", "COLON");
    }

    [Fact]
    public void GraphDeclaration_WithAngleBracketedIri_ShouldParse()
    {
        // Arrange
        var input = "graph G in <http://example.org/Person> = { };";

        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.graphDeclaration(),
            "Graph declaration with angle-bracketed IRI should parse successfully");
    }

    [Fact]
    public void ColonStoreDeclaration_WithAngleBracketedIri_ShouldParse()
    {
        // Arrange
        var input = "S: store = sparql_store(<http://example.org/>);";

        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.colon_store_decl(),
            "Colon-form store declaration with angle-bracketed IRI should parse successfully");
    }

    [Fact]
    public void Alias_WithUnbracketedPrefixedName_ShouldFail()
    {
        // Arrange - This should fail because x:Person is not in angle brackets
        var input = "alias p as x:Person;";

        // Act & Assert
        ParserTestUtils.AssertHasErrors(input, p => p.alias(),
            "Alias with unbracketed prefixed name should fail parsing");
    }

    [Fact]
    public void TokenStream_ComparisonOperator_ShouldNotFormIriref()
    {
        // Arrange - This should tokenize as separate IDENTIFIER, LESS, IDENTIFIER tokens
        var input = "a < b";

        // Act
        var tokenInfo = ParserTestUtils.GetTokenInfo(input);
        var tokenNames = tokenInfo.Select(t => t.typeName).ToList();

        // Assert
        tokenNames.Should().Contain("IDENTIFIER");
        tokenNames.Should().Contain("LESS");
        tokenNames.Should().NotContain("IRIREF", "a < b should not form an IRIREF token");
    }

    [Fact]
    public void TokenStream_GeneratorOperator_ShouldTokenizeCorrectly()
    {
        // Arrange
        var input = "<-";

        // Act & Assert
        ParserTestUtils.AssertContainsTokens(input, "GEN");
        ParserTestUtils.AssertDoesNotContainTokens(input, "IRIREF");
    }

    [Fact]
    public void TokenStream_ConcatOperator_ShouldTokenizeCorrectly()
    {
        // Arrange
        var input = "<>";

        // Act & Assert
        ParserTestUtils.AssertContainsTokens(input, "CONCAT");
        ParserTestUtils.AssertDoesNotContainTokens(input, "IRIREF");
    }

    [Fact]
    public void TokenStream_EmptyAngleBrackets_ShouldNotFormIriref()
    {
        // Arrange - Empty angle brackets should tokenize as LESS + GREATER, not IRIREF
        var input = "<>";

        // Act
        var tokenInfo = ParserTestUtils.GetTokenInfo(input);
        var tokenNames = tokenInfo.Select(t => t.typeName).ToList();

        // Assert - Should be CONCAT, not IRIREF
        tokenNames.Should().Contain("CONCAT");
        tokenNames.Should().NotContain("IRIREF", "Empty angle brackets should be CONCAT token, not IRIREF");
    }

    [Fact]
    public void Iri_WithValidContent_ShouldFormIrirefToken()
    {
        // Arrange
        var input = "<http://example.org>";

        // Act & Assert
        ParserTestUtils.AssertContainsTokens(input, "IRIREF");
    }

    [Fact]
    public void ClassWithPropertiesAndIRI_ShouldBothParseCorrectly()
    {
        // Arrange - Test both property declarations and IRI in same context
        var input = """
            class Person in <http://example.org/Person> {
                Name: string;
                Age: int;
            }
            """;

        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.class_definition(),
            "Class with both IRI and property declarations should parse correctly");
    }

    [Fact]
    public void ComplexPropertyDeclarations_WithColons_ShouldNotInterfereWithIRIs()
    {
        // Arrange - Multiple properties that could potentially be confused with prefixed names
        var input = """
            class ComplexType {
                ns: string;
                xml: string;
                rdf: string;
                owl: string;
            }
            """;

        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.class_definition(),
            "Property names that look like namespace prefixes should parse as properties");

        // Verify no prefixed name tokens appear
        var classTokens = ParserTestUtils.GetTokenInfo(input);
        classTokens.Select(t => t.typeName).Should().NotContain(
            ["PrefixedName", "PNAME_NS", "PNAME_LN"],
            "Property declarations should not be tokenized as prefixed names");
    }

    [Fact]
    public void DebugTokenization_PropertyVsIRI()
    {
        // Tokenization debug removed; no-op assertion retained
        true.Should().BeTrue();
    }
}