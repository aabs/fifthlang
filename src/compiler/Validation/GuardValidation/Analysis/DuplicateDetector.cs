using ast;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Analysis;

internal sealed class DuplicateDetector
{
    private readonly IntervalEngine _intervals = new();

    public List<(int firstIndex, int duplicateIndex, AnalyzedOverload first, AnalyzedOverload duplicate)> DetectDuplicates(List<AnalyzedOverload> analyzed)
    {
        var results = new List<(int, int, AnalyzedOverload, AnalyzedOverload)>();

        for (int i = 0; i < analyzed.Count; i++)
        {
            if (analyzed[i].PredicateType != PredicateType.Analyzable) continue;

            if (!TryGetInterval(analyzed[i].PredicateDescriptor, out var aInterval)) continue;

            for (int j = i + 1; j < analyzed.Count; j++)
            {
                if (analyzed[j].PredicateType != PredicateType.Analyzable) continue;
                if (!TryGetInterval(analyzed[j].PredicateDescriptor, out var bInterval)) continue;

                // Equal intervals imply duplicate conditions
                var ab = _intervals.Intersect(aInterval, bInterval);
                var ba = _intervals.Intersect(bInterval, aInterval);
                var aSubB = _intervals.Subsumes(aInterval, bInterval);
                var bSubA = _intervals.Subsumes(bInterval, aInterval);
                var same = aSubB && bSubA && !_intervals.IsEmpty(ab) && !_intervals.IsEmpty(ba);
                if (same)
                {
                    results.Add((i + 1, j + 1, analyzed[i], analyzed[j]));
                }
            }
        }

        return results;
    }

    private bool TryGetInterval(PredicateDescriptor descriptor, out Interval interval)
    {
        interval = Interval.Unbounded();
        var usePool = (Environment.GetEnvironmentVariable("FIFTH_GUARD_VALIDATION_POOL") ?? string.Empty) == "1";
        if (usePool)
        {
            using var atoms = new Infrastructure.PooledList<BinaryExp>();

            foreach (var expr in descriptor.Constraints)
            {
                if (!CollectAtoms(expr, atoms))
                    return false;
            }

            if (atoms.Count == 0) return false;

            string? varName = null;
            foreach (var be in atoms)
            {
                if (be.LHS is VarRefExp v && be.RHS is Int32LiteralExp lit)
                {
                    if (varName == null) varName = v.VarName; else if (varName != v.VarName) return false;

                    Interval atomInterval = be.Operator switch
                    {
                        Operator.GreaterThan => new Interval(lit.Value, false, null, false),
                        Operator.GreaterThanOrEqual => new Interval(lit.Value, true, null, false),
                        Operator.LessThan => new Interval(null, false, lit.Value, false),
                        Operator.LessThanOrEqual => new Interval(null, false, lit.Value, true),
                        Operator.Equal => Interval.Closed(lit.Value, lit.Value),
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
            var atoms = new List<BinaryExp>();

            foreach (var expr in descriptor.Constraints)
            {
                if (!CollectAtoms(expr, atoms))
                    return false;
            }

            if (atoms.Count == 0) return false;

            string? varName = null;
            foreach (var be in atoms)
            {
                if (be.LHS is VarRefExp v && be.RHS is Int32LiteralExp lit)
                {
                    if (varName == null) varName = v.VarName; else if (varName != v.VarName) return false;

                    Interval atomInterval = be.Operator switch
                    {
                        Operator.GreaterThan => new Interval(lit.Value, false, null, false),
                        Operator.GreaterThanOrEqual => new Interval(lit.Value, true, null, false),
                        Operator.LessThan => new Interval(null, false, lit.Value, false),
                        Operator.LessThanOrEqual => new Interval(null, false, lit.Value, true),
                        Operator.Equal => Interval.Closed(lit.Value, lit.Value),
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

    private static bool CollectAtoms(Expression expr, Infrastructure.PooledList<BinaryExp> atoms)
    {
        if (expr is BinaryExp be)
        {
            if (be.Operator == Operator.LogicalAnd)
                return CollectAtoms(be.LHS, atoms) && CollectAtoms(be.RHS, atoms);

            if (be.Operator == Operator.GreaterThan || be.Operator == Operator.GreaterThanOrEqual ||
                be.Operator == Operator.LessThan || be.Operator == Operator.LessThanOrEqual ||
                be.Operator == Operator.Equal)
            {
                atoms.Add(be);
                return true;
            }
            return false;
        }
        return false;
    }

    private static bool CollectAtoms(Expression expr, List<BinaryExp> atoms)
    {
        if (expr is BinaryExp be)
        {
            if (be.Operator == Operator.LogicalAnd)
                return CollectAtoms(be.LHS, atoms) && CollectAtoms(be.RHS, atoms);

            if (be.Operator == Operator.GreaterThan || be.Operator == Operator.GreaterThanOrEqual ||
                be.Operator == Operator.LessThan || be.Operator == Operator.LessThanOrEqual ||
                be.Operator == Operator.Equal)
            {
                atoms.Add(be);
                return true;
            }
            return false;
        }
        return false;
    }
}
