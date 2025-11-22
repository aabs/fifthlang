using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for built-in functions and standard library features
/// Each test corresponds to a .5th file in TestPrograms/BuiltIns/
/// </summary>
public class ComprehensiveBuiltInTests : RuntimeTestBase
{
    [Fact]
    public async Task string_output_ShouldReturnZeroAndPrintHelloWorld()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "BuiltIns", "string_output.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(0, "main() should return 0 after calling greet() which has void return type");
        result.StandardOutput.Should().Contain("Hello, World", "greet() should output 'Hello, World' via std.print()");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }
}