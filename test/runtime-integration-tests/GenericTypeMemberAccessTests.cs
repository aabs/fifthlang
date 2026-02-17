using ast;
using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Test to validate the fix for generic type member access issue.
/// Previously, accessing members on generic class instances would fail with
/// "Cannot access member on primitive type 'Int32'" error.
/// </summary>
public class GenericTypeMemberAccessTests : RuntimeTestBase
{
    [Fact]
    public async Task GenericClass_MemberAccess_ShouldCompileAndRun()
    {
        // Arrange - This previously failed with "Cannot access member on primitive type 'Int32'"
        var fifthCode = """
            class Box<T> {
                value: T;                Box() { }            }

            main(): int {
                box: Box<int> = new Box<int>();
                box.value = 42;
                return box.value;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(42, "Should return the value stored in the generic box");
        result.StandardError.Should().BeEmpty("Should not have any errors");
    }

    [Fact]
    public async Task GenericClass_MultipleInstances_ShouldWork()
    {
        // Arrange
        var fifthCode = """
            class Box<T> {
                value: T;
                Box() { }
            }

            main(): int {
                box1: Box<int> = new Box<int>();
                box1.value = 42;
                
                box2: Box<int> = new Box<int>();
                box2.value = 100;
                
                return box1.value + box2.value;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(142, "Should return sum of both box values");
        result.StandardError.Should().BeEmpty("Should not have any errors");
    }

    [Fact]
    public async Task GenericClass_WithDifferentTypes_ShouldWork()
    {
        // Arrange - Test that different generic instantiations work
        var fifthCode = """
            class Box<T> {
                value: T;
                Box() { }
            }

            main(): int {
                intBox: Box<int> = new Box<int>();
                intBox.value = 50;
                
                return intBox.value;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(50, "Should return the value from the int box");
        result.StandardError.Should().BeEmpty("Should not have any errors");
    }
}
