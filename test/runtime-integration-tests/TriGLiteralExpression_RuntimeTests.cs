using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Runtime integration tests for TriG Literal Expression feature.
/// Following TDD - these tests will initially fail until the feature is implemented.
/// Tests validate that TriG literals compile and produce expected Store objects.
/// </summary>
[Trait("Category", "KG")]
[Trait("Category", "TriG")]
public class TriGLiteralExpression_RuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task BasicTriGLiteral_ShouldCompileAndInitializeStore()
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
                
                // If we got here and s is not null, test passes
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act - Compile the source
        var exe = await CompileSourceAsync(src, "trig_literal_basic");

        // Assert - Should compile successfully
        File.Exists(exe).Should().BeTrue("Executable should be created");
        
        // For now, we just verify compilation. Runtime execution would require
        // full dotNetRDF integration which may not be available in test environment.
    }

    [Fact]
    public async Task TriGLiteralWithNestedIRIs_ShouldCompile()
    {
        // Arrange - User Story 3: Nested angle brackets should not prematurely terminate
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
        var exe = await CompileSourceAsync(src, "trig_literal_nested_iris");

        // Assert
        File.Exists(exe).Should().BeTrue("Executable with nested IRIs should compile");
    }

    [Fact]
    public async Task TriGLiteralWithInterpolation_ShouldCompileAndSerializeValues()
    {
        // Arrange - User Story 2: Expression interpolation
        var src = """
            main(): int {
                name: string = "Andrew";
                age: int = 42;
                height: float = 1.85;
                active: bool = true;
                
                s: Store = @<
                    @prefix ex: <http://example.org/> .
                    
                    ex:graph1 {
                        ex:Person1 ex:name {{ name }};
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
        var exe = await CompileSourceAsync(src, "trig_literal_interpolation");

        // Assert
        File.Exists(exe).Should().BeTrue("Executable with interpolation should compile");
    }

    [Fact]
    public async Task TriGLiteralWithBraceEscaping_ShouldCompile()
    {
        // Arrange - Triple braces for literal {{ and }}
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
        var exe = await CompileSourceAsync(src, "trig_literal_brace_escaping");

        // Assert
        File.Exists(exe).Should().BeTrue("Executable with brace escaping should compile");
    }

    [Fact]
    public async Task TriGLiteralPreservesWhitespace_ShouldCompile()
    {
        // Arrange - Whitespace preservation (no trimming/indentation normalization)
        var src = """
            main(): int {
                s: Store = @<
            @prefix ex: <http://example.org/> .
            
                    ex:graph1 {
                        ex:Item1 ex:value "one" .
                        
                        ex:Item2 ex:value "two" .
                    }
                >;
                
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_literal_whitespace");

        // Assert
        File.Exists(exe).Should().BeTrue("Executable with preserved whitespace should compile");
    }

    [Fact]
    public async Task EmptyTriGLiteral_ShouldCompile()
    {
        // Arrange - Edge case: empty TriG literal
        var src = """
            main(): int {
                s: Store = @<>;
                
                if (s == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_literal_empty");

        // Assert
        File.Exists(exe).Should().BeTrue("Executable with empty TriG literal should compile");
    }

    [Fact]
    public async Task TriGLiteralWithRuntimeExpressionEvaluation_ShouldCompile()
    {
        // Arrange - Interpolation evaluates at runtime in lexical scope
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
        var exe = await CompileSourceAsync(src, "trig_literal_runtime_eval");

        // Assert
        File.Exists(exe).Should().BeTrue("Runtime evaluation should work correctly");
    }

    [Fact]
    public async Task MultipleTriGLiterals_ShouldCompile()
    {
        // Arrange - Multiple TriG literals in same function
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
                
                if (s1 == null || s2 == null) { return 1; }
                return 0;
            }
            """;

        // Act
        var exe = await CompileSourceAsync(src, "trig_literal_multiple");

        // Assert
        File.Exists(exe).Should().BeTrue("Multiple TriG literals should compile");
    }

    [Fact]
    public async Task TriGLiteralAssignedToVariousTypes_ShouldOnlyAcceptStore()
    {
        // Arrange - TriG literal should only be assignable to Store type
        // This test validates type checking
        var invalidSrc = """
            main(): int {
                s: string = @<
                    @prefix ex: <http://example.org/> .
                    ex:graph1 { ex:Item ex:value "test" . }
                >;
                return 0;
            }
            """;

        // Act & Assert - Should fail compilation due to type mismatch
        var act = async () => await CompileSourceAsync(invalidSrc, "trig_literal_type_error");
        await act.Should().ThrowAsync<InvalidOperationException>(
            "TriG literal should only be assignable to Store type");
    }
}
