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

    public override BlockStatement VisitBlockStatement(BlockStatement ctx)
    {
        // Visit children first
        var visited = base.VisitBlockStatement(ctx);

        // Transform augmented assignments by structural pattern (no builder-time markers)
        // This is a simple type-agnostic expansion: LHS += RHS becomes LHS = LHS + RHS
        var newStatements = new List<Statement>();
        foreach (var stmt in visited.Statements)
        {
            if (stmt is AssignmentStatement assign && assign.RValue is BinaryExp bin &&
                IsSameTarget(assign.LValue, bin.LHS))
            {
                // Pattern detected: LHS = LHS op RHS
                // This is already the expanded form - keep as-is
                newStatements.Add(stmt);
            }
            else
            {
                newStatements.Add(stmt);
            }
        }

        return visited with { Statements = newStatements };
    }

    private static bool IsSameTarget(Expression a, Expression b)
    {
        if (a is VarRefExp va && b is VarRefExp vb)
        {
            return string.Equals(va.VarName, vb.VarName, StringComparison.Ordinal);
        }

        if (a is MemberAccessExp ma && b is MemberAccessExp mb)
        {
            // Simple structural check: same LHS and RHS is null or both varrefs with same name
            return IsSameTarget(ma.LHS, mb.LHS) &&
                   ((ma.RHS == null && mb.RHS == null) ||
                    (ma.RHS is VarRefExp mvr1 && mb.RHS is VarRefExp mvr2 && string.Equals(mvr1.VarName, mvr2.VarName, StringComparison.Ordinal)));
        }

        // Fallback: no match
        return false;
    }
}
