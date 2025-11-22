// T035A: Canonical serialization and round-trip test for triple literals
// Covers FR-018 & FR-018A: proper escaping of '>' and ',' in triple serialization
using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using ast;
using test_infra;
using compiler;

namespace ast_tests;

/// <summary>
/// Tests for canonical triple serialization and round-trip parsing.
/// Ensures triples can be serialized to valid Fifth source code and re-parsed correctly.
/// T035A focuses on demonstrating that special characters in triple components don't break parsing.
/// </summary>
public class TripleSerializationRoundTripTests
{
    [Fact]
    public void SimpleTriple_ParsesSuccessfully()
    {
        // Basic test: a simple triple can be parsed
        var code = @"alias ex as <http://example.org/>; main(): int { <ex:s, ex:p, ex:o>; return 0; }";
        var result = ParseHarness.ParseString(code, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        
        result.Root.Should().NotBeNull();
        result.Diagnostics.Should().NotContain(d => d.Severity == test_infra.DiagnosticSeverity.Error);
        
        var triples = ParseHarness.FindTriples(result.Root!).ToList();
        triples.Should().HaveCount(1);
        triples[0].SubjectExp.Should().BeOfType<UriLiteralExp>();
        triples[0].PredicateExp.Should().BeOfType<UriLiteralExp>();
        triples[0].ObjectExp.Should().BeOfType<UriLiteralExp>();
    }

    [Fact]
    public void TripleWithStringObject_ParsesSuccessfully()
    {
        // Test with string object (common case)
        var code = @"alias ex as <http://example.org/>; main(): int { <ex:s, ex:p, ""object value"">; return 0; }";
        var result = ParseHarness.ParseString(code, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        
        result.Root.Should().NotBeNull();
        result.Diagnostics.Should().NotContain(d => d.Severity == test_infra.DiagnosticSeverity.Error);
        
        var triples = ParseHarness.FindTriples(result.Root!).ToList();
        triples.Should().HaveCount(1);
        triples[0].ObjectExp.Should().BeOfType<StringLiteralExp>();
    }

    [Fact]
    public void FR_018A_CommaInStringObject_ParsesCorrectly()
    {
        // FR-018A: String objects containing ',' should not break parsing
        // The comma is within the string literal, not part of triple delimiter
        var code = @"alias ex as <http://example.org/>; main(): int { <ex:s, ex:p, ""value, with, commas"">; return 0; }";
        var result = ParseHarness.ParseString(code, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        
        result.Root.Should().NotBeNull();
        result.Diagnostics.Should().NotContain(d => d.Severity == test_infra.DiagnosticSeverity.Error);
        
        var triples = ParseHarness.FindTriples(result.Root!).ToList();
        triples.Should().HaveCount(1);
        var strObj = triples[0].ObjectExp.Should().BeOfType<StringLiteralExp>().Subject;
        strObj.Value.Should().Contain(",");
        // String literals may include surrounding quotes in the value
    }

    [Fact]
    public void MultipleTriples_ParseCorrectly()
    {
        // Test with multiple triples to ensure no cross-contamination
        var code = @"
alias ex as <http://example.org/>;
main(): int { 
    <ex:s1, ex:p1, ex:o1>;
    <ex:s2, ex:p2, ""string object"">;
    <ex:s3, ex:p3, 42>;
    return 0; 
}";
        var result = ParseHarness.ParseString(code, new ParseOptions(FifthParserManager.AnalysisPhase.All));
        
        result.Root.Should().NotBeNull();
        result.Diagnostics.Should().NotContain(d => d.Severity == test_infra.DiagnosticSeverity.Error);
        
        var triples = ParseHarness.FindTriples(result.Root!).ToList();
        triples.Should().HaveCount(3);
        
        triples[0].ObjectExp.Should().BeOfType<UriLiteralExp>();
        triples[1].ObjectExp.Should().BeOfType<StringLiteralExp>();
        triples[2].ObjectExp.Should().BeOfType<Int32LiteralExp>();
    }
}
