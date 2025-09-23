using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Analysis;

/// <summary>
/// Handles analysis of overload completeness, subsumption detection, and reachability.
/// Implements the core logic for determining if guard sets are complete and identifying
/// unreachable overloads.
/// </summary>
internal class CompletenessAnalyzer
{
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
        // Simple heuristic: if we have any unknown predicates and no base, it's incomplete
        var hasAnalyzable = analyzedOverloads.Any(a => a.PredicateType == PredicateType.Analyzable);
        var hasUnknown = analyzedOverloads.Any(a => a.PredicateType == PredicateType.Unknown);

        // Conservative approach: if there are unknowns or no analyzable predicates, consider incomplete
        return !hasUnknown && hasAnalyzable;
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

        // TODO: Implement detailed subsumption analysis for intervals, etc.
        return false;
    }
}