using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// End-to-end runtime tests for multiple type parameters (T079, Phase 7 - User Story 5).
/// Verifies that multiple type parameters compile and execute correctly.
/// </summary>
public class MultipleTypeParamRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task Class_With_Two_Type_Parameters_Compiles()
    {
        // Arrange
        var fifthCode = """
            class Pair<T1, T2> {
                first: T1;
                second: T2;
                Pair() { }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Class with two type parameters should compile");

        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task Function_With_Three_Type_Parameters_Compiles()
    {
        // Arrange
        var fifthCode = """
            triple<T1, T2, T3>(a: T1, b: T2, c: T3): int {
                return 0;
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Function with three type parameters should compile");

        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task Dictionary_With_Key_Value_Type_Parameters_Compiles()
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
        File.Exists(executablePath).Should().BeTrue("Dictionary with key-value type parameters should compile");

        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }
}
