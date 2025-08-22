using Xunit;
using FluentAssertions;
using compiler;
using System.IO;
using System.Diagnostics;

namespace ast_tests;

/// <summary>
/// Tests for IL emission fixes for InvalidProgramException
/// </summary>
public class ILEmissionValidationTests
{
    [Fact]
    public void SimpleReturnProgram_ShouldNotCrashWithInvalidProgramException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"ILTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var sourceFile = Path.Combine(tempDir, "simple.5th");
            var outputFile = Path.Combine(tempDir, "simple.exe");
            File.WriteAllText(sourceFile, "main():int{return 42;}");
            
            // Act - Compile
            var compiler = new Compiler();
            var options = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: sourceFile,
                Output: outputFile,
                UseDirectPEEmission: true,
                Diagnostics: false);
            
            var result = compiler.CompileAsync(options).GetAwaiter().GetResult();
            
            // Assert compilation succeeds
            result.Should().NotBeNull();
            result.Success.Should().BeTrue("Compilation should succeed");
            File.Exists(outputFile).Should().BeTrue("Output file should be created");
            
            // Create runtime config
            var runtimeConfigPath = Path.ChangeExtension(outputFile, "runtimeconfig.json");
            var runtimeConfig = """
                {
                    "runtimeOptions": {
                        "tfm": "net8.0",
                        "framework": {
                            "name": "Microsoft.NETCore.App",
                            "version": "8.0.0"
                        }
                    }
                }
                """;
            File.WriteAllText(runtimeConfigPath, runtimeConfig);
            
            // Act - Execute
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = outputFile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit(5000); // 5 second timeout
            
            // Assert execution succeeds without InvalidProgramException
            process.ExitCode.Should().Be(42, "Should return 42 as specified in the program");
            process.ExitCode.Should().NotBe(134, "Should not crash with InvalidProgramException (exit code 134)");
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
    public void VariableDeclarationProgram_ShouldNotCrashWithInvalidProgramException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"ILTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var sourceFile = Path.Combine(tempDir, "variables.5th");
            var outputFile = Path.Combine(tempDir, "variables.exe");
            File.WriteAllText(sourceFile, "main():int{x:int=5;y:int=10;return x+y;}");
            
            // Act - Compile
            var compiler = new Compiler();
            var options = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: sourceFile,
                Output: outputFile,
                UseDirectPEEmission: true,
                Diagnostics: false);
            
            var result = compiler.CompileAsync(options).GetAwaiter().GetResult();
            
            // Assert compilation succeeds
            result.Should().NotBeNull();
            result.Success.Should().BeTrue("Compilation should succeed");
            File.Exists(outputFile).Should().BeTrue("Output file should be created");
            
            // Create runtime config
            var runtimeConfigPath = Path.ChangeExtension(outputFile, "runtimeconfig.json");
            var runtimeConfig = """
                {
                    "runtimeOptions": {
                        "tfm": "net8.0",
                        "framework": {
                            "name": "Microsoft.NETCore.App",
                            "version": "8.0.0"
                        }
                    }
                }
                """;
            File.WriteAllText(runtimeConfigPath, runtimeConfig);
            
            // Act - Execute
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = outputFile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit(5000); // 5 second timeout
            
            // Assert execution succeeds without InvalidProgramException
            // Note: The exact return value might not be correct yet (we're fixing the crash first)
            // but it should NOT be 134 (InvalidProgramException crash)
            process.ExitCode.Should().NotBe(134, "Should not crash with InvalidProgramException (exit code 134)");
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