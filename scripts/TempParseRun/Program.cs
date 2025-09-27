using System;
using System.IO;
using Antlr4.Runtime;
using Fifth;

// Simple harness to print parse tree for class-definition sample
// Compute repository root assuming bin/Debug/net8.0 -> ../.. up to scripts/TempParseRun then up twice
var baseDir = AppContext.BaseDirectory; // .../scripts/TempParseRun/bin/Debug/net8.0/
var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
var text = File.ReadAllText(Path.Combine(repoRoot, "test", "ast-tests", "CodeSamples", "class-definition.5th"));
var input = CharStreams.fromString(text);
var lexer = new FifthLexer(input);
var tokens = new CommonTokenStream(lexer);
var parser = new FifthParser(tokens);
parser.BuildParseTree = true;
var tree = parser.fifth();
Console.WriteLine(tree.ToStringTree(parser));
