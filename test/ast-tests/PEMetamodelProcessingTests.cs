using Xunit;
using FluentAssertions;
using compiler;
using System.IO;
using System;

namespace ast_tests;

public class PEMetamodelProcessingTests
{
    [Fact]
    public async Task PEEmitter_ProcessesILMetamodel_InsteadOfHardcodedContent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"MetamodelTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Create two different programs that should produce different PE files
            var sourceFile1 = Path.Combine(tempDir, "return42.5th");
            var outputFile1 = Path.Combine(tempDir, "return42.exe");
            File.WriteAllText(sourceFile1, "main():int{return 42;}");
            
            var sourceFile2 = Path.Combine(tempDir, "return123.5th");
            var outputFile2 = Path.Combine(tempDir, "return123.exe");  
            File.WriteAllText(sourceFile2, "main():int{return 123;}");
            
            var compiler = new Compiler();
            
            // Act - Compile both programs
            var options1 = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: sourceFile1,
                Output: outputFile1,
                
                Diagnostics: false); // Disable diagnostics to avoid debug output
            
            var options2 = new CompilerOptions(
                Command: CompilerCommand.Build,
                Source: sourceFile2,
                Output: outputFile2,
                
                Diagnostics: false);
            
            var result1 = await compiler.CompileAsync(options1);
            var result2 = await compiler.CompileAsync(options2);
            
            // Assert - Both should succeed
            result1.Success.Should().BeTrue("First compilation should succeed");
            result2.Success.Should().BeTrue("Second compilation should succeed");
            
            // Assert - Both files should exist and have valid PE headers
            File.Exists(outputFile1).Should().BeTrue("First PE file should exist");
            File.Exists(outputFile2).Should().BeTrue("Second PE file should exist");
            
            var bytes1 = File.ReadAllBytes(outputFile1);
            var bytes2 = File.ReadAllBytes(outputFile2);
            
            // Basic PE validation
            bytes1[0].Should().Be(0x4D); // 'M'
            bytes1[1].Should().Be(0x5A); // 'Z'
            bytes2[0].Should().Be(0x4D); // 'M'
            bytes2[1].Should().Be(0x5A); // 'Z'
            
            // Key test: The files should be different because they contain different IL
            // If the PEEmitter was ignoring the IL metamodel and hardcoding content,
            // both files would be identical
            bytes1.Should().NotEqual(bytes2, 
                "PE files should be different because they contain different user code. " +
                "If they are identical, the PEEmitter is likely ignoring the IL metamodel.");
                
            // Additional validation: files should have reasonable sizes
            bytes1.Length.Should().BeGreaterThan(100, "PE file should not be trivially small");
            bytes2.Length.Should().BeGreaterThan(100, "PE file should not be trivially small");
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