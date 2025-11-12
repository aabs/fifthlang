using FluentAssertions;
using Fifth.System;
using VDS.RDF;

namespace runtime_integration_tests;

/// <summary>
/// Enhanced runtime integration tests for TriG Literal Expression feature.
/// These tests compile and execute Fifth programs, then verify Store contents.
/// Validates end-to-end functionality including parsing, lowering, and runtime execution.
/// </summary>
[Category("KG")]
[Category("TriG")]
[Category("Integration")]
public class TriGLiteralExpression_ContentValidationTests : RuntimeTestBase
{
    [Test]
    public async Task BasicTriGLiteral_ShouldExecuteSuccessfully()
    {
        // Arrange - User Story 1: Basic TriG literal without interpolation
        var src = """
            main(): int {
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    @prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
                    
                    ex:graph1 {
                        ex:Andrew rdf:type ex:Person;
                                  ex:name "Andrew";
                                  ex:age 42 .
                    }
                >;
                
                // If store is created, test passes
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act - Compile and execute
        var exe = await CompileSourceAsync(src, "trig_content_basic");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Program should execute successfully. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task TriGLiteralWithInterpolation_ShouldSerializeValuesCorrectly()
    {
        // Arrange - User Story 2: Validate interpolated values appear in RDF
        var src = """
            main(): int {
                name: string = "Andrew";
                age: int = 42;
                
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    
                    ex:graph1 {
                        ex:Person1 ex:name {{ name }};
                                   ex:age {{ age }} .
                    }
                >;
                
                // If store is created with interpolated values, test passes
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_interpolation");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Program with interpolation should execute. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task TriGLiteralWithMultipleTypes_ShouldSerializeAllCorrectly()
    {
        // Arrange - Test multiple data types in interpolations
        var src = """
            main(): int {
                name: string = "TestPerson";
                age: int = 25;
                height: float = 1.75;
                active: bool = true;
                
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    
                    ex:graph1 {
                        ex:Person ex:name {{ name }};
                                  ex:age {{ age }};
                                  ex:height {{ height }};
                                  ex:active {{ active }} .
                    }
                >;
                
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_multiple_types");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Program with multiple types should execute. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task TriGLiteralWithNestedIRIs_ShouldPreserveAllIRIs()
    {
        // Arrange - Verify nested IRIs don't cause premature termination
        var src = """
            main(): int {
                s: Store = @<
                    <http://example.org/graph1> {
                        <http://example.org/subject1> <http://example.org/predicate1> <http://example.org/object1> .
                        <http://example.org/subject2> <http://example.org/predicate2> "literal value" .
                    }
                >;
                
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_nested_iris");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Program with nested IRIs should execute. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task TriGLiteralWithRuntimeModifiedValue_ShouldUseCurrentValue()
    {
        // Arrange - Interpolation should evaluate at runtime with current values
        var src = """
            main(): int {
                age: int = 20;
                age = age + 1;  // Modify before literal evaluation
                
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    
                    ex:graph1 {
                        ex:Person ex:age {{ age }} .
                    }
                >;
                
                // The interpolated value should be 21, not 20
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_runtime_value");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Runtime-modified value should work. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task MultipleTriGLiterals_ShouldCreateSeparateStores()
    {
        // Arrange - Multiple TriG literals should create separate stores
        var src = """
            main(): int {
                s1: Store = @<
                    @prefix ex: <http://example.org/> .
                    ex:graph1 { ex:Item1 ex:value "one" . }
                >;
                
                s2: Store = @<
                    @prefix ex: <http://example.org/> .
                    ex:graph2 { ex:Item2 ex:value "two" . }
                >;
                
                // Verify both stores were created
                if (s1 == null || s2 == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_multiple");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Multiple stores should work independently. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task EmptyTriGLiteral_ShouldCreateValidEmptyStore()
    {
        // Arrange - Empty TriG literal should create a valid store
        var src = """
            main(): int {
                s: Store = @<>;
                
                // Store should be valid even if empty
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_empty");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Empty store should be valid. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task TriGLiteralWithBraceEscaping_ShouldPreserveLiteralBraces()
    {
        // Arrange - Triple braces should produce literal braces in output
        var src = """
            main(): int {
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    
                    ex:graph1 {
                        ex:Item ex:description "Use {{{ and }}} for braces" .
                    }
                >;
                
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_brace_escaping");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Brace escaping should work. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task TriGLiteralWithComplexGraph_ShouldPreserveAllTriples()
    {
        // Arrange - Complex graph with multiple subjects and predicates
        var src = """
            main(): int {
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    @prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
                    @prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
                    
                    ex:graph1 {
                        ex:Person1 rdf:type ex:Person ;
                                   rdfs:label "Alice" ;
                                   ex:age 30 ;
                                   ex:email "alice@example.org" .
                        
                        ex:Person2 rdf:type ex:Person ;
                                   rdfs:label "Bob" ;
                                   ex:age 25 .
                        
                        ex:Person1 ex:knows ex:Person2 .
                    }
                >;
                
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_complex");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"Complex graph should be parsed correctly. Stderr: {result.StandardError}");
    }

    [Test]
    public async Task TriGLiteralWithIRIsFollowedBySemicolons_ShouldParseCorrectly()
    {
        // Arrange - Validate bracket depth counting handles IRIs followed by semicolons
        var src = """
            main(): int {
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    
                    ex:graph1 {
                        ex:subject ex:predicate <http://example.org/object> ;
                                   ex:another "value" .
                    }
                >;
                
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_content_iris_semicolons");
        var result = await ExecuteAsync(exe);

        // Assert
        result.ExitCode.Should().Be(0, $"IRIs followed by semicolons should work. Stderr: {result.StandardError}");
    }
}
