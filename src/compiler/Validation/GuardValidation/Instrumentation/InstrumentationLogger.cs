using System;
using System.Text.Json;

namespace compiler.Validation.GuardValidation.Instrumentation;

/// <summary>
/// Lightweight instrumentation logger for guard validation.
/// When enabled via env var `FIFTH_GUARD_VALIDATION_PROFILE=1` this class writes
/// one JSON line per function/group with metrics usable by local profiling tools.
/// </summary>
internal static class InstrumentationLogger
{
    private static readonly bool Enabled;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    static InstrumentationLogger()
    {
        try
        {
            var v = Environment.GetEnvironmentVariable("FIFTH_GUARD_VALIDATION_PROFILE");
            Enabled = !string.IsNullOrEmpty(v) && (v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            Enabled = false;
        }
    }

    public static void LogGroupMetrics(string functionName, int overloadCount, int unknownCount, double unknownPercent, long elapsedMicros)
    {
        if (!Enabled) return;

        var obj = new
        {
            function = functionName,
            overloads = overloadCount,
            unknown = unknownCount,
            unknownPercent = Math.Round(unknownPercent, 2),
            elapsedMicros = elapsedMicros
        };

        try
        {
            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            Console.Error.WriteLine(json);
        }
        catch
        {
            // Best-effort logging only — do not let instrumentation throw
        }
    }
}
