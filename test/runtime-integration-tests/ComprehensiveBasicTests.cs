using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for basic language features
/// Each test corresponds to a .5th file in TestPrograms/Basic/
/// </summary>
public class ComprehensiveBasicTests : RuntimeTestBase
{
    [Fact]
    public async Task return_int_ShouldReturn42()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Basic", "return_int.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(42, "main() should return 42 as specified in the source");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Fact]
    public async Task arithmetic_ShouldReturn25()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Basic", "arithmetic.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(25, "main() should return 25 (10 + 15) as calculated in the source");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }
}