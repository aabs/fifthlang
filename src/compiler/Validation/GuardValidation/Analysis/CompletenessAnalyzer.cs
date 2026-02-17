using compiler.Validation.GuardValidation.Infrastructure;
using ast;

namespace compiler.Validation.GuardValidation.Analysis;

/// <summary>
/// Handles analysis of overload completeness, subsumption detection, and reachability.
/// Implements the core logic for determining if guard sets are complete and identifying
/// unreachable overloads.
/// </summary>
internal class CompletenessAnalyzer
{
    private readonly IntervalEngine _intervals = new();
    /// <summary>
    /// Checks for unreachable overloads due to subsumption by earlier overloads.
    /// Returns list of (unreachable_index, covering_index) pairs.
    /// </summary>
    public List<(int unreachableIndex, int coveringIndex, AnalyzedOverload unreachable, AnalyzedOverload covering)>
        CheckForUnreachableOverloads(List<AnalyzedOverload> analyzedOverloads)
    {
        var unreachableOverloads = new List<(int, int, AnalyzedOverload, AnalyzedOverload)>();

        for (int i = 0; i < analyzedOverloads.Count; i++)
        {
            var current = analyzedOverloads[i];

            // Check if this overload is subsumed by any earlier overload
            for (int j = 0; j < i; j++)
            {
                var earlier = analyzedOverloads[j];

                if (IsSubsumed(current, earlier))
                {
                    unreachableOverloads.Add((i + 1, j + 1, current, earlier));
                    break; // Only report first subsumption
                }
            }
        }

        return unreachableOverloads;
    }

    /// <summary>
    /// Checks if the overload group is complete (has exhaustive coverage).
    /// Only applies to groups without base cases.
    /// </summary>
    public bool IsComplete(FunctionGroup group, List<AnalyzedOverload> analyzedOverloads)
    {
        // Conservative default: Without a base overload, assume incomplete.
        // Specific exhaustive cases (e.g., boolean true/false pair) are handled upstream.
        return false;
    }

    /// <summary>
    /// Validates the ordering of base overloads within a function group.
    /// Returns the index (1-based) of the invalid subsequent overload if found, otherwise null.
    /// </summary>
    public int? ValidateBaseOrdering(FunctionGroup group, IOverloadableFunction baseOverload)
    {
        // FR-035: Base must be last
        var baseIndex = group.Overloads.IndexOf(baseOverload);
        if (baseIndex >= 0 && baseIndex < group.Overloads.Count - 1)
        {
            return baseIndex + 2; // Return 1-based index of invalid subsequent overload
        }
        return null;
    }

    /// <summary>
    /// Calculates the percentage of unknown predicates in a group.
    /// </summary>
    public int CalculateUnknownPercentage(List<AnalyzedOverload> analyzedOverloads)
    {
        var unknownCount = analyzedOverloads.Count(a => a.PredicateType == PredicateType.Unknown);
        return (unknownCount * 100) / analyzedOverloads.Count;
    }

    private bool IsSubsumed(AnalyzedOverload current, AnalyzedOverload earlier)
    {
        // Simplified subsumption check
        if (earlier.PredicateType == PredicateType.Base)
        {
            return true; // Base case subsumes everything after it
        }

        // Only attempt interval subsumption when both are analyzable
        if (earlier.PredicateType != PredicateType.Analyzable || current.PredicateType != PredicateType.Analyzable)
        {
            return false;
        }

        // Try to derive intervals for a single variable from constraints
        if (TryGetInterval(earlier.PredicateDescriptor, out var earlierInterval) &&
            TryGetInterval(current.PredicateDescriptor, out var currentInterval))
        {
            return _intervals.Subsumes(earlierInterval, currentInterval);
        }

        // Fallback: unknown subsumption
        return false;
    }

    private bool TryGetInterval(PredicateDescriptor descriptor, out Interval interval)
    {
        // Start as unbounded
        interval = Interval.Unbounded();

        var usePool = (Environment.GetEnvironmentVariable("FIFTH_GUARD_VALIDATION_POOL") ?? string.Empty) == "1";
        if (usePool)
        {
            using var atoms = new Infrastructure.PooledList<BinaryExp>();
            foreach (var expr in descriptor.Constraints)
            {
                if (!CollectAtoms(expr, atoms)) return false;
            }
            if (atoms.Count == 0) return false;

            string? varName = null;
            foreach (var be in atoms)
            {
                if (!(be.LHS is VarRefExp v) || !(be.RHS is Int32LiteralExp lit)) return false;
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
            return true;
        }
        else
        {
            // Collect atomic comparisons from descriptor constraints
            // The descriptor may contain raw constraints or a single conjunction
            var atoms = new List<BinaryExp>();
            foreach (var expr in descriptor.Constraints)
            {
                if (!CollectAtoms(expr, atoms))
                {
                    return false;
                }
            }
            if (atoms.Count == 0)
            {
                return false;
            }

            // Ensure all atoms reference the same variable and use int literals
            string? varName = null;
            foreach (var be in atoms)
            {
                if (!(be.LHS is VarRefExp v) || !(be.RHS is Int32LiteralExp lit))
                {
                    return false;
                }
                if (varName == null)
                {
                    varName = v.VarName;
                }
                else if (varName != v.VarName)
                {
                    return false; // multiple different variables not supported here
                }

                // Build half-interval from this atom and intersect
                Interval atomInterval;
                switch (be.Operator)
                {
                    case Operator.GreaterThan:
                        atomInterval = new Interval(lit.Value, false, null, false);
                        break;
                    case Operator.GreaterThanOrEqual:
                        atomInterval = new Interval(lit.Value, true, null, false);
                        break;
                    case Operator.LessThan:
                        atomInterval = new Interval(null, false, lit.Value, false);
                        break;
                    case Operator.LessThanOrEqual:
                        atomInterval = new Interval(null, false, lit.Value, true);
                        break;
                    case Operator.Equal:
                        atomInterval = Interval.Closed(lit.Value, lit.Value);
                        break;
                    default:
                        return false; // unsupported operator for interval mapping
                }

                interval = _intervals.Intersect(interval, atomInterval);
            }

            return true;
        }
    }

    private bool CollectAtoms(Expression expr, Infrastructure.PooledList<BinaryExp> atoms)
    {
        if (expr is BinaryExp be)
        {
            if (be.Operator == Operator.LogicalAnd)
            {
                return CollectAtoms(be.LHS, atoms) && CollectAtoms(be.RHS, atoms);
            }

            // Supported comparison operators and equality
            if (be.Operator == Operator.GreaterThan ||
                be.Operator == Operator.GreaterThanOrEqual ||
                be.Operator == Operator.LessThan ||
                be.Operator == Operator.LessThanOrEqual ||
                be.Operator == Operator.Equal)
            {
                atoms.Add(be);
                return true;
            }

            return false;
        }

        // Non-binary expressions not supported for interval mapping here
        return false;
    }

    private bool CollectAtoms(Expression expr, List<BinaryExp> atoms)
    {
        if (expr is BinaryExp be)
        {
            if (be.Operator == Operator.LogicalAnd)
            {
                return CollectAtoms(be.LHS, atoms) && CollectAtoms(be.RHS, atoms);
            }

            // Supported comparison operators and equality
            if (be.Operator == Operator.GreaterThan ||
                be.Operator == Operator.GreaterThanOrEqual ||
                be.Operator == Operator.LessThan ||
                be.Operator == Operator.LessThanOrEqual ||
                be.Operator == Operator.Equal)
            {
                atoms.Add(be);
                return true;
            }

            return false;
        }

        // Non-binary expressions not supported for interval mapping here
        return false;
    }
}