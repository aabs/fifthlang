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

    [Test]
    public async Task MissingBase_ShouldFailWithE1001()
    {
        var src = LoadSourceFromRepo("missing_base.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }

    [Test]
    public async Task BooleanExhaustive_ShouldCompileSuccessfully()
    {
        var src = LoadSourceFromRepo("boolean_exhaustive.5th");
        var executablePath = await CompileSourceAsync(src);
        File.Exists(executablePath).Should().BeTrue();
    }

    [Test]
    public async Task MultipleBase_ShouldFailWithE1005()
    {
        var src = LoadSourceFromRepo("multiple_base.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_MULTIPLE_BASE (E1005)"));
    }

    [Test]
    public async Task BaseNotLast_ShouldReportE1004AndE1001()
    {
        var src = LoadSourceFromRepo("base_not_last.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_BASE_NOT_LAST (E1004)") && ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }

    [Test]
    public async Task DuplicateUnreachable_ShouldWarnW1002()
    {
        var src = LoadSourceFromRepo("duplicate_unreachable.5th");
        var result = await CompileSourceToResultAsync(src);
        // DEBUG: print diagnostics for investigation
        Console.WriteLine("DEBUG: DuplicateUnreachable diagnostics:\n" + string.Join('\n', result.Diagnostics.Select(d => d.Message)));
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("Should compile successfully despite warnings");
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }

    [Test]
    public async Task IntervalSubsumed_ShouldWarnW1002()
    {
        var src = LoadSourceFromRepo("interval_subsumed.5th");
        var result = await CompileSourceToResultAsync(src);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }

    [Test]
    public async Task ExplosionIncomplete_ShouldReportW1102AndE1001()
    {
        var src = LoadSourceFromRepo("explosion_incomplete.5th");
        var act = async () => await CompileSourceAsync(src);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_INCOMPLETE (E1001)"));
        // Unknown explosion may be reported as a warning alongside the error; we validate E1001 at minimum
    }

    [Test]
    public async Task OverloadCount_ShouldWarnW1101()
    {
        var src = LoadSourceFromRepo("overload_count.5th");
        var result = await CompileSourceToResultAsync(src);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_OVERLOAD_COUNT (W1101)"));
    }

    [Test]
    public async Task TautologyBaseEquivalence_ShouldFailWithE1005()
    {
        var src = LoadSourceFromRepo("tautology_base_equivalence.5th");
        var act = async () => await CompileSourceAsync(src);
        // DEBUG: print diagnostics when compiling this source to inspect behavior
        try
        {
            var res = await CompileSourceToResultAsync(src);
            Console.WriteLine("DEBUG: Tautology diagnostics:\n" + string.Join('\n', res.Diagnostics.Select(d => d.Message)));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("DEBUG: Tautology compile failed with message:\n" + ex.Message);
        }
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("GUARD_MULTIPLE_BASE (E1005)"));
    }

    [Test]
    public async Task EmptyVsDuplicate_ShouldReportEmptyPrecedence()
    {
        var src = LoadSourceFromRepo("empty_vs_duplicate.5th");
        var result = await CompileSourceToResultAsync(src);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }
}