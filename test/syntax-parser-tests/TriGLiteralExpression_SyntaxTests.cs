using FluentAssertions;

namespace syntax_parser_tests;

/// <summary>
/// Tests for TriG Literal Expression syntax (@&lt; ... &gt;).
/// Following TDD - these tests will initially fail until the feature is implemented.
/// </summary>
public class TriGLiteralExpression_SyntaxTests
{
    /// <summary>
    /// Helper to parse Fifth source code using the parser manager
    /// </summary>
    private static void ParseSource(string source)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, source);
            compiler.FifthParserManager.ParseFileSyntaxOnly(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void BasicTriGLiteral_ShouldParse()
    {
        // Arrange - basic TriG literal without interpolation
        var source = @"
main(): int {
    s: Store = @<
        @prefix ex: <http://example.org/> .
        
        ex:graph1 {
            ex:Andrew ex:name ""Andrew"" .
        }
    >;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert - This will fail until we implement the feature
        act.Should().NotThrow("Valid TriG literal should parse successfully");
    }

    [Test]
    public void TriGLiteralWithNestedIRIs_ShouldNotTerminatePrematurely()
    {
        // Arrange - TriG literal with multiple nested <...> IRIs
        var source = @"
main(): int {
    s: Store = @<
        <http://example.org/graph1> {
            <http://example.org/s1> <http://example.org/p1> <http://example.org/o1> .
        }
    >;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert - Nested angle brackets should not terminate the literal
        act.Should().NotThrow("Nested IRIs should not prematurely terminate the literal");
    }

    [Test]
    public void TriGLiteralWithInterpolation_ShouldParse()
    {
        // Arrange - TriG literal with expression interpolation
        var source = @"
main(): int {
    age: int = 42;
    s: Store = @<
        @prefix ex: <http://example.org/> .
        
        ex:graph1 {
            ex:Person ex:age {{ age }} .
        }
    >;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert
        act.Should().NotThrow("TriG literal with interpolation should parse successfully");
    }

    [Test]
    public void TriGLiteralWithMultipleInterpolations_ShouldParse()
    {
        // Arrange - Multiple interpolations in one literal
        var source = @"
main(): int {
    name: string = ""Test"";
    age: int = 30;
    active: bool = true;
    s: Store = @<
        @prefix ex: <http://example.org/> .
        
        ex:graph1 {
            ex:Person ex:name {{ name }};
                     ex:age {{ age }};
                     ex:active {{ active }} .
        }
    >;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert
        act.Should().NotThrow("TriG literal with multiple interpolations should parse");
    }

    [Test]
    public void TriGLiteralWithBraceEscaping_ShouldParse()
    {
        // Arrange - Escaped braces using triple braces
        var source = @"
main(): int {
    s: Store = @<
        @prefix ex: <http://example.org/> .
        
        ex:graph1 {
            ex:Item ex:text ""Use {{{ and }}} for literal braces"" .
        }
    >;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert
        act.Should().NotThrow("Escaped braces should parse correctly");
    }

    [Test]
    public void EmptyTriGLiteral_ShouldParse()
    {
        // Arrange - Empty TriG literal (edge case)
        var source = @"
main(): int {
    s: Store = @<>;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert
        act.Should().NotThrow("Empty TriG literal should be valid");
    }

    [Test]
    public void TriGLiteralPreservesWhitespace_ShouldParse()
    {
        // Arrange - Check that whitespace is preserved
        var source = @"
main(): int {
    s: Store = @<
@prefix ex: <http://example.org/> .

        ex:graph1 {
            ex:Item ex:value ""test"" .
        }
    >;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert
        act.Should().NotThrow("Whitespace preservation should not affect parsing");
    }

    [Test]
    public void UnbalancedTriGLiteral_ShouldFailParsing()
    {
        // Arrange - Missing closing >
        var source = @"
main(): int {
    s: Store = @<
        @prefix ex: <http://example.org/> .
        ex:graph1 { ex:Item ex:value ""test"" . }
    ;
    return 0;
}";

        // Act
        var act = () => ParseSource(source);

        // Assert - This should throw because the literal is not properly terminated
        act.Should().Throw<Exception>("Unbalanced TriG literal should fail parsing");
    }
}
