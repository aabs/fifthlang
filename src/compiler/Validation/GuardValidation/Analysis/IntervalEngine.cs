using System;

namespace compiler.Validation.GuardValidation.Analysis;

// NOTE: T015 scaffolding. No LINQ, API surface only.
internal readonly struct Interval
{
    public readonly long? Min;
    public readonly long? Max;
    public readonly bool MinInclusive;
    public readonly bool MaxInclusive;

    public Interval(long? min, bool minInclusive, long? max, bool maxInclusive)
    {
        Min = min;
        MinInclusive = minInclusive;
        Max = max;
        MaxInclusive = maxInclusive;
    }

    public static Interval Closed(long min, long max) => new(min, true, max, true);
    public static Interval Open(long min, long max) => new(min, false, max, false);
    public static Interval LeftOpen(long min, long max) => new(min, false, max, true);
    public static Interval RightOpen(long min, long max) => new(min, true, max, false);
    public static Interval Unbounded() => new(null, false, null, false);
}

internal sealed class IntervalEngine
{
    public Interval Intersect(in Interval a, in Interval b)
    {
        // Lower bound: take the greater of the two lower bounds
        long? min;
        bool minInc;

        if (!a.Min.HasValue && !b.Min.HasValue)
        {
            min = null;
            minInc = a.MinInclusive && b.MinInclusive; // inconsequential when unbounded
        }
        else if (!a.Min.HasValue)
        {
            min = b.Min;
            minInc = b.MinInclusive;
        }
        else if (!b.Min.HasValue)
        {
            min = a.Min;
            minInc = a.MinInclusive;
        }
        else if (a.Min.Value > b.Min.Value)
        {
            min = a.Min;
            minInc = a.MinInclusive;
        }
        else if (b.Min.Value > a.Min.Value)
        {
            min = b.Min;
            minInc = b.MinInclusive;
        }
        else // equal values
        {
            min = a.Min; // same as b.Min
            minInc = a.MinInclusive && b.MinInclusive; // both must include to include boundary
        }

        // Upper bound: take the lesser of the two upper bounds
        long? max;
        bool maxInc;

        if (!a.Max.HasValue && !b.Max.HasValue)
        {
            max = null;
            maxInc = a.MaxInclusive && b.MaxInclusive; // inconsequential when unbounded
        }
        else if (!a.Max.HasValue)
        {
            max = b.Max;
            maxInc = b.MaxInclusive;
        }
        else if (!b.Max.HasValue)
        {
            max = a.Max;
            maxInc = a.MaxInclusive;
        }
        else if (a.Max.Value < b.Max.Value)
        {
            max = a.Max;
            maxInc = a.MaxInclusive;
        }
        else if (b.Max.Value < a.Max.Value)
        {
            max = b.Max;
            maxInc = b.MaxInclusive;
        }
        else // equal values
        {
            max = a.Max; // same as b.Max
            maxInc = a.MaxInclusive && b.MaxInclusive; // both must include to include boundary
        }

        var result = new Interval(min, minInc, max, maxInc);
        return result;
    }

    public bool IsEmpty(in Interval interval)
    {
        var min = interval.Min;
        var max = interval.Max;

        if (min.HasValue && max.HasValue)
        {
            if (min.Value > max.Value)
                return true;
            if (min.Value == max.Value)
                return !(interval.MinInclusive && interval.MaxInclusive);
        }

        // Unbounded/half-bounded intervals are never empty
        return false;
    }

    public bool Subsumes(in Interval covering, in Interval covered)
    {
        // Lower bound check
        bool lowerOk;
        if (!covered.Min.HasValue)
        {
            // Covered is unbounded below; covering must also be unbounded below
            lowerOk = !covering.Min.HasValue;
        }
        else if (!covering.Min.HasValue)
        {
            lowerOk = true; // covering extends to -infinity
        }
        else if (covering.Min.Value < covered.Min.Value)
        {
            lowerOk = true;
        }
        else if (covering.Min.Value > covered.Min.Value)
        {
            lowerOk = false;
        }
        else // equal lower bounds
        {
            // Invalid only if covered includes the point but covering excludes it
            lowerOk = !(covered.MinInclusive && !covering.MinInclusive);
        }

        if (!lowerOk) return false;

        // Upper bound check
        bool upperOk;
        if (!covered.Max.HasValue)
        {
            // Covered is unbounded above; covering must also be unbounded above
            upperOk = !covering.Max.HasValue;
        }
        else if (!covering.Max.HasValue)
        {
            upperOk = true; // covering extends to +infinity
        }
        else if (covering.Max.Value > covered.Max.Value)
        {
            upperOk = true;
        }
        else if (covering.Max.Value < covered.Max.Value)
        {
            upperOk = false;
        }
        else // equal upper bounds
        {
            // Invalid only if covered includes the point but covering excludes it
            upperOk = !(covered.MaxInclusive && !covering.MaxInclusive);
        }

        return upperOk;
    }
}
