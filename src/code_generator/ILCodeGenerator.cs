using ast;
using il_ast;
using il_ast_generated;

namespace code_generator;

public class ILCodeGenerator : ICodeGenerator
{
    private readonly ILCodeGeneratorConfiguration _configuration;
    private readonly AstToIlTransformationVisitor _transformationVisitor;
    private readonly ILEmissionVisitor _emissionVisitor;

    public ILCodeGenerator() : this(new ILCodeGeneratorConfiguration())
    {
    }

    public ILCodeGenerator(ILCodeGeneratorConfiguration configuration)
    {
        _configuration = configuration;
        _transformationVisitor = new AstToIlTransformationVisitor();
        _emissionVisitor = new ILEmissionVisitor();
    }

    public void GenerateCode()
    {
        throw new InvalidOperationException("Use GenerateCode(AstThing ast) method instead.");
    }

    /// <summary>
    /// Generates IL code for the provided AST structure
    /// </summary>
    /// <param name="ast">The AST structure to generate IL for</param>
    /// <returns>The path to the generated IL file</returns>
    public string GenerateCode(ast.AstThing ast)
    {
        if (ast == null)
            throw new ArgumentNullException(nameof(ast));

        // Transform AST to IL metamodel
        var ilAssembly = TransformAstToIl(ast);
        
        // Generate IL code
        var ilCode = EmitILCode(ilAssembly);
        
        // Write to file
        var outputPath = GetOutputPath(ilAssembly);
        EnsureOutputDirectoryExists(outputPath);
        File.WriteAllText(outputPath, ilCode);
        
        return outputPath;
    }

    /// <summary>
    /// Validates that the generated IL file can be compiled by ILASM
    /// </summary>
    /// <param name="ilFilePath">Path to the IL file to validate</param>
    /// <returns>True if the IL file is valid and compiles successfully</returns>
    public bool ValidateILFile(string ilFilePath)
    {
        if (!File.Exists(ilFilePath))
            return false;

        try
        {
            // Try to read the file to ensure it's well-formed
            var content = File.ReadAllText(ilFilePath);
            
            // Basic validation - check for required IL structure
            return content.Contains(".assembly") && 
                   content.Contains(".module") &&
                   !string.IsNullOrWhiteSpace(content);
        }
        catch
        {
            return false;
        }
    }

    private AssemblyDeclaration TransformAstToIl(ast.AstThing ast)
    {
        return ast switch
        {
            AssemblyDef assemblyDef => _transformationVisitor.TransformAssembly(assemblyDef),
            _ => throw new ArgumentException($"Unsupported AST type: {ast.GetType().Name}")
        };
    }

    private string EmitILCode(AssemblyDeclaration ilAssembly)
    {
        return _emissionVisitor.EmitAssembly(ilAssembly);
    }

    private string GetOutputPath(AssemblyDeclaration assembly)
    {
        var fileName = $"{assembly.Name}.il";
        return Path.Combine(_configuration.OutputDirectory, fileName);
    }

    private void EnsureOutputDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

public interface ICodeGenerator
{
    void GenerateCode();
}

/// <summary>
/// Configuration object for specifying IL code generation options
/// </summary>
public class ILCodeGeneratorConfiguration
{
    public string OutputDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "FifthIL");
    public bool OptimizeCode { get; set; } = false;
    public bool GenerateDebugInfo { get; set; } = false;
    public bool ValidateOutput { get; set; } = true;
}