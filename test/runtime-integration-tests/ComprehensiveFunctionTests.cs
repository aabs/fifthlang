using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for function features
/// Each test corresponds to a .5th file in TestPrograms/Functions/
/// </summary>
public class ComprehensiveFunctionTests : RuntimeTestBase
{
    [Fact]
    public async Task simple_function_ShouldReturn25()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Functions", "simple_function.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(25, "main() should return 25 from add(10, 15)");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Fact]
    public async Task recursive_function_ShouldReturn120()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Functions", "recursive_function.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(120, "main() should return 120 from factorial(5) = 5! = 120");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Fact]
    public async Task overloaded_function_ShouldReturn3()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Functions", "overloaded_function.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(3, "main() should return 3 from foo(10) + foo(20) = 1 + 2 = 3 based on constraint overloading");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }
}