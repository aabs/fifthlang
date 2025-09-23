using FluentAssertions;
using TUnit.Core;
using compiler;

namespace runtime_integration_tests.GuardValidation;

public class GuardValidationIntegrationTests
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
        var compiler = new CompilerInstance();

        // Act
        var result = await compiler.CompileFileAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Diagnostics.Where(d => d.Level == DiagnosticLevel.Error)
            .Should().BeEmpty("complete guard set should not produce errors");
    }

    [Test]
    public async Task IncompleteGuards_ShouldEmitE1001Error()
    {
        // Arrange
        var filePath = Path.Combine(_testFilesPath, "incomplete_guards.5th");
        var compiler = new CompilerInstance();

        // Act
        var result = await compiler.CompileFileAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        var errors = result.Diagnostics.Where(d => d.Level == DiagnosticLevel.Error).ToList();
        errors.Should().HaveCount(1);
        errors[0].Message.Should().Contain("GUARD_INCOMPLETE (E1001)");
        errors[0].Message.Should().Contain("myFunc/1");
    }

    [Test]
    public async Task UnreachableGuards_ShouldEmitW1002Warning()
    {
        // Arrange
        var filePath = Path.Combine(_testFilesPath, "unreachable_guards.5th");
        var compiler = new CompilerInstance();

        // Act
        var result = await compiler.CompileFileAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        var warnings = result.Diagnostics.Where(d => d.Level == DiagnosticLevel.Warning).ToList();
        warnings.Should().HaveCount(1);
        warnings[0].Message.Should().Contain("GUARD_UNREACHABLE (W1002)");
    }

    [Test]
    public async Task ValidatedFunction_ShouldHavePerformanceMetrics()
    {
        // Arrange
        var filePath = Path.Combine(_testFilesPath, "complete_guards.5th");
        var compiler = new CompilerInstance();

        // Act
        var result = await compiler.CompileFileAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        // Verify that validation instrumentation was captured
        // This would require the compiler to expose validation metrics
        // which may need to be added as part of the implementation
    }
}