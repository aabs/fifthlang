using Antlr4.Runtime;
using ast;
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
        if (listener.Diagnostics.Count == 0 && tree is not null)
        {
            var visitor = new AstBuilderVisitor();
            var visited = visitor.Visit(tree);
            ast = visited as AssemblyDef;
        }

        return new ParsedDocument(uri, text, ast, listener.Diagnostics);
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
    IReadOnlyList<ParsingDiagnostic> Diagnostics);
