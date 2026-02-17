using FluentAssertions;
using compiler;

namespace ast_tests;

public class CompilerTests
{
    [Fact]
    public async Task CompileAsync_WithHelpCommand_ShouldReturnSuccess()
    {
        var compiler = new Compiler();
        var options = new CompilerOptions(CompilerCommand.Help);
        
        var result = await compiler.CompileAsync(options);
        
        result.Success.Should().BeTrue();
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task CompileAsync_WithInvalidOptions_ShouldReturnError()
    {
        var compiler = new Compiler();
        var options = new CompilerOptions(CompilerCommand.Build, "", ""); // Missing source and output
        
        var result = await compiler.CompileAsync(options);
        
        result.Success.Should().BeFalse();
        result.ExitCode.Should().Be(1);
        result.Diagnostics.Should().HaveCount(1);
        result.Diagnostics[0].Level.Should().Be(DiagnosticLevel.Error);
        result.Diagnostics[0].Message.Should().Contain("Source file or directory must be specified");
    }

    [Fact]
    public async Task CompileAsync_WithNonExistentSource_ShouldReturnParseError()
    {
        var compiler = new Compiler();
        var options = new CompilerOptions(CompilerCommand.Build, "nonexistent.5th", "test.exe");
        
        var result = await compiler.CompileAsync(options);
        
        result.Success.Should().BeFalse();
        result.ExitCode.Should().Be(1);
        result.Diagnostics.Should().HaveCount(1);
        result.Diagnostics[0].Message.Should().Contain("Source path does not exist");
    }

    [Fact]
    public async Task CompileAsync_WithLintCommand_ShouldNotRequireOutput()
    {
        // Create a simple temporary Fifth file
        var tempFile = Path.GetTempFileName();
        var fifthFile = Path.ChangeExtension(tempFile, ".5th");
        try
        {
            File.WriteAllText(fifthFile, "main():int{return 42;}");
            
            var compiler = new Compiler();
            var options = new CompilerOptions(CompilerCommand.Lint, fifthFile, "");
            
            var result = await compiler.CompileAsync(options);
            
            // Even if there are parse/transform errors, the validation should pass
            // because lint doesn't require output path
            var validationError = options.Validate();
            validationError.Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            if (File.Exists(fifthFile))
                File.Delete(fifthFile);
        }
    }

    [Fact]
    public async Task CompileAsync_WithMockProcessRunner_ShouldUseProvidedRunner()
    {
        var mockRunner = new MockProcessRunner();
        var compiler = new Compiler(mockRunner);
        
        // Test with help command to avoid needing files and processes
        var options = new CompilerOptions(CompilerCommand.Help);
        var result = await compiler.CompileAsync(options);
        
        result.Success.Should().BeTrue();
    }
}

public class CompilerIntegrationTests
{
    [Fact]
    public async Task CompileAsync_WithSimpleFifthFile_ShouldBuild()
    {
        // Create a temporary directory for our test
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Create a simple Fifth file
            var sourceFile = Path.Combine(tempDir, "hello.5th");
            // Compiler always outputs .dll files (cross-platform)
            var outputFile = Path.Combine(tempDir, "hello.dll");
            File.WriteAllText(sourceFile, "main():int{return 42;}");
            
            var compiler = new Compiler();
            var options = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: sourceFile,
                Output: outputFile,
                Diagnostics: true);
            
            var result = await compiler.CompileAsync(options);
            
            // The build should succeed completely - this is an integration test
            // that verifies the entire compilation pipeline works
            result.Should().NotBeNull();
            result.Success.Should().BeTrue("Build should succeed completely. " +
                "If this fails due to missing ilasm, the test environment needs to be properly configured with IL assembler.");
            
            // Verify the output file was actually created
            File.Exists(outputFile).Should().BeTrue("Output executable should be created");
            result.OutputPath.Should().Be(outputFile);
            
            // Should have some diagnostics if diagnostics mode is enabled
            result.Diagnostics.Should().Contain(d => d.Message.Contains("phase"), 
                "Should have phase diagnostics when diagnostics mode is enabled");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task CompileAsync_WithLintCommand_ShouldValidateWithoutGeneratingFiles()
    {
        // Create a temporary directory for our test
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthLintTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Create a simple Fifth file
            var sourceFile = Path.Combine(tempDir, "hello.5th");
            File.WriteAllText(sourceFile, "main():int{return 42;}");
            
            var compiler = new Compiler();
            var options = new CompilerOptions(
                Command: CompilerCommand.Lint,
                Source: sourceFile,
                Diagnostics: true);
            
            var result = await compiler.CompileAsync(options);
            
            // Lint should succeed (or fail with semantic errors, but not missing files)
            if (!result.Success)
            {
                // Should not fail due to missing output path or ilasm
                result.ExitCode.Should().NotBe(4, "Lint should not require ilasm");
                result.Diagnostics.Should().NotContain(d => d.Message.Contains("Output path"));
            }
            
            // Should not generate any output files
            result.OutputPath.Should().BeNull();
            result.ILPath.Should().BeNull();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task CompileAsync_WithEmptyDirectory_ShouldReturnError()
    {
        // Create an empty temporary directory
        var tempDir = Path.Combine(Path.GetTempPath(), $"EmptyTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var compiler = new Compiler();
            var options = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: tempDir,
                Output: Path.Combine(tempDir, "output.exe"));
            
            var result = await compiler.CompileAsync(options);
            
            result.Success.Should().BeFalse();
            result.ExitCode.Should().Be(2); // Parse error
            result.Diagnostics.Should().Contain(d => 
                d.Message.Contains("No .5th files found"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}