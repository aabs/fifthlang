using System;
using System.IO;
using Antlr4.Runtime;
using Fifth;

class Entry
{
    static void Main()
    {
        var text = File.ReadAllText("test/ast-tests/CodeSamples/class-definition.5th");
        var input = CharStreams.fromString(text);
        var lexer = new FifthLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new FifthParser(tokens);
        parser.BuildParseTree = true;
        var tree = parser.fifth();
        Console.WriteLine(tree.ToStringTree(parser));
    }
}
