using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// End-to-end runtime tests for generic constraints (T064, Phase 6 - User Story 4).
/// Verifies that constraint syntax compiles correctly.
/// NOTE: Tests use simplified syntax due to parser limitations with constraint keywords.
/// </summary>
public class GenericConstraintRuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task Function_With_Constraint_Clause_Compiles()
    {
        // Arrange - Test basic where clause parsing
        var fifthCode = """
            sort<T>(items: int): int where T: IComparable {
                return items;
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Function with constraint clause should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task Class_With_Constraint_Clause_Compiles()
    {
        // Arrange
        var fifthCode = """
            class Factory<T> where T: BaseType {
                value: T;
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Class with constraint clause should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task Function_With_Multiple_Constraints_Compiles()
    {
        // Arrange
        var fifthCode = """
            process<T>(item: T): int where T: IComparable, IDisposable {
                return 0;
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Function with multiple constraints should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0);
    }
}
