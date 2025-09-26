using compiler.Validation.GuardValidation.Analysis;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Diagnostics;

internal sealed class BaseOrderingRules
{
    private readonly CompletenessAnalyzer _analyzer = new();

    public void Apply(FunctionGroup group, DiagnosticEmitter emitter)
    {
        var baseOverloads = group.GetBaseOverloads();
        if (baseOverloads.Count > 1)
        {
            emitter.EmitMultipleBaseError(group, baseOverloads);
            return;
        }

        var firstBase = baseOverloads.FirstOrDefault();
        if (firstBase != null)
        {
            var invalidIndex = _analyzer.ValidateBaseOrdering(group, firstBase);
            if (invalidIndex.HasValue)
            {
                var baseIndex = group.Overloads.IndexOf(firstBase);
                emitter.EmitBaseNotLastError(group, baseIndex, invalidIndex.Value);
            }
        }
    }
}
