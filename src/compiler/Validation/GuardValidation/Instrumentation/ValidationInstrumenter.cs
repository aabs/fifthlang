using System.Diagnostics;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Instrumentation;

/// <summary>
/// Handles performance tracking and metrics collection during guard validation.
/// Implements instrumentation to ensure the validation meets the <5% overhead target.
/// </summary>
internal class ValidationInstrumenter
{
    private readonly Stopwatch _stopwatch = new();
    private readonly Dictionary<string, TimeSpan> _phaseTimings = new();
    private int _totalOverloadsAnalyzed;
    private int _totalFunctionGroupsProcessed;
    private readonly Stopwatch _groupStopwatch = new();
    private int _currentGroupOverloads;
    private string? _currentGroupName;

    /// <summary>
    /// Starts timing for a named phase of validation.
    /// </summary>
    public void StartPhase(string phaseName)
    {
        _stopwatch.Restart();
    }

    /// <summary>
    /// Begin timing work for a function/group. Must be paired with RecordGroupMetrics.
    /// </summary>
    public void StartGroup(string? functionName = null, int overloadCount = 0)
    {
        _currentGroupName = functionName;
        _currentGroupOverloads = overloadCount;
        _groupStopwatch.Restart();
    }

    /// <summary>
    /// Record per-function/group metrics and emit instrumentation if enabled.
    /// </summary>
    public void RecordGroupMetrics(FunctionGroup group, double unknownPercent, int unknownCount)
    {
        _groupStopwatch.Stop();
        var elapsedMicros = (long)(_groupStopwatch.Elapsed.TotalMilliseconds * 1000.0);
        Instrumentation.InstrumentationLogger.LogGroupMetrics(
            group.Name + "/" + group.Arity,
            group.Overloads.Count,
            unknownCount,
            unknownPercent,
            elapsedMicros);
    }

    /// <summary>
    /// Ends timing for the current phase and records the duration.
    /// </summary>
    public void EndPhase(string phaseName)
    {
        _stopwatch.Stop();
        _phaseTimings[phaseName] = _stopwatch.Elapsed;
    }

    /// <summary>
    /// Records metrics about function groups processed.
    /// </summary>
    public void RecordFunctionGroupMetrics(int groupCount, int totalOverloads)
    {
        _totalFunctionGroupsProcessed = groupCount;
        _totalOverloadsAnalyzed = totalOverloads;
    }

    /// <summary>
    /// Gets the total time spent in validation.
    /// </summary>
    public TimeSpan TotalTime => _phaseTimings.Values.Aggregate(TimeSpan.Zero, (acc, time) => acc + time);

    /// <summary>
    /// Gets detailed timing breakdown by phase.
    /// </summary>
    public IReadOnlyDictionary<string, TimeSpan> PhaseTimings => _phaseTimings;

    /// <summary>
    /// Gets performance metrics for analysis.
    /// </summary>
    public ValidationMetrics GetMetrics()
    {
        return new ValidationMetrics(
            TotalTime,
            _totalFunctionGroupsProcessed,
            _totalOverloadsAnalyzed,
            new Dictionary<string, TimeSpan>(_phaseTimings)
        );
    }

    /// <summary>
    /// Resets all collected metrics for a new validation session.
    /// </summary>
    public void Reset()
    {
        _stopwatch.Reset();
        _phaseTimings.Clear();
        _totalOverloadsAnalyzed = 0;
        _totalFunctionGroupsProcessed = 0;
    }
}

/// <summary>
/// Performance metrics collected during guard validation.
/// </summary>
internal record ValidationMetrics(
    TimeSpan TotalTime,
    int FunctionGroupsProcessed,
    int OverloadsAnalyzed,
    IReadOnlyDictionary<string, TimeSpan> PhaseTimings
);