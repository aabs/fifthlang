using ast;
using ast_model;
using il_ast;
using il_ast_generated;

namespace code_generator;

public class ILCodeGenerator
{
    private readonly ILCodeGeneratorConfiguration _configuration;
    private readonly List<Func<AssemblyDeclaration, AssemblyDeclaration>> _transformationPipeline;
    private readonly AstToIlTransformationVisitor _transformationVisitor;
    private readonly ILEmissionVisitor _emissionVisitor;

    public ILCodeGenerator() : this(new ILCodeGeneratorConfiguration())
    {
    }

    public ILCodeGenerator(ILCodeGeneratorConfiguration configuration)
    {
        _configuration = configuration;
        _transformationPipeline = new List<Func<AssemblyDeclaration, AssemblyDeclaration>>();
        _transformationVisitor = new AstToIlTransformationVisitor();
        _emissionVisitor = new ILEmissionVisitor();
    }

    public ILCodeGenerator(ILCodeGeneratorConfiguration configuration, 
        IEnumerable<Func<AssemblyDeclaration, AssemblyDeclaration>> transformations)
    {
        _configuration = configuration;
        _transformationPipeline = new List<Func<AssemblyDeclaration, AssemblyDeclaration>>(transformations);
        _transformationVisitor = new AstToIlTransformationVisitor();
        _emissionVisitor = new ILEmissionVisitor();
    }

    /// <summary>
    /// Generates IL code for the provided AST structure
    /// </summary>
    /// <param name="ast">The AST structure to generate IL for</param>
    /// <returns>The path to the generated IL file</returns>
    public string GenerateCode(ast.AstThing ast)
    {
        ArgumentNullException.ThrowIfNull(ast);

        try
        {
            // Transform AST to IL metamodel
            var ilAssembly = TransformAstToIl(ast);
            
            // Apply transformation pipeline
            ilAssembly = ApplyTransformationPipeline(ilAssembly);
            
            // Generate IL code
            var ilCode = EmitILCode(ilAssembly);
            
            // Write to file
            var outputPath = GetOutputPath(ilAssembly);
            EnsureOutputDirectoryExists(outputPath);
            File.WriteAllText(outputPath, ilCode);
            
            return outputPath;
        }
        catch (System.Exception ex) when (!(ex is CodeGenerationException))
        {
            throw new CodeGenerationException($"IL code generation failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Transforms AST to IL metamodel without emitting IL text
    /// </summary>
    /// <param name="ast">The AST structure to transform</param>
    /// <returns>The IL metamodel assembly</returns>
    public AssemblyDeclaration TransformToILMetamodel(ast.AstThing ast)
    {
        ArgumentNullException.ThrowIfNull(ast);

        try
        {
            // Transform AST to IL metamodel
            var ilAssembly = TransformAstToIl(ast);
            
            // Apply transformation pipeline
            ilAssembly = ApplyTransformationPipeline(ilAssembly);
            
            return ilAssembly;
        }
        catch (System.Exception ex) when (!(ex is CodeGenerationException))
        {
            throw new CodeGenerationException($"AST to IL metamodel transformation failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Adds a transformation function to the pipeline that will be applied to the IL metamodel
    /// </summary>
    /// <param name="transformation">A function that transforms an AssemblyDeclaration</param>
    public void AddTransformation(Func<AssemblyDeclaration, AssemblyDeclaration> transformation)
    {
        ArgumentNullException.ThrowIfNull(transformation);
        _transformationPipeline.Add(transformation);
    }

    /// <summary>
    /// Applies all registered transformations to the IL assembly in order
    /// </summary>
    /// <param name="ilAssembly">The IL assembly to transform</param>
    /// <returns>The transformed assembly</returns>
    private AssemblyDeclaration ApplyTransformationPipeline(AssemblyDeclaration ilAssembly)
    {
        ArgumentNullException.ThrowIfNull(ilAssembly);
        
        var result = ilAssembly;
        foreach (var transformation in _transformationPipeline)
        {
            try
            {
                result = transformation(result);
            }
            catch (System.Exception ex)
            {
                throw new CodeGenerationException($"Transformation pipeline failed: {ex.Message}", ex);
            }
        }
        return result;
    }

    /// <summary>
    /// Performs basic structural validation of the IL file content.
    /// Note: This does NOT validate IL correctness or compilability. 
    /// Use ILASM compilation in tests to validate actual IL correctness.
    /// </summary>
    /// <param name="ilFilePath">Path to the IL file to validate</param>
    /// <returns>True if the IL file has basic required structure</returns>
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
        try
        {
            return ast switch
            {
                AssemblyDef assemblyDef => _transformationVisitor.TransformAssembly(assemblyDef),
                _ => throw new ArgumentException($"Unsupported AST type: {ast.GetType().Name}")
            };
        }
        catch (System.Exception ex) when (!(ex is CodeGenerationException))
        {
            throw new CodeGenerationException($"AST to IL transformation failed: {ex.Message}", ex);
        }
    }

    private string EmitILCode(AssemblyDeclaration ilAssembly)
    {
        try
        {
            return _emissionVisitor.EmitAssembly(ilAssembly);
        }
        catch (System.Exception ex) when (!(ex is CodeGenerationException))
        {
            throw new CodeGenerationException($"IL code emission failed: {ex.Message}", ex);
        }
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