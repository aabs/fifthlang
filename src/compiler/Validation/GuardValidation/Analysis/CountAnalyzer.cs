namespace compiler.Validation.GuardValidation.Analysis;

internal sealed class CountAnalyzer
{
    private const int Threshold = 33;

    public bool ShouldWarn(int overloadCount) => overloadCount >= Threshold;
}
