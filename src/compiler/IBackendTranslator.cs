namespace compiler;

using ast;

/// <summary>
/// Backend translator interface: translates a lowered AST (AssemblyDef) into
/// a TranslationResult that contains generated C# sources and mapping information.
/// </summary>
public interface IBackendTranslator
{
    /// <summary>
    /// Translate the provided assembly IR into generated sources.
    /// </summary>
    TranslationResult Translate(AssemblyDef assembly);
}
