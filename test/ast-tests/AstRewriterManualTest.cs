using ast;
using ast_generated;
using ast_model.TypeSystem;
using System;
using System.Collections.Generic;

namespace ast_tests;

/// <summary>
/// Standalone manual test for AST rewriter to verify it works correctly
/// without depending on the full test framework.
/// </summary>
public static class AstRewriterManualTest
{
    /* Commented out to avoid TUnit error - this is a manual test
    public static void Main(string[] args)
    {
        Console.WriteLine("=== AST Rewriter Manual Test ===\n");
        
        TestDefaultRewriterPreservesStructure();
        TestBlockStatementConsumesProlog();
        TestRewriteResultFactory();
        
        Console.WriteLine("\n=== All manual tests passed! ===");
    }
    */

    private static void TestDefaultRewriterPreservesStructure()
    {
        Console.WriteLine("Test 1: DefaultAstRewriter preserves structure with no prologue");
        
        var left = new Int32LiteralExp { Value = 5 };
        var right = new Int32LiteralExp { Value = 3 };
        var binaryExp = new BinaryExp 
        { 
            LHS = left, 
            RHS = right, 
            Operator = Operator.ArithmeticAdd 
        };
        var rewriter = new DefaultAstRewriter();
        
        var result = rewriter.VisitBinaryExp(binaryExp);
        
        Assert(result != null, "Result should not be null");
        Assert(result.Prologue.Count == 0, "Prologue should be empty");
        Assert(result.Node is BinaryExp, "Node should be BinaryExp");
        var rewrittenBinary = (BinaryExp)result.Node;
        Assert(rewrittenBinary.Operator == Operator.ArithmeticAdd, "Operator should be preserved");
        
        Console.WriteLine("  ✓ Passed\n");
    }

    private static void TestBlockStatementConsumesProlog()
    {
        Console.WriteLine("Test 2: BlockStatement consumes prologue from child statements");
        
        var rewriter = new HoistingRewriter();
        
        var binaryExp = new BinaryExp 
        { 
            LHS = new Int32LiteralExp { Value = 5 },
            RHS = new Int32LiteralExp { Value = 3 },
            Operator = Operator.ArithmeticAdd
        };
        var expStatement = new ExpStatement { RHS = binaryExp };
        var blockStatement = new BlockStatement { Statements = [expStatement] };
        
        var result = rewriter.VisitBlockStatement(blockStatement);
        
        Assert(result != null, "Result should not be null");
        Assert(result.Prologue.Count == 0, "BlockStatement should consume all prologues");
        
        var rewrittenBlock = (BlockStatement)result.Node;
        Assert(rewrittenBlock.Statements.Count > 1, 
            $"Block should have hoisted statements (got {rewrittenBlock.Statements.Count})");
        Assert(rewrittenBlock.Statements[0] is VarDeclStatement, 
            "First statement should be hoisted VarDeclStatement");
        Assert(rewrittenBlock.Statements[1] is ExpStatement, 
            "Second statement should be the rewritten ExpStatement");
        
        Console.WriteLine("  ✓ Passed\n");
    }

    private static void TestRewriteResultFactory()
    {
        Console.WriteLine("Test 3: RewriteResult.From creates result with empty prologue");
        
        var node = new Int32LiteralExp { Value = 42 };
        var result = RewriteResult.From(node);
        
        Assert(result != null, "Result should not be null");
        Assert(result.Node == node, "Node should be the same as input");
        Assert(result.Prologue.Count == 0, "Prologue should be empty");
        
        Console.WriteLine("  ✓ Passed\n");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private class HoistingRewriter : DefaultAstRewriter
    {
        private int _tmpCounter = 0;

        public override RewriteResult VisitBinaryExp(BinaryExp ctx)
        {
            var lhs = Rewrite(ctx.LHS);
            var rhs = Rewrite(ctx.RHS);
            
            var prologue = new List<Statement>();
            prologue.AddRange(lhs.Prologue);
            prologue.AddRange(rhs.Prologue);

            if (ctx.Operator == Operator.ArithmeticAdd)
            {
                var tmpName = $"__tmp{_tmpCounter++}";
                var tmpDecl = new VariableDecl 
                { 
                    Name = tmpName,
                    TypeName = TypeName.From("int"),
                    CollectionType = CollectionType.SingleInstance,
                    Visibility = Visibility.Private
                };
                
                var rewrittenBinary = ctx with
                {
                    LHS = (Expression)lhs.Node,
                    RHS = (Expression)rhs.Node
                };
                
                var declStmt = new VarDeclStatement 
                { 
                    VariableDecl = tmpDecl,
                    InitialValue = rewrittenBinary
                };
                prologue.Add(declStmt);
                
                var tmpRef = new VarRefExp 
                { 
                    VarName = tmpName,
                    VariableDecl = tmpDecl
                };
                return new RewriteResult(tmpRef, prologue);
            }

            var rebuilt = ctx with
            {
                LHS = (Expression)lhs.Node,
                RHS = (Expression)rhs.Node
            };
            return new RewriteResult(rebuilt, prologue);
        }
    }
}
