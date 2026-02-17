using ast;
using ast_generated;
using ast_model.TypeSystem;
using FluentAssertions;

namespace ast_tests;

/// <summary>
/// Tests for the new AST rewriter API with statement-level desugaring support.
/// </summary>
public class AstRewriterTests : VisitorTestsBase
{
    [Fact]
    public void DefaultAstRewriter_ShouldPreserveStructure_WithNoPrologue()
    {
        // Arrange: Create a simple AST with a binary expression
        var left = new Int32LiteralExp { Value = 5 };
        var right = new Int32LiteralExp { Value = 3 };
        var binaryExp = new BinaryExp 
        { 
            LHS = left, 
            RHS = right, 
            Operator = Operator.ArithmeticAdd 
        };
        var rewriter = new DefaultAstRewriter();

        // Act: Rewrite without any overrides
        var result = rewriter.VisitBinaryExp(binaryExp);

        // Assert: Structure preserved, no prologue
        result.Should().NotBeNull();
        result.Prologue.Should().BeEmpty();
        result.Node.Should().BeOfType<BinaryExp>();
        var rewrittenBinary = (BinaryExp)result.Node;
        rewrittenBinary.Operator.Should().Be(Operator.ArithmeticAdd);
    }

    [Fact]
    public void BlockStatement_ShouldConsumePrologueFromChildStatements()
    {
        // Arrange: Create a custom rewriter that hoists statements
        var rewriter = new HoistingRewriter();
        
        // Create a block with a single expression statement containing a binary expression
        var binaryExp = new BinaryExp 
        { 
            LHS = new Int32LiteralExp { Value = 5 },
            RHS = new Int32LiteralExp { Value = 3 },
            Operator = Operator.ArithmeticAdd
        };
        var expStatement = new ExpStatement { RHS = binaryExp };
        var blockStatement = new BlockStatement { Statements = [expStatement] };

        // Act: Rewrite the block
        var result = rewriter.VisitBlockStatement(blockStatement);

        // Assert: Prologue should be empty at block level (consumed)
        result.Should().NotBeNull();
        result.Prologue.Should().BeEmpty("BlockStatement consumes all prologues");
        
        // The block should now contain hoisted statement(s) plus the original
        var rewrittenBlock = (BlockStatement)result.Node;
        rewrittenBlock.Statements.Should().HaveCountGreaterThan(1, 
            "hoisted temp declaration plus the rewritten expression statement");
        
        // First statement should be the hoisted VarDeclStatement
        rewrittenBlock.Statements[0].Should().BeOfType<VarDeclStatement>();
        
        // Second statement should be the rewritten expression statement
        rewrittenBlock.Statements[1].Should().BeOfType<ExpStatement>();
    }

    [Fact]
    public void RewriteResult_From_ShouldCreateResultWithEmptyPrologue()
    {
        // Arrange
        var node = new Int32LiteralExp { Value = 42 };

        // Act
        var result = RewriteResult.From(node);

        // Assert
        result.Should().NotBeNull();
        result.Node.Should().Be(node);
        result.Prologue.Should().BeEmpty();
    }

    /// <summary>
    /// Custom rewriter that demonstrates expression-to-statements desugaring
    /// by hoisting temporary variables for binary expressions.
    /// </summary>
    private class HoistingRewriter : DefaultAstRewriter
    {
        private int _tmpCounter = 0;

        public override RewriteResult VisitBinaryExp(BinaryExp ctx)
        {
            // First rewrite children
            var lhs = Rewrite(ctx.LHS);
            var rhs = Rewrite(ctx.RHS);
            
            // Collect child prologues
            var prologue = new List<Statement>();
            prologue.AddRange(lhs.Prologue);
            prologue.AddRange(rhs.Prologue);

            if (ctx.Operator == Operator.ArithmeticAdd)
            {
                // Hoist: introduce a temporary for the add expression
                var tmpName = $"__tmp{_tmpCounter++}";
                var tmpDecl = new VariableDecl 
                { 
                    Name = tmpName,
                    TypeName = TypeName.From("int"),
                    CollectionType = CollectionType.SingleInstance,
                    Visibility = Visibility.Private
                };
                
                // Create the rewritten binary expression
                var rewrittenBinary = ctx with
                {
                    LHS = (Expression)lhs.Node,
                    RHS = (Expression)rhs.Node
                };
                
                // Hoist: add temp declaration with initializer
                var declStmt = new VarDeclStatement 
                { 
                    VariableDecl = tmpDecl,
                    InitialValue = rewrittenBinary
                };
                prologue.Add(declStmt);
                
                // Return a var reference instead of the binary expression
                var tmpRef = new VarRefExp 
                { 
                    VarName = tmpName,
                    VariableDecl = tmpDecl
                };
                return new RewriteResult(tmpRef, prologue);
            }

            // Default: just rebuild with rewritten children
            var rebuilt = ctx with
            {
                LHS = (Expression)lhs.Node,
                RHS = (Expression)rhs.Node
            };
            return new RewriteResult(rebuilt, prologue);
        }
    }
}
