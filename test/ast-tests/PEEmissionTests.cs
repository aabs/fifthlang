using Xunit;
using FluentAssertions;
using compiler;

namespace ast_tests;

public class PEEmissionTests
{
    [Fact]
    public async Task DirectPEEmission_WithSimpleProgram_ShouldGenerateValidPE()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"PEEmissionTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var sourceFile = Path.Combine(tempDir, "simple.5th");
            var outputFile = Path.Combine(tempDir, "simple.exe");
            File.WriteAllText(sourceFile, "main():int{return 42;}");
            
            // Act - Force direct PE emission (should be default)
            var compiler = new Compiler();
            var options = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: sourceFile,
                Output: outputFile,
                
                Diagnostics: true);
            
            var result = await compiler.CompileAsync(options);
            
            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue("Direct PE emission should succeed without ilasm.exe");
            result.Diagnostics.Should().Contain(d => d.Message.Contains("Using direct PE emission"), 
                "Should use direct PE emission path");
            
            // Verify the output file was created and is a valid PE
            File.Exists(outputFile).Should().BeTrue("Output PE file should be created");
            result.OutputPath.Should().Be(outputFile);
            
            // Verify it's a valid PE file (basic check)
            var fileBytes = File.ReadAllBytes(outputFile);
            fileBytes.Length.Should().BeGreaterThan(0, "PE file should not be empty");
            
            // PE files start with "MZ" signature
            fileBytes[0].Should().Be(0x4D); // 'M'
            fileBytes[1].Should().Be(0x5A); // 'Z'
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
    public async Task DirectPEEmission_DoesNotRequireIlasm()
    {
        // Arrange - This test validates that we can compile without any external ilasm dependency
        var tempDir = Path.Combine(Path.GetTempPath(), $"NoIlasmTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var sourceFile = Path.Combine(tempDir, "noilasm.5th");
            var outputFile = Path.Combine(tempDir, "noilasm.exe");
            File.WriteAllText(sourceFile, "main():int{return 99;}");
            
            // Act - Use direct PE emission
            var compiler = new Compiler();
            var options = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: sourceFile,
                Output: outputFile); // Direct PE emission is now always used
            
            var result = await compiler.CompileAsync(options);
            
            // Assert - Should succeed without any ilasm dependency
            result.Should().NotBeNull();
            result.Success.Should().BeTrue("Should compile successfully without ilasm.exe");
            result.ExitCode.Should().Be(0);
            
            // Should not contain any ilasm-related error messages
            result.Diagnostics.Should().NotContain(d => 
                d.Message.Contains("ilasm") || 
                d.Message.Contains("IL Assembler") ||
                d.Message.Contains("not found"),
                "Should not have any ilasm-related errors");
                
            File.Exists(outputFile).Should().BeTrue("Should create output assembly");
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