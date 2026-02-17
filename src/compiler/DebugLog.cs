namespace Fifth;

internal static class DebugHelpers
{
    public static readonly bool DebugEnabled =
        (global::System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("1", global::System.StringComparison.Ordinal) ||
        (global::System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("true", global::System.StringComparison.OrdinalIgnoreCase) ||
        (global::System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("on", global::System.StringComparison.OrdinalIgnoreCase);

    public static void DebugLog(string message)
    {
        if (DebugEnabled) global::System.Console.Error.WriteLine(message);
    }
}
