using ast;
using compiler.LangProcessingPhases;

namespace compiler.NamespaceResolution;

/// <summary>
/// Extracts ModuleMetadata from a parsed AssemblyDef AST.
/// Reads annotations added by AstBuilderVisitor to create namespace resolution metadata.
/// </summary>
public static class ModuleMetadataExtractor
{
    /// <summary>
    /// Extract ModuleMetadata from a parsed AST for a single source file.
    /// </summary>
    /// <param name="ast">The parsed AssemblyDef AST</param>
    /// <param name="sourceFilePath">Absolute path to the source file</param>
    /// <returns>ModuleMetadata containing namespace and import information</returns>
    public static ModuleMetadata ExtractFromAst(AssemblyDef ast, string sourceFilePath)
    {
        if (ast == null || ast.Modules.Count == 0)
        {
            return new ModuleMetadata
            {
                Path = sourceFilePath,
                DeclaredNamespace = null,
                Imports = new List<string>(),
                Declarations = new List<IAstThing>(),
                ParsedRoot = ast
            };
        }

        var module = ast.Modules[0];  // AssemblyDef contains one ModuleDef per source file
        
        // Extract declared namespace from annotations
        string? declaredNamespace = null;
        if (module.Annotations != null && module.Annotations.TryGetValue("DeclaredNamespace", out var nsValue))
        {
            declaredNamespace = nsValue as string;
        }

        // Extract imports from annotations
        var imports = new List<string>();
        if (module.Annotations != null && module.Annotations.TryGetValue("Imports", out var importsValue))
        {
            if (importsValue is List<string> importList)
            {
                imports.AddRange(importList);
            }
        }

        // Extract declarations (functions and classes)
        var declarations = new List<IAstThing>();
        declarations.AddRange(module.Functions);
        declarations.AddRange(module.Classes);

        return new ModuleMetadata
        {
            Path = sourceFilePath,
            DeclaredNamespace = declaredNamespace,
            Imports = imports,
            Declarations = declarations,
            ParsedRoot = ast
        };
    }

    /// <summary>
    /// Extract ModuleMetadata from multiple source files.
    /// </summary>
    public static List<ModuleMetadata> ExtractFromFiles(IEnumerable<string> sourceFiles)
    {
        var metadataList = new List<ModuleMetadata>();
        
        foreach (var sourceFile in sourceFiles)
        {
            try
            {
                var ast = FifthParserManager.ParseFile(sourceFile);
                if (ast is AssemblyDef assemblyDef)
                {
                    var metadata = ExtractFromAst(assemblyDef, sourceFile);
                    metadataList.Add(metadata);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unexpected AST type for {sourceFile}: {ast.GetType().Name}. Expected AssemblyDef.");
                }
            }
            catch (System.Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to parse and extract metadata from {sourceFile}: {ex.Message}", ex);
            }
        }

        return metadataList;
    }
}
