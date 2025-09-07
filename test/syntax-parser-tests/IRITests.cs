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
    [Test]
    public void Iri_WithAngleBrackets_ShouldParse()
    {
        // Arrange
        var input = "<http://example.org/Person>";
        
        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.iri(), 
            "Angle-bracketed IRI should parse successfully");
    }

    [Test]
    public void Alias_WithAngleBracketedIri_ShouldParse()
    {
        // Arrange
        var input = "alias p as <x:Person>;";
        
        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.alias(), 
            "Alias with angle-bracketed IRI should parse successfully");
    }

    [Test]
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

    [Test]
    public void PropertyDeclarations_ShouldNotContainPrefixedNameTokens()
    {
        // Arrange
        var input = "Name: string; Height: float;";
        
        // Act & Assert
        ParserTestUtils.AssertDoesNotContainTokens(input, "PrefixedName", "PNAME_NS", "PNAME_LN");
        ParserTestUtils.AssertContainsTokens(input, "IDENTIFIER", "COLON");
    }

    [Test]
    public void GraphDeclaration_WithAngleBracketedIri_ShouldParse()
    {
        // Arrange
        var input = "graph G in <http://example.org/Person> = { };";
        
        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.graphDeclaration(), 
            "Graph declaration with angle-bracketed IRI should parse successfully");
    }

    [Test]
    public void StoreDeclaration_WithAngleBracketedIri_ShouldParse()
    {
        // Arrange
        var input = "store S = sparql_store(<http://example.org/>);";
        
        // Act & Assert
        ParserTestUtils.AssertNoErrors(input, p => p.store_decl(), 
            "Store declaration with angle-bracketed IRI should parse successfully");
    }

    [Test]
    public void Alias_WithUnbracketedPrefixedName_ShouldFail()
    {
        // Arrange - This should fail because x:Person is not in angle brackets
        var input = "alias p as x:Person;";
        
        // Act & Assert
        ParserTestUtils.AssertHasErrors(input, p => p.alias(), 
            "Alias with unbracketed prefixed name should fail parsing");
    }

    [Test]
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

    [Test]
    public void TokenStream_GeneratorOperator_ShouldTokenizeCorrectly()
    {
        // Arrange
        var input = "<-";
        
        // Act & Assert
        ParserTestUtils.AssertContainsTokens(input, "GEN");
        ParserTestUtils.AssertDoesNotContainTokens(input, "IRIREF");
    }

    [Test]
    public void TokenStream_ConcatOperator_ShouldTokenizeCorrectly()
    {
        // Arrange
        var input = "<>";
        
        // Act & Assert
        ParserTestUtils.AssertContainsTokens(input, "CONCAT");
        ParserTestUtils.AssertDoesNotContainTokens(input, "IRIREF");
    }

    [Test]
    public void TokenStream_GraphOperator_ShouldTokenizeCorrectly()
    {
        // Arrange
        var input = "<{";
        
        // Act & Assert
        ParserTestUtils.AssertContainsTokens(input, "L_GRAPH");
        ParserTestUtils.AssertDoesNotContainTokens(input, "IRIREF");
    }

    [Test]
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

    [Test]
    public void Iri_WithValidContent_ShouldFormIrirefToken()
    {
        // Arrange
        var input = "<http://example.org>";
        
        // Act & Assert
        ParserTestUtils.AssertContainsTokens(input, "IRIREF");
    }

    [Test]
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

    [Test]
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

    [Test]
    public void DebugTokenization_PropertyVsIRI()
    {
        // This test helps debug tokenization differences
        var propertyInput = "Name: string;";
        var iriInput = "<x:Person>";
        
        Console.WriteLine("Property declaration tokens:");
        Console.WriteLine(ParserTestUtils.PrintTokens(propertyInput));
        
        Console.WriteLine("\nIRI tokens:");
        Console.WriteLine(ParserTestUtils.PrintTokens(iriInput));
        
        // Should always pass - this is just for debugging output
        true.Should().BeTrue();
    }
}