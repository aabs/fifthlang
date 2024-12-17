using compiler;
using Fifth.LangProcessingPhases;

namespace ast_tests;

public class TreeDumpingTests
{
    [Fact]
    public void TestParseAndDumpAST()
    {
        var t = typeof(AstBuilderVisitorTests);
        Console.WriteLine(string.Join('\n', t.Assembly.GetManifestResourceNames()));
        using Stream stream = t.Assembly.GetManifestResourceStream(t.Namespace + ".CodeSamples.statement-if.5th");
        var ast = FifthParserManager.ParseEmbeddedResource(stream);
        var dumpVisitor = new DumpTreeVisitor(Console.Out); // Replace with actual visitor
        _ = dumpVisitor.Visit(ast);
    }
}
