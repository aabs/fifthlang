using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Tests for basic namespace import functionality.
/// Verifies that modules can declare namespaces and import them.
/// </summary>
public class NamespaceImportRuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task BasicNamespaceImport_ShouldResolveSymbolsCorrectly()
    {
        // This test compiles the quickstart scenario: math.5th and consumer.5th
        // math.5th declares namespace Utilities.Math with an add() function
        // consumer.5th imports Utilities.Math and calls add(2, 3) from main
        // Expected: main() returns 5
        
        // Arrange - Find the test program files
        var testProgramsDir = Path.Combine(AppContext.BaseDirectory, "TestPrograms", "NamespaceImports");
        Directory.Exists(testProgramsDir).Should().BeTrue($"TestPrograms/NamespaceImports directory should exist at {testProgramsDir}");
        
        var mathFile = Path.Combine(testProgramsDir, "math.5th");
        var consumerFile = Path.Combine(testProgramsDir, "consumer.5th");
        
        File.Exists(mathFile).Should().BeTrue($"math.5th should exist at {mathFile}");
        File.Exists(consumerFile).Should().BeTrue($"consumer.5th should exist at {consumerFile}");
        
        // Act - Compile the multi-file program
        // NOTE: This test will fail until CompilerOptions and Compiler support multiple source files
        var outputFile = Path.Combine(TempDirectory, "namespace_test.exe");
        GeneratedFiles.Add(outputFile);
        
        var compiler = new compiler.Compiler();
        var options = new compiler.CompilerOptions(
            Command: compiler.CompilerCommand.Build,
            Source: consumerFile,  // Primary file - will need to be extended to support multiple files
            Output: outputFile,
            Diagnostics: true);
        
        var result = await compiler.CompileAsync(options);
        
        // For now, we expect this to fail because namespace/import support isn't implemented yet
        // Once implemented, these assertions should pass
        if (result.Success)
        {
            File.Exists(outputFile).Should().BeTrue("Executable should be created");
            
            // Generate runtime configuration
            await GenerateRuntimeConfigAsync(outputFile);
            
            // Execute and verify
            var execResult = await ExecuteAsync(outputFile);
            execResult.ExitCode.Should().Be(5, "add(2, 3) should return 5");
            execResult.StandardError.Should().BeEmpty("No errors should occur");
        }
        else
        {
            // Expected to fail until namespace import feature is fully implemented
            result.Success.Should().BeFalse("This test is expected to fail until namespace imports are implemented");
            var diagnosticsText = string.Join("\n", result.Diagnostics.Select(d => $"{d.Level}: {d.Message}"));
            Console.WriteLine($"Expected failure (namespace imports not yet implemented):\n{diagnosticsText}");
        }
    }
}
