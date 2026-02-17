namespace compiler;

using System.Collections.Generic;

/// <summary>
/// Result returned by a Lowered AST -> Roslyn translator.
/// For the POC we emit generated C# source strings and mapping information.
/// </summary>
public record TranslationResult(
    IReadOnlyList<string> Sources,
    MappingTable Mapping,
    IReadOnlyList<Diagnostic>? Diagnostics = null);

