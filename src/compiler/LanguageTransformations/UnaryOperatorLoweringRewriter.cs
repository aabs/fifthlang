using System;
using System.Collections.Generic;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowers unary operators to simpler binary expressions or identity operations.
/// This is a type-agnostic transformation that simplifies:
/// 
///   ++p  →  p + 1  (prefix/postfix increment)
///   --p  →  p - 1  (prefix/postfix decrement)
///   p++  →  p + 1  (prefix/postfix increment)
///   p--  →  p - 1  (prefix/postfix decrement)
///   -p   →  -1 * p (unary minus)
///   +p   →  p      (unary plus - identity)
/// 
/// Note: This rewriter operates on UnaryExp nodes in the AST, not on statements.
/// Both prefix and postfix operators are lowered identically to their binary equivalents.
/// The actual assignment semantics (for ++ and --) would typically be handled
/// at a higher level (e.g., in an assignment statement context).
/// </summary>
public class UnaryOperatorLoweringRewriter : DefaultAstRewriter
{
    private static readonly FifthType Void = new FifthType.TVoidType() { Name = TypeName.From("void") };
    private static readonly FifthType IntType = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };

    public override RewriteResult VisitUnaryExp(UnaryExp ctx)
    {
        // First rewrite children
        var operandResult = Rewrite(ctx.Operand);
        var prologue = new List<Statement>(operandResult.Prologue);
        var operand = (Expression)operandResult.Node;

        // Check if this is an operator we need to lower
        Expression loweredExpr = ctx.Operator switch
        {
            // Increment (both prefix and postfix): ++p or p++ => p + 1
            Operator.ArithmeticAdd when IsIncrementDecrement(ctx) => 
                CreateBinaryExpression(operand, Operator.ArithmeticAdd, CreateIntLiteral(1), ctx),
            
            // Decrement (both prefix and postfix): --p or p-- => p - 1
            Operator.ArithmeticSubtract when IsIncrementDecrement(ctx) => 
                CreateBinaryExpression(operand, Operator.ArithmeticSubtract, CreateIntLiteral(1), ctx),
            
            // Unary minus: -p => -1 * p
            Operator.ArithmeticNegative => 
                CreateBinaryExpression(CreateIntLiteral(-1), Operator.ArithmeticMultiply, operand, ctx),
            
            // Unary plus: +p => p (just return the operand)
            // Only handle if this is a unary plus (not increment operator)
            Operator.ArithmeticAdd when !IsIncrementDecrement(ctx) => 
                operand,
            
            // LogicalNot and other operators pass through unchanged
            _ => ctx with { Operand = operand }
        };

        return new RewriteResult(loweredExpr, prologue);
    }

    /// <summary>
    /// Determines if a UnaryExp represents an increment/decrement operator (++ or --)
    /// by checking for specific annotations or context that would indicate this.
    /// 
    /// Since the parser sets Operator.ArithmeticAdd for ++, we need additional
    /// context to distinguish between + and ++.
    /// </summary>
    private bool IsIncrementDecrement(UnaryExp ctx)
    {
        // Check if there's an annotation marking this as increment/decrement
        if (ctx.Annotations != null && 
            ctx.Annotations.TryGetValue("OperatorPosition", out var posObj) &&
            posObj is OperatorPosition pos)
        {
            // Increment/decrement operators have an OperatorPosition annotation
            return pos == OperatorPosition.Prefix || pos == OperatorPosition.Postfix;
        }
        
        // Also check for explicit operator type annotation
        if (ctx.Annotations != null && 
            ctx.Annotations.TryGetValue("OperatorType", out var opTypeObj) &&
            opTypeObj is string opType)
        {
            return opType == "++" || opType == "--";
        }
        
        return false;
    }

    /// <summary>
    /// Creates a binary expression from the given operands and operator.
    /// </summary>
    private BinaryExp CreateBinaryExpression(Expression lhs, Operator op, Expression rhs, UnaryExp source)
    {
        return new BinaryExp
        {
            LHS = lhs,
            RHS = rhs,
            Operator = op,
            Location = source.Location,
            Type = source.Type ?? Void,
            Annotations = new Dictionary<string, object>
            {
                ["FromUnaryLowering"] = true
            }
        };
    }

    /// <summary>
    /// Creates an integer literal expression with the given value.
    /// </summary>
    private Int32LiteralExp CreateIntLiteral(int value)
    {
        return new Int32LiteralExp
        {
            Value = value,
            Type = IntType
        };
    }
}
