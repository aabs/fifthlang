using ast;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Analysis;

internal sealed class CoverageEvaluator
{
    private readonly IntervalEngine _intervals = new();

    public bool IsBooleanComplete(List<AnalyzedOverload> analyzed)
    {
        // Simple heuristic: for a single boolean parameter x, check x==true and x==false present
        bool hasTrue = false, hasFalse = false;
        foreach (var a in analyzed)
        {
            if (a.PredicateType != PredicateType.Analyzable) continue;
            foreach (var e in a.PredicateDescriptor.Constraints)
            {
                if (e is BinaryExp { Operator: Operator.Equal, LHS: VarRefExp, RHS: BooleanLiteralExp bl })
                {
                    if (bl.Value) hasTrue = true; else hasFalse = true;
                }
            }
        }
        return hasTrue && hasFalse;
    }
}
