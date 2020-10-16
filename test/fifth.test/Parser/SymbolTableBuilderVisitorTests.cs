using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using NUnit.Framework;
using static FifthParser;

namespace fifth.parser.Parser.Tests
{
    [TestFixture()]
    public class SymbolTableBuilderVisitorTests : ParserTestBase
    {
        [Test]
        public void TestCanGatherSingleFunctionDefinitions()
        {
            string TestProgram = @"main() {myprint(""hello world"");}";

            var ctx = ParseProgram(TestProgram);
            var visitor = new SymbolTableBuilderVisitor();
            ParseTreeWalker.Default.Walk(visitor, ctx);
            var symtab = visitor.GlobalScope.SymbolTable;
            Assert.That(symtab.Count, Is.EqualTo(1));
            Assert.That(symtab.Resolve("main"), Is.Not.Null);
        }
                [Test]
        public void TestCanGatherMultipleDefinitions()
        {
            string TestProgram = @"main() {myprint(""hello world"");}
            myprint(string x) {std.print(x);}
            blah() {int result = 5; return result;}";

            var ctx = ParseProgram(TestProgram);
            var visitor = new SymbolTableBuilderVisitor();
            ParseTreeWalker.Default.Walk(visitor, ctx);
            var symtab = visitor.GlobalScope.SymbolTable;
            Assert.That(symtab.Count, Is.EqualTo(3));
            foreach (var v in symtab.Values)
            {
                Assert.That(v.SymbolKind, Is.EqualTo(SymbolKind.FunctionDeclaration));
            }
        }
    }
}