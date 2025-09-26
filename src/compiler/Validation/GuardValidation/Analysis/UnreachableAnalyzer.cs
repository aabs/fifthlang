using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Analysis;

internal sealed class UnreachableAnalyzer
{
    private readonly IntervalEngine _intervals = new();

    public List<(int unreachableIndex, int coveringIndex, AnalyzedOverload unreachable, AnalyzedOverload covering)>
        Analyze(List<AnalyzedOverload> analyzed)
    {
        var results = new List<(int, int, AnalyzedOverload, AnalyzedOverload)>();

        for (int i = 0; i < analyzed.Count; i++)
        {
            var current = analyzed[i];
            for (int j = 0; j < i; j++)
            {
                var earlier = analyzed[j];
                if (IsSubsumed(current, earlier))
                {
                    results.Add((i + 1, j + 1, current, earlier));
                    break;
                }
            }
        }

        return results;
    }

    private bool IsSubsumed(AnalyzedOverload current, AnalyzedOverload earlier)
    {
        if (earlier.PredicateType == PredicateType.Base) return true;
        if (earlier.PredicateType != PredicateType.Analyzable || current.PredicateType != PredicateType.Analyzable) return false;

        if (TryGetInterval(earlier.PredicateDescriptor, out var earlierInterval) &&
            TryGetInterval(current.PredicateDescriptor, out var currentInterval))
        {
            // Empty intervals do not subsume anything (FR-070 precedence)
            if (_intervals.IsEmpty(earlierInterval))
            {
                return false;
            }
            return _intervals.Subsumes(earlierInterval, currentInterval);
        }
        return false;
    }

    private bool TryGetInterval(PredicateDescriptor descriptor, out Interval interval)
    {
        // Delegate to CompletenessAnalyzer’s logic analogy (duplicated here for isolation)
        interval = Interval.Unbounded();
        var usePool = (Environment.GetEnvironmentVariable("FIFTH_GUARD_VALIDATION_POOL") ?? string.Empty) == "1";
        if (usePool)
        {
            using var atoms = new Infrastructure.PooledList<ast.BinaryExp>();
            foreach (var expr in descriptor.Constraints)
            {
                if (!CollectAtoms(expr, atoms)) return false;
            }
            if (atoms.Count == 0) return false;

            string? varName = null;
            foreach (var be in atoms)
            {
                if (be.LHS is ast.VarRefExp v && be.RHS is ast.Int32LiteralExp lit)
                {
                    if (varName == null) varName = v.VarName; else if (varName != v.VarName) return false;
                    Interval atomInterval = be.Operator switch
                    {
                        ast.Operator.GreaterThan => new Interval(lit.Value, false, null, false),
                        ast.Operator.GreaterThanOrEqual => new Interval(lit.Value, true, null, false),
                        ast.Operator.LessThan => new Interval(null, false, lit.Value, false),
                        ast.Operator.LessThanOrEqual => new Interval(null, false, lit.Value, true),
                        ast.Operator.Equal => Interval.Closed(lit.Value, lit.Value),
                        _ => default
                    };
                    if (Equals(atomInterval, default(Interval))) return false;
                    interval = _intervals.Intersect(interval, atomInterval);
                }
                else return false;
            }
            return true;
        }
        else
        {
            var atoms = new List<ast.BinaryExp>();
            foreach (var expr in descriptor.Constraints)
            {
                if (!CollectAtoms(expr, atoms)) return false;
            }
            if (atoms.Count == 0) return false;

            string? varName = null;
            foreach (var be in atoms)
            {
                if (be.LHS is ast.VarRefExp v && be.RHS is ast.Int32LiteralExp lit)
                {
                    if (varName == null) varName = v.VarName; else if (varName != v.VarName) return false;
                    Interval atomInterval = be.Operator switch
                    {
                        ast.Operator.GreaterThan => new Interval(lit.Value, false, null, false),
                        ast.Operator.GreaterThanOrEqual => new Interval(lit.Value, true, null, false),
                        ast.Operator.LessThan => new Interval(null, false, lit.Value, false),
                        ast.Operator.LessThanOrEqual => new Interval(null, false, lit.Value, true),
                        ast.Operator.Equal => Interval.Closed(lit.Value, lit.Value),
                        _ => default
                    };
                    if (Equals(atomInterval, default(Interval))) return false;
                    interval = _intervals.Intersect(interval, atomInterval);
                }
                else return false;
            }
            return true;
        }
    }

    private static bool CollectAtoms(ast.Expression expr, Infrastructure.PooledList<ast.BinaryExp> atoms)
    {
        if (expr is ast.BinaryExp be)
        {
            if (be.Operator == ast.Operator.LogicalAnd)
                return CollectAtoms(be.LHS, atoms) && CollectAtoms(be.RHS, atoms);
            if (be.Operator == ast.Operator.GreaterThan || be.Operator == ast.Operator.GreaterThanOrEqual ||
                be.Operator == ast.Operator.LessThan || be.Operator == ast.Operator.LessThanOrEqual ||
                be.Operator == ast.Operator.Equal)
            {
                atoms.Add(be);
                return true;
            }
            return false;
        }
        return false;
    }

    private static bool CollectAtoms(ast.Expression expr, List<ast.BinaryExp> atoms)
    {
        if (expr is ast.BinaryExp be)
        {
            if (be.Operator == ast.Operator.LogicalAnd)
                return CollectAtoms(be.LHS, atoms) && CollectAtoms(be.RHS, atoms);
            if (be.Operator == ast.Operator.GreaterThan || be.Operator == ast.Operator.GreaterThanOrEqual ||
                be.Operator == ast.Operator.LessThan || be.Operator == ast.Operator.LessThanOrEqual ||
                be.Operator == ast.Operator.Equal)
            {
                atoms.Add(be);
                return true;
            }
            return false;
        }
        return false;
    }
}
