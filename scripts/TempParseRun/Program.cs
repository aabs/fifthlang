using System.Collections.Generic;
using ast;
using compiler;
using Fifth.LangProcessingPhases;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: TempParseRun <source-file>");
    return 1;
}

var sourcePath = args[0];
if (!File.Exists(sourcePath))
{
    Console.Error.WriteLine($"Source file not found: {sourcePath}");
    return 1;
}

var source = await File.ReadAllTextAsync(sourcePath);

AssemblyDef astRoot;
try
{
    astRoot = (AssemblyDef)FifthParserManager.ParseString(source);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Parsing failed: {ex.Message}");
    return 1;
}

var diagnostics = new List<compiler.Diagnostic>();
var lowered = FifthParserManager.ApplyLanguageAnalysisPhases(
                 astRoot,
                 diagnostics,
                 FifthParserManager.AnalysisPhase.GraphTripleOperatorLowering) as AssemblyDef ?? astRoot;

if (diagnostics.Count > 0)
{
    Console.Error.WriteLine("Diagnostics:");
    foreach (var diag in diagnostics)
    {
        Console.Error.WriteLine($"  [{diag.Level}] {diag.Code ?? ""} {diag.Message}");
    }
}

var dumpVisitor = new DumpTreeVisitor(Console.Out);
dumpVisitor.Visit(lowered);

return 0;
