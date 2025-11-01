using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for try/catch/finally exception handling
/// Tests Phase 5 (IL emission) and Phase 7 (runtime semantics)
/// Each test corresponds to a .5th file in TestPrograms/TryCatchFinally/
/// </summary>
public class TryCatchFinallyRuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task TryFinally_Basic_ShouldReturn25()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "TryCatchFinally", "try_finally_basic.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert - T038: Finally always executes
        result.ExitCode.Should().Be(25, "finally block should execute and add 10 to x (10+5+10=25)");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task TryCatch_Basic_ShouldReturn42()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "TryCatchFinally", "try_catch_basic.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert - T031: try/catch structure compiles and executes
        result.ExitCode.Should().Be(42, "try block should execute normally (no throw), result=42");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task TryCatchFinally_Combined_ShouldReturn15()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "TryCatchFinally", "try_catch_finally.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert - T031 + T038: Try/catch/finally structure compiles
        result.ExitCode.Should().Be(15, "try sets result to 10, finally adds 5 (10+5=15)");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task CatchClause_StructuralTest()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "TryCatchFinally", "throw_statement.5th");
        
        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);
        
        // Execute and validate result
        var result = await ExecuteAsync(executablePath);
        
        // Assert - T031: Catch clause IL structure is valid
        result.ExitCode.Should().Be(99, "try block executes normally, result=99");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }
}
