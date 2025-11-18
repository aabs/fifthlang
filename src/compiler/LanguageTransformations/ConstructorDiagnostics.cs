using ast;

namespace compiler.LanguageTransformations;

/// <summary>
/// Constructor-related diagnostic codes and helper methods for CTOR001-CTOR010.
/// These diagnostics enforce constructor function requirements per spec.
/// </summary>
public static class ConstructorDiagnostics
{
    /// <summary>
    /// CTOR001: No matching constructor found for given arguments
    /// </summary>
    public static Diagnostic NoMatchingConstructor(string className, string argTypes, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"No constructor found for class '{className}' matching arguments ({argTypes})",
            source,
            "CTOR001");
    }

    /// <summary>
    /// CTOR002: Ambiguous constructor call - multiple candidates match equally
    /// </summary>
    public static Diagnostic AmbiguousConstructor(string className, string candidateSignatures, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Ambiguous constructor call for class '{className}'. Candidates: {candidateSignatures}",
            source,
            "CTOR002");
    }

    /// <summary>
    /// CTOR003: Constructor does not assign required fields
    /// </summary>
    public static Diagnostic UnassignedRequiredFields(string className, string fieldList, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Constructor for '{className}' does not assign required fields: {fieldList}",
            source,
            "CTOR003");
    }

    /// <summary>
    /// CTOR004: Missing base constructor call when base has no parameterless constructor
    /// </summary>
    public static Diagnostic MissingBaseConstructorCall(string className, string baseClassName, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Constructor for '{className}' must invoke base constructor; base class '{baseClassName}' has no parameterless constructor",
            source,
            "CTOR004");
    }

    /// <summary>
    /// CTOR005: Cannot synthesize parameterless constructor - required fields lack defaults
    /// </summary>
    public static Diagnostic CannotSynthesizeConstructor(string className, string fieldList, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Cannot synthesize parameterless constructor for '{className}'; required fields lack defaults: {fieldList}",
            source,
            "CTOR005");
    }

    /// <summary>
    /// CTOR006: Duplicate constructor signature
    /// </summary>
    public static Diagnostic DuplicateConstructorSignature(string className, string signature, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Duplicate constructor signature for '{className}': {signature}",
            source,
            "CTOR006");
    }

    /// <summary>
    /// CTOR007: Constructor cannot declare independent type parameters
    /// </summary>
    public static Diagnostic InvalidConstructorTypeParameter(string className, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Constructor '{className}' cannot declare independent type parameters; only class-level type parameters allowed",
            source,
            "CTOR007");
    }

    /// <summary>
    /// CTOR008: Cyclic base constructor dependency detected
    /// </summary>
    public static Diagnostic CyclicBaseConstructor(string cyclePath, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Cyclic base constructor dependency detected: {cyclePath}",
            source,
            "CTOR008");
    }

    /// <summary>
    /// CTOR009: Constructor cannot return a value
    /// </summary>
    public static Diagnostic ValueReturnInConstructor(string className, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Constructor '{className}' cannot return a value; use 'return;' without expression",
            source,
            "CTOR009");
    }

    /// <summary>
    /// CTOR010: Constructor has forbidden modifier
    /// </summary>
    public static Diagnostic ForbiddenModifier(string className, string modifier, string? source = null)
    {
        return new Diagnostic(
            DiagnosticLevel.Error,
            $"Constructor '{className}' has forbidden modifier '{modifier}'; constructors cannot be async, static, abstract, virtual, override, or sealed",
            source,
            "CTOR010");
    }
}
