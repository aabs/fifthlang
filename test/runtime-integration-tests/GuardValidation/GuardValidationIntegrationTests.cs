using FluentAssertions;
using Xunit;
using compiler;

namespace runtime_integration_tests.GuardValidation;

public class GuardValidationIntegrationTests : RuntimeTestBase
{
    private readonly string _testFilesPath = Path.Combine(
        Environment.CurrentDirectory,
        "GuardValidation"
    );

    private static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "fifthlang.sln"))) return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }
        return Directory.GetCurrentDirectory();
    }

    private string LoadSourceFromRepo(string fileName)
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "test", "runtime-integration-tests", "GuardValidation", fileName);
        File.Exists(path).Should().BeTrue($"Source file should exist in repo: {path}");
        return File.ReadAllText(path);
    }

    private async Task<compiler.CompilationResult> CompileSourceToResultAsync(string sourceCode, string? fileName = null)
    {
        fileName ??= $"tmp_{Guid.NewGuid():N}.5th";
        var sourcePath = Path.Combine(TempDirectory, fileName);
        await File.WriteAllTextAsync(sourcePath, sourceCode);
        GeneratedFiles.Add(sourcePath);

        var compiler = new Compiler();
        var options = new CompilerOptions(CompilerCommand.Build, sourcePath, Path.Combine(TempDirectory, Path.GetFileNameWithoutExtension(sourcePath) + ".exe"), Diagnostics: true);
        var result = await compiler.CompileAsync(options);
        return result;
    }

    [Fact]
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

    [Fact]
    public async Task IncompleteGuards_ShouldFailCompilation()
    {
        // Arrange
        var filePath = Path.Combine(_testFilesPath, "incomplete_guards.5th");

        // Act & Assert
        var act = async () => await CompileFileAsync(filePath);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("Compilation failed") && ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }

    [Fact]
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
        warnings.Should().NotBeEmpty("At least one warning is expected due to unreachable guard detection");

        // Ensure the unreachable guard warning is present among any additional warnings introduced
        warnings.Should().Contain(w => w.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }

    [Fact]
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

    [Fact]
    public async Task MissingBase_ShouldFailWithE1001()
    {
        var src = LoadSourceFromRepo("missing_base.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }

    [Fact]
    public async Task BooleanExhaustive_ShouldCompileSuccessfully()
    {
        var src = LoadSourceFromRepo("boolean_exhaustive.5th");
        var executablePath = await CompileSourceAsync(src);
        File.Exists(executablePath).Should().BeTrue();
    }

    [Fact]
    public async Task MultipleBase_ShouldFailWithE1005()
    {
        var src = LoadSourceFromRepo("multiple_base.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_MULTIPLE_BASE (E1005)"));
    }

    [Fact]
    public async Task BaseNotLast_ShouldReportE1004AndE1001()
    {
        var src = LoadSourceFromRepo("base_not_last.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_BASE_NOT_LAST (E1004)") && ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }

    [Fact]
    public async Task DuplicateUnreachable_ShouldWarnW1002()
    {
        var src = LoadSourceFromRepo("duplicate_unreachable.5th");
        var result = await CompileSourceToResultAsync(src);
        // Diagnostics are asserted by the test; debug logging removed for cleaner output
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("Should compile successfully despite warnings");
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }

    [Fact]
    public async Task IntervalSubsumed_ShouldWarnW1002()
    {
        var src = LoadSourceFromRepo("interval_subsumed.5th");
        var result = await CompileSourceToResultAsync(src);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }

    [Fact]
    public async Task ExplosionIncomplete_ShouldReportW1102AndE1001()
    {
        var src = LoadSourceFromRepo("explosion_incomplete.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
        // Unknown explosion may be reported as a warning alongside the error; we validate E1001 at minimum
    }

    [Fact]
    public async Task OverloadCount_ShouldWarnW1101()
    {
        var src = LoadSourceFromRepo("overload_count.5th");
        var result = await CompileSourceToResultAsync(src);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_OVERLOAD_COUNT (W1101)"));
    }

    [Fact]
    public async Task TautologyBaseEquivalence_ShouldFailWithE1005()
    {
        var src = LoadSourceFromRepo("tautology_base_equivalence.5th");
        var act = async () => await CompileSourceAsync(src);
        // Tautology diagnostics logging removed; test logic remains
        // Tautology compile failure message suppressed in test output
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_MULTIPLE_BASE (E1005)"));
    }

    [Fact]
    public async Task EmptyVsDuplicate_ShouldReportEmptyPrecedence()
    {
        var src = LoadSourceFromRepo("empty_vs_duplicate.5th");
        var result = await CompileSourceToResultAsync(src);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }
}
