using ast;
using compiler;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Diagnostics;

/// <summary>
/// Handles emission of all guard validation diagnostics (E1001-E1005, W1101-W1102).
/// Centralizes diagnostic formatting, secondary notes, and proper error code assignment.
/// </summary>
internal class DiagnosticEmitter
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public void Reset()
    {
        _diagnostics.Clear();
    }

    /// <summary>
    /// Emits E1001: GUARD_INCOMPLETE error for function groups without base case and incomplete coverage.
    /// </summary>
    public void EmitIncompleteError(FunctionGroup group)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Error,
            $"GUARD_INCOMPLETE (E1001): Function '{group.Name}/{group.Arity}' has guarded overloads but no base case and guards are not exhaustive."
        );
        _diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Emits W1002: GUARD_UNREACHABLE warning for overloads that are covered by earlier guards.
    /// </summary>
    public void EmitUnreachableWarning(FunctionGroup group, IOverloadableFunction unreachable, IOverloadableFunction covering, int unreachableIndex, int coveringIndex)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Warning,
            $"GUARD_UNREACHABLE (W1002): Overload #{unreachableIndex} for function '{group.Name}/{group.Arity}' is unreachable (covered by previous guards)."
        );
        _diagnostics.Add(diagnostic);

        // Secondary note per FR-036 (skip if no concrete covering index provided)
        if (coveringIndex > 0)
        {
            var note = new Diagnostic(
                DiagnosticLevel.Info,
                $"note: overload #{unreachableIndex} unreachable due to earlier coverage by overload #{coveringIndex}"
            );
            _diagnostics.Add(note);
        }
    }

    /// <summary>
    /// Emits E1004: GUARD_BASE_NOT_LAST error when base overload is not in final position.
    /// </summary>
    public void EmitBaseNotLastError(FunctionGroup group, int baseIndex, int invalidSubsequentIndex)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Error,
            $"GUARD_BASE_NOT_LAST (E1004): Base (unguarded) overload for function '{group.Name}/{group.Arity}' must be the final overload; subsequent overload at #{invalidSubsequentIndex} is invalid."
        );
        _diagnostics.Add(diagnostic);

        // Secondary note for invalid subsequent overload
        var note = new Diagnostic(
            DiagnosticLevel.Info,
            $"note: overload #{invalidSubsequentIndex} invalid because base overload terminates overloading at #{baseIndex + 1}"
        );
        _diagnostics.Add(note);
    }

    /// <summary>
    /// Emits E1005: GUARD_MULTIPLE_BASE error when function group has multiple base overloads.
    /// </summary>
    public void EmitMultipleBaseError(FunctionGroup group, List<IOverloadableFunction> baseOverloads)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Error,
            $"GUARD_MULTIPLE_BASE (E1005): Multiple unguarded base overloads detected for function '{group.Name}/{group.Arity}'. Only one final base overload is permitted."
        );
        _diagnostics.Add(diagnostic);

        // Secondary notes for additional bases per FR-036
        for (int i = 1; i < baseOverloads.Count; i++)
        {
            var note = new Diagnostic(
                DiagnosticLevel.Info,
                $"note: extra base overload ignored; base already declared at earlier position"
            );
            _diagnostics.Add(note);
        }
    }

    /// <summary>
    /// Emits W1101: GUARD_OVERLOAD_COUNT warning for groups exceeding recommended maximum.
    /// </summary>
    public void EmitOverloadCountWarning(FunctionGroup group)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Warning,
            $"GUARD_OVERLOAD_COUNT (W1101): Overload group for '{group.Name}/{group.Arity}' exceeds recommended maximum of 32 (found {group.Overloads.Count}). Analysis precision may degrade."
        );
        _diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Emits W1102: GUARD_UNKNOWN_EXPLOSION warning for groups with excessive unknown predicates.
    /// </summary>
    public void EmitUnknownExplosionWarning(FunctionGroup group, int unknownPercent)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Warning,
            $"GUARD_UNKNOWN_EXPLOSION (W1102): Overload group for '{group.Name}/{group.Arity}' has excessive UNKNOWN guards ({unknownPercent}%). Refactor or add base case for clarity."
        );
        _diagnostics.Add(diagnostic);
    }
}