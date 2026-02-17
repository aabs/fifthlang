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

    [Fact]
    public async Task Comprehension_WithEmptySource_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                empty: [int] = [];
                result: [int] = [x * 2 from x in empty];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with empty source should compile");
    }

    [Fact]
    public async Task Comprehension_WithEmptySourceFromFile_ShouldCompile()
    {
        // Arrange
        var testProgramPath = Path.Combine("TestPrograms", "Comprehensions", "empty-source.5th");
        
        // Act
        var executablePath = await CompileFileAsync(testProgramPath);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Empty source comprehension test should compile");
    }

    [Fact]
    public async Task Comprehension_WithConstraintsFilteringAllItems_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                result: [int] = [x from x in numbers where x > 10];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with constraints filtering all items should compile");
    }

    [Fact]
    public async Task Comprehension_FilteredEmptyResultFromFile_ShouldCompile()
    {
        // Arrange
        var testProgramPath = Path.Combine("TestPrograms", "Comprehensions", "filtered-empty-result.5th");
        
        // Act
        var executablePath = await CompileFileAsync(testProgramPath);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Filtered empty result comprehension test should compile");
    }

    [Fact]
    public async Task Comprehension_WithSingleConstraintFiltering_ShouldCompile()
    {
        // Arrange - T021: Single constraint filters items
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
                evens: [int] = [x from x in numbers where x % 2 == 0];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with single constraint should compile and filter");
    }

    [Fact]
    public async Task Comprehension_WithMultipleConstraintsAND_ShouldCompile()
    {
        // Arrange - T021: Multiple constraints behave as logical AND
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
                inRange: [int] = [x from x in numbers where x > 3, x < 8];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with multiple AND-ed constraints should compile");
    }

    [Fact]
    public async Task Comprehension_WithConstraintAndProjection_ShouldCompile()
    {
        // Arrange - T021: Constraint with projection
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5];
                doubled: [int] = [x * 2 from x in numbers where x < 4];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with constraint and projection should compile");
    }

    [Fact]
    public async Task Comprehension_WithThreeConstraints_ShouldCompile()
    {
        // Arrange - T021: Three constraints all AND-ed
        var sourceCode = """
            main(): int {
                numbers: [int] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
                complex: [int] = [x from x in numbers where x > 2, x < 9, x % 2 == 1];
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehension with three AND-ed constraints should compile");
    }

    [Fact]
    public async Task Comprehension_ConstraintsFilterFromFile_ShouldCompile()
    {
        // Arrange - T022: Comprehensive constraint filtering test from file
        var testProgramPath = Path.Combine("TestPrograms", "Comprehensions", "constraints-filter.5th");
        
        // Act
        var executablePath = await CompileFileAsync(testProgramPath);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Comprehensive constraint filtering test should compile");
    }
}
