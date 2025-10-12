namespace compiler;

using System.Collections.Generic;

/// <summary>
/// Minimal translator skeleton for the Roslyn backend POC.
/// The implementation intentionally returns an empty TranslationResult so
/// tests that express the desired mapping/PDB behavior fail and drive
/// further implementation (TDD-first).
/// </summary>
public enum BackendCompatibilityMode
{
    LegacyShim,
    Strict
}

public class TranslatorOptions
{
    public bool EmitDebugInfo { get; set; } = true;
    public BackendCompatibilityMode BackendCompatibilityMode { get; set; } = BackendCompatibilityMode.LegacyShim;
    public IReadOnlyList<string>? AdditionalReferences { get; set; }
}

public class LoweredAstToRoslynTranslator
{
    public TranslationResult Translate(LoweredAstModule module, TranslatorOptions? options = null)
    {
        options ??= new TranslatorOptions();
        
        var mapping = new MappingTable();
        var sources = new List<string>();
        var diagnostics = new List<Diagnostic>();

        // Generate C# source code for each method in the module
        var sourceBuilder = new System.Text.StringBuilder();
        sourceBuilder.AppendLine("using System;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine($"namespace {SanitizeIdentifier(module.ModuleName)}");
        sourceBuilder.AppendLine("{");
        
        // Generate a class to contain the methods
        sourceBuilder.AppendLine("    public static class GeneratedProgram");
        sourceBuilder.AppendLine("    {");
        
        int currentLine = 5; // Track current line in generated source (after using + namespace + class opening)
        
        foreach (var method in module.Methods)
        {
            // Add mapping entry for this method
            // Map the lowered AST node to the generated source location
            int methodStartLine = currentLine;
            
            // Generate method signature
            sourceBuilder.AppendLine($"        public static void {SanitizeIdentifier(method.Name)}()");
            sourceBuilder.AppendLine("        {");
            sourceBuilder.AppendLine("            // Generated method stub");
            sourceBuilder.AppendLine("        }");
            sourceBuilder.AppendLine();
            
            int methodEndLine = currentLine + 4;
            
            // Add mapping entry: NodeId -> generated source coordinates
            mapping.Add(new MappingEntry(
                method.NodeId, 
                0, // sourceIndex (first and only source file)
                methodStartLine, 
                8, // start column (indentation)
                methodEndLine, 
                9  // end column (closing brace)
            ));
            
            currentLine = methodEndLine + 1; // +1 for blank line
        }
        
        sourceBuilder.AppendLine("    }"); // Close class
        sourceBuilder.AppendLine("}"); // Close namespace
        
        sources.Add(sourceBuilder.ToString());
        
        return new TranslationResult(sources, mapping, diagnostics);
    }
    
    private static string SanitizeIdentifier(string identifier)
    {
        // Basic sanitization: replace invalid characters with underscores
        var result = identifier.Replace('-', '_').Replace('.', '_');
        // Ensure it starts with a letter or underscore
        if (result.Length > 0 && !char.IsLetter(result[0]) && result[0] != '_')
        {
            result = "_" + result;
        }
        return result;
    }
}
