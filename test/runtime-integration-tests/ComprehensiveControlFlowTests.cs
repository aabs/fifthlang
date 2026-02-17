using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for control flow features
/// Each test corresponds to a .5th file in TestPrograms/ControlFlow/
/// </summary>
public class ComprehensiveControlFlowTests : RuntimeTestBase
{
    [Fact]
    public async Task if_statement_ShouldReturn1()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "ControlFlow", "if_statement.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(1, "main() should return 1 because x=20 and 20 > 10 is true");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Fact]
    public async Task if_else_statement_ShouldReturn2()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "ControlFlow", "if_else_statement.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(2, "main() should return 2 because x=5 and 5 > 10 is false, so else branch executes");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Fact]
    public async Task while_loop_ShouldReturn5()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "ControlFlow", "while_loop.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert
        result.ExitCode.Should().Be(5, "main() should return 5 because count is incremented 5 times in the while loop");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }
}