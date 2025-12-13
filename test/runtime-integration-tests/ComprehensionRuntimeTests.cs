using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Tests for list comprehensions with the new from/where syntax.
/// Tests basic variable projection, constraints, and complex expressions.
/// </summary>
public class ComprehensionRuntimeTests : RuntimeTestBase
{
    [Fact]
    public async Task SimpleComprehension_WithProjection_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                doubled: [int] = [x * 2 from x in numbers];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Simple comprehension with projection should compile");
    }

    [Fact]
    public async Task Comprehension_WithSingleWhereConstraint_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                evens: [int] = [x from x in numbers where x % 2 == 0];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with single where constraint should compile");
    }

    [Fact]
    public async Task Comprehension_WithMultipleWhereConstraints_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                filtered: [int] = [x from x in numbers where x > 1, x < 5];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with multiple where constraints should compile");
    }

    [Fact]
    public async Task Comprehension_WithComplexProjection_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                transformed: [int] = [x * 2 + 1 from x in numbers where x > 2];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with complex projection should compile");
    }

    [Fact]
    public async Task Comprehension_FromFile_ShouldCompile()
    {
        // Arrange
        var testProgramPath = Path.Combine("TestPrograms", "Comprehensions", "basic-list-comprehension.5th");
        
        // Act
        var executablePath = await CompileFileAsync(testProgramPath);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension test program should compile");
    }
}
