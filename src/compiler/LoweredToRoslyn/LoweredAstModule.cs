namespace compiler;

using System.Collections.Generic;

// Lightweight lowered-AST model used to feed the Roslyn translator in the POC.
// Keep this intentionally small for the initial spike — we will expand shapes
// as the translator implementation progresses.
public record LoweredType(string Name);

public record LoweredMethod(
    string NodeId,
    string Name,
    string SourceFilePath,
    int StartLine,
    int StartColumn,
    int EndLine = 0,
    int EndColumn = 0);

public record LoweredAstModule(
    string ModuleName,
    IReadOnlyList<LoweredType> Types,
    IReadOnlyList<LoweredMethod> Methods,
    IReadOnlyList<string> OriginSourceFiles);
