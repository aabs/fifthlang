using Antlr4.Runtime;
using ast;
using compiler;
using compiler.LangProcessingPhases;
using Fifth;

namespace Fifth.LanguageServer.Parsing;

public sealed class ParsingService
{
    public ParsedDocument Parse(Uri uri, string text)
    {
        var listener = new CollectingErrorListener();

        var input = CharStreams.fromString(text);
        var lexer = new FifthLexer(input);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(listener);

        var parser = new FifthParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(listener);

        FifthParser.FifthContext? tree = null;

        try
        {
            tree = parser.fifth();
        }
        catch
        {
            // Syntax errors are captured by the listener
        }

        AssemblyDef? ast = null;
        AssemblyDef? analyzedAst = null;
        var semanticDiagnostics = new List<Diagnostic>();
        if (listener.Diagnostics.Count == 0 && tree is not null)
        {
            var visitor = new AstBuilderVisitor();
            var visited = visitor.Visit(tree);
            ast = visited as AssemblyDef;

            if (ast is not null)
            {
                try
                {
                    var analyzed = FifthParserManager.ApplyLanguageAnalysisPhases(ast, semanticDiagnostics);
                    analyzedAst = analyzed as AssemblyDef;
                }
                catch (Exception ex)
                {
                    semanticDiagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Semantic analysis failed: {ex.Message}"));
                }
            }
        }

        return new ParsedDocument(uri, text, ast, analyzedAst, listener.Diagnostics, semanticDiagnostics);
    }

    private sealed class CollectingErrorListener :
        IAntlrErrorListener<IToken>,
        IAntlrErrorListener<int>
    {
        public List<ParsingDiagnostic> Diagnostics { get; } = new();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
            int charPositionInLine, string msg, RecognitionException e)
        {
            Diagnostics.Add(new ParsingDiagnostic(line - 1, charPositionInLine, msg));
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
            int charPositionInLine, string msg, RecognitionException e)
        {
            Diagnostics.Add(new ParsingDiagnostic(line - 1, charPositionInLine, msg));
        }
    }
}

public sealed record ParsingDiagnostic(int Line, int Column, string Message);

public sealed record ParsedDocument(
    Uri Uri,
    string Text,
    AssemblyDef? Ast,
    AssemblyDef? AnalyzedAst,
    IReadOnlyList<ParsingDiagnostic> Diagnostics,
    IReadOnlyList<Diagnostic> SemanticDiagnostics);
