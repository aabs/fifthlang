using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Analysis;

internal sealed class ExplosionAnalyzer
{
    public int CalculateUnknownPercent(List<AnalyzedOverload> analyzed)
    {
        if (analyzed.Count == 0) return 0;
        var unknown = analyzed.Count(a => a.PredicateType == PredicateType.Unknown);
        return (unknown * 100) / analyzed.Count;
    }
}
