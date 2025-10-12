namespace compiler;

using System.Collections.Generic;
using ast;

/// <summary>
/// Minimal proof-of-concept translator from Lowered AST to generated C# sources.
/// This skeleton intentionally returns string sources (not SyntaxTrees) to avoid
/// introducing Microsoft.CodeAnalysis dependencies into the compiler project at this stage.
/// </summary>
public class LoweredAstToRoslynTranslator : IBackendTranslator
{
    public TranslationResult Translate(AssemblyDef assembly)
    {
        // Produce a minimal generated program that can be used by downstream tests
    var src = "using System; public static class GeneratedEntry { public static int Main() { System.Console.WriteLine(\"Hello from translator POC\"); return 0; } }";
        var sources = new List<string> { src };
        var mapping = new MappingTable();

        // POC: no mapping entries yet
        return new TranslationResult(sources, mapping, new List<Diagnostic>());
    }
}
