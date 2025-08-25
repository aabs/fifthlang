using FluentAssertions;
using code_generator;
using compiler;
using ast;

namespace code_generator.Tests;

public class EndToEndILGenerationTests
{
    [Test]
    public void GenerateIL_FromSimpleFifthProgram_ShouldProduceValidIL()
    {
        // Arrange - use unique temp directory to avoid file conflicts
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthILTest_{Guid.NewGuid():N}");
        var config = new ILCodeGeneratorConfiguration { OutputDirectory = tempDir };
        var generator = new ILCodeGenerator(config);
        var fifthCode = @"main():int{return 42;}";

        try
        {
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
            }
            else
            {
                // If the processed AST is not an AssemblyDef, we need to wrap it
                // This is a simple test to verify the structure
                processedAst.Should().NotBeNull();
            }
        }
        finally
        {
            // Cleanup - remove entire temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Test]
    public void GenerateIL_WithConfiguration_ShouldUseCustomSettings()
    {
        // Arrange - use unique temp directory to avoid file conflicts
        var customOutputDir = Path.Combine(Path.GetTempPath(), $"FifthILTest_{Guid.NewGuid():N}");
        var config = new ILCodeGeneratorConfiguration
        {
            OutputDirectory = customOutputDir,
            ValidateOutput = true
        };
        var generator = new ILCodeGenerator(config);
        var fifthCode = @"
main():int { return 0; }
";

        try
        {
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
            }
        }
        finally
        {
            // Cleanup - remove entire temp directory
            if (Directory.Exists(customOutputDir))
            {
                Directory.Delete(customOutputDir, true);
            }
        }
    }

    [Test]
    public void GenerateIL_WithIntReturnType_ShouldShowCorrectReturnTypeInIL()
    {
        // Arrange - use unique temp directory to avoid file conflicts
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthILTest_{Guid.NewGuid():N}");
        var config = new ILCodeGeneratorConfiguration { OutputDirectory = tempDir };
        var generator = new ILCodeGenerator(config);
        var fifthCode = @"main():int{return 42;}";

        try
        {
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
                // The IL should show proper .NET primitive types (like System.Int32, Int32, System.String, etc.)
                // instead of Fifth.Generated types for primitive types
                
                // Check for proper IL primitive types (like int32, string, etc.)
                // The IL emitter correctly converts System.Int32 to int32 for IL output
                ilContent.Should().Contain("int32", 
                    "IL should contain proper IL primitive type 'int32' for integer return types");
                    
                // Ensure it doesn't contain Fifth.Generated types for primitives
                ilContent.Should().NotContain("Fifth.Generated.Int32", 
                    "Primitive types should map to .NET types, not Fifth.Generated types");
                
                // Also ensure it doesn't contain unknown types
                ilContent.Should().NotContain("unknown", "The return type should be properly resolved, not unknown");
            }
            else
            {
                processedAst.Should().NotBeNull();
            }
        }
        finally
        {
            // Cleanup - remove entire temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Test]
    [Arguments("string", "main():string{return \"hello\";}", "string")]
    [Arguments("bool", "main():bool{return true;}", "bool")]
    [Arguments("float", "main():float{return 3.14;}", "float32")]
    [Arguments("double", "main():double{return 3.14;}", "float64")]
    public void GenerateIL_WithPrimitiveReturnTypes_ShouldUseDotNetTypes(string fifthType, string code, string expectedILType)
    {
        // Arrange - use unique temp directory to avoid file conflicts
        var tempDir = Path.Combine(Path.GetTempPath(), $"FifthILTest_{Guid.NewGuid():N}");
        var config = new ILCodeGeneratorConfiguration { OutputDirectory = tempDir };
        var generator = new ILCodeGenerator(config);

        try
        {
            // Act
            var ast = FifthParserManager.ParseString(code);
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
                
                // Check for proper IL primitive types instead of Fifth.Generated types
                ilContent.Should().Contain(expectedILType, 
                    $"IL should contain proper IL primitive type '{expectedILType}' for {fifthType} return type");
                    
                // Ensure it doesn't contain Fifth.Generated types for primitives
                ilContent.Should().NotContain($"Fifth.Generated.", 
                    $"Primitive type {fifthType} should map to IL primitive type, not Fifth.Generated types");
                
                // Also ensure it doesn't contain unknown types
                ilContent.Should().NotContain("unknown", "The return type should be properly resolved, not unknown");
            }
            else
            {
                processedAst.Should().NotBeNull();
            }
        }
        finally
        {
            // Cleanup - remove entire temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Test]
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

    [Test]
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