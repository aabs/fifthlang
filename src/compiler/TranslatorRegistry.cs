namespace compiler;

/// <summary>
/// Lightweight registry to hold the current backend translator during experimentation.
/// This avoids large constructor API changes while providing a single injection point.
/// </summary>
public static class TranslatorRegistry
{
    /// <summary>
    /// Currently registered translator, or null when none is installed.
    /// </summary>
    public static IBackendTranslator? Current { get; set; }
}
