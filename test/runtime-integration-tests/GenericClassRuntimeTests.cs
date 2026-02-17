using ast;
using ast_model.TypeSystem;
using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// End-to-end runtime tests for generic class functionality (T019).
/// User Story 1: Generic Collection Classes - Runtime validation.
/// </summary>
public class GenericClassRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task Generic_Stack_Class_Parses_Successfully()
    {
        // Arrange
        var fifthCode = """
            class Stack<T> {
                items: [T];
                Stack() { }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Generic class should compile to executable");

        // Execute to verify it runs
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Should return 0 as specified");
    }

    [Fact]
    public async Task Generic_Class_With_Multiple_Type_Parameters_Parses()
    {
        // Arrange
        var fifthCode = """
            class Dictionary<TKey, TValue> {
                keys: [TKey];
                values: [TValue];
                Dictionary() { }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Multi-parameter generic class should compile");

        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task Generic_Function_With_Type_Parameter_Parses()
    {
        // Arrange
        var fifthCode = """
            identity<T>(x: T): T {
                return x;
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Generic function should compile");

        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task Non_Generic_Class_Still_Works()
    {
        // Arrange
        var fifthCode = """
            class SimpleClass {
                value: int;
                SimpleClass() { value = 0; }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Non-generic classes should still work");

        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }
}
