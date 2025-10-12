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
        // Extremely small stub to allow test compilation.
        // Real implementation will construct C# SyntaxTrees, emit #line/EmbeddedText
        // and populate MappingTable and Diagnostics.
        var mapping = new MappingTable();
        var sources = new List<string>();
        var diagnostics = new List<Diagnostic>();

        return new TranslationResult(sources, mapping, diagnostics);
    }
}
