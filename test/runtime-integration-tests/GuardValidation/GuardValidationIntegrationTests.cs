using FluentAssertions;
using TUnit.Core;
using compiler;

namespace runtime_integration_tests.GuardValidation;

public class GuardValidationIntegrationTests : RuntimeTestBase
{
    private readonly string _testFilesPath = Path.Combine(
        Environment.CurrentDirectory,
        "GuardValidation"
    );

    [Test]
    public async Task CompleteGuards_ShouldCompileWithoutErrors()
    {
        // Arrange
        var filePath = Path.Combine(_testFilesPath, "complete_guards.5th");

        // Act
        var executablePath = await CompileFileAsync(filePath);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Complete guard set should compile successfully");

        // Execute to verify no runtime issues
        var result = await ExecuteAsync(executablePath);
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task IncompleteGuards_ShouldFailCompilation()
    {
        // Arrange
        var filePath = Path.Combine(_testFilesPath, "incomplete_guards.5th");

        // Act & Assert
        var act = async () => await CompileFileAsync(filePath);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("Compilation failed") && ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }

    [Test]
    public async Task UnreachableGuards_ShouldCompileWithWarnings()
    {
        // Arrange - Use direct compiler to access diagnostics
        var filePath = Path.Combine(_testFilesPath, "unreachable_guards.5th");
        var outputFile = Path.Combine(TempDirectory, "unreachable_test.exe");
        GeneratedFiles.Add(outputFile);

        var compiler = new Compiler();
        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: filePath,
            Output: outputFile,
            Diagnostics: true);

        // Act
        var result = await compiler.CompileAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("Should compile successfully despite warnings");

        var warnings = result.Diagnostics.Where(d => d.Level == DiagnosticLevel.Warning).ToList();
        warnings.Should().HaveCount(1);
        warnings[0].Message.Should().Contain("GUARD_UNREACHABLE (W1002)");
    }

    [Test]
    public async Task ValidatedFunction_ShouldHaveValidationMetrics()
    {
        // Arrange
        var filePath = Path.Combine(_testFilesPath, "complete_guards.5th");

        // Act
        var executablePath = await CompileFileAsync(filePath);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Validated function should compile successfully");

        // TODO: When validation instrumentation is fully implemented,
        // add assertions for performance metrics capture
        // This would require the compiler to expose validation metrics
        // which may need to be added as part of the implementation
    }
}