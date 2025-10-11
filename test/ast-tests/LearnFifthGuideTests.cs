using FluentAssertions;
using compiler;
using ast;

namespace ast_tests;

/// <summary>
/// Tests that the Learn Fifth in Y Minutes guide contains valid, parseable Fifth code.
/// This ensures the documentation stays in sync with the actual language grammar.
/// </summary>
public class LearnFifthGuideTests
{
    private static string GetLearnFifthFilePath()
    {
        // Navigate from test assembly location to repository root
        var testDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "learnfifth.5th");
    }
    
    [Test]
    public void LearnFifth_SourceFile_ParsesSuccessfully()
    {
        // Arrange
        var filePath = GetLearnFifthFilePath();
        File.Exists(filePath).Should().BeTrue($"Learn Fifth source file should exist at {filePath}");
        
        var source = File.ReadAllText(filePath);
        source.Should().NotBeNullOrEmpty("Learn Fifth source file should have content");
        
        // Act
        var parseResult = FifthParserManager.ParseString(source);
        
        // Assert
        parseResult.Should().NotBeNull("Parser should return a result");
        
        var assembly = parseResult as AssemblyDef;
        assembly.Should().NotBeNull("Parse result should be an AssemblyDef");
        
        // Verify we have modules
        assembly!.Modules.Should().NotBeEmpty("Assembly should contain at least one module");
        
        // Verify we have functions - should have many example functions
        var allFunctions = assembly.Modules
            .SelectMany(m => m.Functions)
            .OfType<FunctionDef>()
            .ToList();
        
        allFunctions.Should().NotBeEmpty("Assembly should contain functions");
        allFunctions.Should().HaveCountGreaterThan(30, "Assembly should contain many example functions");
    }
    
    [Test]
    public void LearnFifth_SourceFile_AppliesLanguageAnalysisPhasesSuccessfully()
    {
        // Arrange
        var filePath = GetLearnFifthFilePath();
        var source = File.ReadAllText(filePath);
        var ast = FifthParserManager.ParseString(source);
        
        // Act
        var processed = FifthParserManager.ApplyLanguageAnalysisPhases(ast);
        
        // Assert
        processed.Should().NotBeNull("Language analysis phases should process the AST successfully");
        
        var assembly = processed as AssemblyDef;
        assembly.Should().NotBeNull("Processed result should be an AssemblyDef");
    }
    
    [Test]
    public void LearnFifth_SourceFile_ContainsExpectedExamples()
    {
        // Arrange
        var filePath = GetLearnFifthFilePath();
        var source = File.ReadAllText(filePath);
        var ast = FifthParserManager.ParseString(source);
        var assembly = ast as AssemblyDef;
        
        var allFunctions = assembly!.Modules
            .SelectMany(m => m.Functions)
            .OfType<FunctionDef>()
            .Select(f => f.Name.Value)
            .ToList();
        
        // Assert - Check for key example functions
        allFunctions.Should().Contain("add", "Should contain add function example");
        allFunctions.Should().Contain("positive", "Should contain constrained function example");
        allFunctions.Should().Contain("classify", "Should contain overloaded function example");
        allFunctions.Should().Contain("example_integer_literals", "Should contain integer literals example");
        allFunctions.Should().Contain("example_list_comprehension", "Should contain list comprehension example");
        allFunctions.Should().Contain("example_triple_literals", "Should contain triple literals example");
        allFunctions.Should().Contain("example_transparent_assertion", "Should contain transparent assertion example");
        allFunctions.Should().Contain("isAdult", "Should contain isAdult helper function");
        
        // Check for classes
        var allClasses = assembly.Modules
            .SelectMany(m => m.Classes)
            .Select(c => c.Name.Value)
            .ToList();
        
        allClasses.Should().Contain("Person", "Should contain Person class");
        allClasses.Should().Contain("Rectangle", "Should contain Rectangle class");
        allClasses.Should().Contain("Calculator", "Should contain Calculator class with methods");
        allClasses.Should().Contain("Shape", "Should contain Shape class for inheritance example");
        allClasses.Should().Contain("Circle", "Should contain Circle class for inheritance example");
    }
    
    [Test]
    public void LearnFifth_SourceFile_ContainsConstrainedFunctionsWithBaseCases()
    {
        // Arrange
        var filePath = GetLearnFifthFilePath();
        var source = File.ReadAllText(filePath);
        var ast = FifthParserManager.ParseString(source);
        var assembly = ast as AssemblyDef;
        
        var allFunctions = assembly!.Modules
            .SelectMany(m => m.Functions)
            .OfType<FunctionDef>()
            .ToList();
        
        // Check positive function has both constrained and base case
        var positiveFunctions = allFunctions.Where(f => f.Name.Value == "positive").ToList();
        positiveFunctions.Should().HaveCount(2, "positive function should have constrained version and base case");
        
        // Check classify function has multiple overloads plus base case
        var classifyFunctions = allFunctions.Where(f => f.Name.Value == "classify").ToList();
        classifyFunctions.Should().HaveCountGreaterThanOrEqualTo(2, "classify function should have multiple overloads including base case");
    }
}
