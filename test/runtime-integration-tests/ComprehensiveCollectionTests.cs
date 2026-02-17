using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for collection features
/// Each test corresponds to a .5th file in TestPrograms/Collections/
/// </summary>
public class ComprehensiveCollectionTests : RuntimeTestBase
{
    [Fact]
    public async Task list_access_ShouldReturn20()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Collections", "list_access.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(20, "main() should return 20 from items[1] where items = [10, 20, 30]");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Fact]
    public async Task array_sum_ShouldReturn15()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Collections", "array_sum.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(15, "main() should return 15 from sum of [1, 2, 3, 4, 5] = 1+2+3+4+5 = 15");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }
}