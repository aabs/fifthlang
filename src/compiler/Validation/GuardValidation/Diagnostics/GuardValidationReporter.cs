using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Diagnostics;

internal sealed class GuardValidationReporter
{
    private readonly DiagnosticEmitter _emitter;

    public GuardValidationReporter(DiagnosticEmitter emitter)
    {
        _emitter = emitter;
    }

    public void ReportUnreachable(FunctionGroup group, List<(int unreachableIndex, int coveringIndex, AnalyzedOverload unreachable, AnalyzedOverload covering)> items)
    {
        foreach (var (ui, ci, u, c) in items)
        {
            _emitter.EmitUnreachableWarning(group, u.Overload, c.Overload, ui, ci);
        }
    }
}
