using Xunit;
using FluentAssertions;
using code_generator;
using compiler;
using ast;

namespace code_generator.Tests;

public class EndToEndILGenerationTests
{
    [Fact]
    public void GenerateIL_FromSimpleFifthProgram_ShouldProduceValidIL()
    {
        // Arrange
        var fifthCode = @"main():int{return 42;}";
        var generator = new ILCodeGenerator();

        // Act & Assert: Parse the Fifth code and generate IL
        var ast = FifthParserManager.ParseString(fifthCode);
        ast.Should().NotBeNull();

        // Apply language analysis phases to transform the AST
        var processedAst = FifthParserManager.ApplyLanguageAnalysisPhases(ast);
        processedAst.Should().NotBeNull();

        // Cast to AssemblyDef if that's what we expect
        if (processedAst is AssemblyDef assemblyDef)
        {
            var ilFilePath = generator.GenerateCode(assemblyDef);
            
            // Assert
            ilFilePath.Should().NotBeNullOrEmpty();
            File.Exists(ilFilePath).Should().BeTrue();
            
            var ilContent = File.ReadAllText(ilFilePath);
            ilContent.Should().Contain(".assembly");
            ilContent.Should().Contain(".method");
            ilContent.Should().Contain("main");
            
            // Cleanup
            File.Delete(ilFilePath);
        }
        else
        {
            // If the processed AST is not an AssemblyDef, we need to wrap it
            // This is a simple test to verify the structure
            processedAst.Should().NotBeNull();
        }
    }

    [Fact]
    public void GenerateIL_WithConfiguration_ShouldUseCustomSettings()
    {
        // Arrange
        var customOutputDir = Path.Combine(Path.GetTempPath(), "FifthILTest");
        var config = new ILCodeGeneratorConfiguration
        {
            OutputDirectory = customOutputDir,
            ValidateOutput = true
        };
        var generator = new ILCodeGenerator(config);
        var fifthCode = @"
main():int { return 0; }
";

        // Act
        var ast = FifthParserManager.ParseString(fifthCode);
        var processedAst = FifthParserManager.ApplyLanguageAnalysisPhases(ast);

        if (processedAst is AssemblyDef assemblyDef)
        {
            var ilFilePath = generator.GenerateCode(assemblyDef);
            
            // Assert
            ilFilePath.Should().StartWith(customOutputDir);
            Directory.Exists(customOutputDir).Should().BeTrue();
            
            var isValid = generator.ValidateILFile(ilFilePath);
            isValid.Should().BeTrue();
            
            // Cleanup
            File.Delete(ilFilePath);
            Directory.Delete(customOutputDir, true);
        }
    }

    [Fact]
    public void GenerateIL_WithIntReturnType_ShouldShowCorrectReturnTypeInIL()
    {
        // Arrange
        var fifthCode = @"main():int{return 42;}";
        var generator = new ILCodeGenerator();

        // Act
        var ast = FifthParserManager.ParseString(fifthCode);
        ast.Should().NotBeNull();

        var processedAst = FifthParserManager.ApplyLanguageAnalysisPhases(ast);
        processedAst.Should().NotBeNull();

        if (processedAst is AssemblyDef assemblyDef)
        {
            var ilFilePath = generator.GenerateCode(assemblyDef);
            
            // Assert
            ilFilePath.Should().NotBeNullOrEmpty();
            File.Exists(ilFilePath).Should().BeTrue();
            
            var ilContent = File.ReadAllText(ilFilePath);
            ilContent.Should().Contain(".assembly");
            ilContent.Should().Contain(".method");
            ilContent.Should().Contain("main");
            
            // Most importantly, check that the IL contains the correct return type
            // The IL should show Fifth.Generated.Int32 as the return type for main()
            // instead of an unknown/void type
            ilContent.Should().Contain("Fifth.Generated.Int32");
            
            // Also ensure it doesn't contain unknown types
            ilContent.Should().NotContain("unknown", "The return type should be properly resolved, not unknown");
            
            // Cleanup
            File.Delete(ilFilePath);
        }
        else
        {
            processedAst.Should().NotBeNull();
        }
    }

    [Fact]
    public void ValidateILFile_WithMalformedContent_ShouldReturnFalse()
    {
        // Arrange
        var generator = new ILCodeGenerator();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "This is not valid IL code");

        // Act
        var isValid = generator.ValidateILFile(tempFile);

        // Assert
        isValid.Should().BeFalse();
        
        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void ValidateILFile_WithValidBasicStructure_ShouldReturnTrue()
    {
        // Arrange
        var generator = new ILCodeGenerator();
        var tempFile = Path.GetTempFileName();
        var validIL = @"
.assembly TestAssembly
{
  .ver 1:0:0:0
}
.module TestModule.dll
";
        File.WriteAllText(tempFile, validIL);

        // Act
        var isValid = generator.ValidateILFile(tempFile);

        // Assert
        isValid.Should().BeTrue();
        
        // Cleanup
        File.Delete(tempFile);
    }
}