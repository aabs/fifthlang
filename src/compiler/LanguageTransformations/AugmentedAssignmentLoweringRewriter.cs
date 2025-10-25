using System;
using System.Collections.Generic;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowers augmented assignment operators (+= and -=) to regular binary expressions.
/// This is a type-agnostic transformation that simply expands:
/// 
///   LHS += RHS  →  LHS = LHS + RHS
///   LHS -= RHS  →  LHS = LHS - RHS
/// 
/// Graph/triple specific handling (e.g., transforming graph + triple to Assert calls)
/// is handled by a separate pass: TripleGraphAdditionLoweringRewriter.
/// </summary>
public class AugmentedAssignmentLoweringRewriter : DefaultRecursiveDescentVisitor
{
    private static readonly FifthType Void = new FifthType.TVoidType() { Name = TypeName.From("void") };

    public override AssignmentStatement VisitAssignmentStatement(AssignmentStatement ctx)
    {
        // First visit children to handle nested structures
        var visited = base.VisitAssignmentStatement(ctx);

        // Check if this is an augmented assignment (marked by parser with annotation)
        if (visited.Annotations != null && 
            visited.Annotations.TryGetValue("AugmentedOperator", out var opObj) &&
            opObj is string op)
        {
            // Expand augmented assignment: LHS op= RHS  →  LHS = LHS op RHS
            Operator binaryOp = op switch
            {
                "+=" => Operator.ArithmeticAdd,
                "-=" => Operator.ArithmeticSubtract,
                _ => throw new InvalidOperationException($"Unknown augmented operator: {op}")
            };

            // Clone the LHS to use in the binary expression, preserving all properties including Type
            var clonedLHS = CloneExpression(visited.LValue);
            
            var binaryExpr = new BinaryExp
            {
                Annotations = new Dictionary<string, object>(),
                LHS = clonedLHS,
                RHS = visited.RValue,
                Operator = binaryOp,
                Location = visited.Location,
                Type = visited.LValue.Type ?? Void
            };

            // Remove the augmented operator annotation since we've expanded it
            var newAnnotations = new Dictionary<string, object>(visited.Annotations);
            newAnnotations.Remove("AugmentedOperator");

            return visited with
            {
                RValue = binaryExpr,
                Annotations = newAnnotations
            };
        }

        return visited;
    }

    /// <summary>
    /// Clone an expression, preserving all properties including Type annotations.
    /// Uses the visitor pattern to recursively copy the entire expression tree.
    /// </summary>
    private static Expression CloneExpression(Expression expr)
    {
        return new ExpressionCloner().VisitExpression(expr);
    }

    /// <summary>
    /// Visitor that performs deep cloning of expressions, preserving all properties.
    /// </summary>
    private class ExpressionCloner : DefaultRecursiveDescentVisitor
    {
        public Expression VisitExpression(Expression expr)
        {
            return (Expression)Visit(expr);
        }
        
        // The base DefaultRecursiveDescentVisitor uses 'with' syntax to create copies,
        // which preserves all properties including Type annotations.
    }
}
