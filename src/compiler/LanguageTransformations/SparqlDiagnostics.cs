namespace Fifth.LangProcessingPhases;

/// <summary>
/// Diagnostic codes for SPARQL literal expression errors and warnings.
/// These codes follow the format "SPARQL00X: message text" with line/column offset within the literal body.
/// </summary>
/// <remarks>
/// Code allocation:
/// - SPARQL001-003: Syntax and semantic errors
/// - SPARQL004-006: Interpolation and size constraints
/// </remarks>
public static class SparqlDiagnostics
{
    /// <summary>
    /// SPARQL001: Malformed SPARQL syntax.
    /// Emitted when the SPARQL text cannot be parsed according to SPARQL 1.1 specification.
    /// Example: "SPARQL001: Syntax error at line 2:15 - expected WHERE but found FROM"
    /// </summary>
    public const string MalformedSparql = "SPARQL001";

    /// <summary>
    /// SPARQL002: Unknown variable reference.
    /// Emitted when a variable identifier in the SPARQL text does not match any in-scope Fifth variable.
    /// Example: "SPARQL002: Unknown variable 'age' in SPARQL literal"
    /// </summary>
    public const string UnknownVariable = "SPARQL002";

    /// <summary>
    /// SPARQL003: Incompatible type for parameter binding.
    /// Emitted when a Fifth variable has a type that cannot be bound to a SPARQL parameter.
    /// Types like Graph and Triple cannot be directly bound; only primitives and IRIs are supported.
    /// Example: "SPARQL003: Type 'Graph' cannot be bound to SPARQL parameter 'myGraph'"
    /// </summary>
    public const string IncompatibleType = "SPARQL003";

    /// <summary>
    /// SPARQL004: Nested interpolation not allowed.
    /// Emitted when an interpolation expression contains another interpolation (e.g., {{outer{{inner}}}}).
    /// Example: "SPARQL004: Nested interpolation not allowed"
    /// </summary>
    public const string NestedInterpolation = "SPARQL004";

    /// <summary>
    /// SPARQL005: Non-constant interpolation expression.
    /// Emitted when an interpolation expression is not a compile-time constant or simple variable reference.
    /// Example: "SPARQL005: Interpolation expression must be compile-time constant or variable reference"
    /// </summary>
    public const string NonConstantInterpolation = "SPARQL005";

    /// <summary>
    /// SPARQL006: SPARQL literal exceeds size limit.
    /// Emitted when the SPARQL text exceeds 1MB in size.
    /// Example: "SPARQL006: SPARQL literal exceeds 1MB size limit; consider using external file"
    /// </summary>
    public const string OversizedLiteral = "SPARQL006";

    /// <summary>
    /// Creates a formatted diagnostic message for malformed SPARQL syntax.
    /// </summary>
    public static string FormatMalformedSparql(int line, int column, string details)
        => $"{MalformedSparql}: Malformed SPARQL syntax at line {line}:{column} - {details}";

    /// <summary>
    /// Creates a formatted diagnostic message for unknown variable reference.
    /// </summary>
    public static string FormatUnknownVariable(string variableName)
        => $"{UnknownVariable}: Unknown variable '{variableName}' in SPARQL literal";

    /// <summary>
    /// Creates a formatted diagnostic message for incompatible type binding.
    /// </summary>
    public static string FormatIncompatibleType(string typeName, string variableName)
        => $"{IncompatibleType}: Type '{typeName}' cannot be bound to SPARQL parameter '{variableName}'";

    /// <summary>
    /// Creates a formatted diagnostic message for nested interpolation.
    /// </summary>
    public static string FormatNestedInterpolation()
        => $"{NestedInterpolation}: Nested interpolation not allowed";

    /// <summary>
    /// Creates a formatted diagnostic message for non-constant interpolation.
    /// </summary>
    public static string FormatNonConstantInterpolation()
        => $"{NonConstantInterpolation}: Interpolation expression must be compile-time constant or variable reference";

    /// <summary>
    /// Creates a formatted diagnostic message for oversized literal.
    /// </summary>
    public static string FormatOversizedLiteral(long sizeBytes)
        => $"{OversizedLiteral}: SPARQL literal exceeds 1MB size limit ({sizeBytes} bytes); consider using external file";
}

/// <summary>
/// Extension methods for working with SPARQL diagnostics.
/// </summary>
public static class SparqlDiagnosticExtensions
{
    /// <summary>
    /// Determines if a diagnostic code is a SPARQL-related diagnostic.
    /// </summary>
    public static bool IsSparqlDiagnostic(this string code)
        => code.StartsWith("SPARQL", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the severity level for a SPARQL diagnostic code.
    /// All SPARQL diagnostics are errors except SPARQL006 which is a warning.
    /// </summary>
    public static DiagnosticSeverity GetSeverity(this string code)
    {
        if (!code.IsSparqlDiagnostic())
            return DiagnosticSeverity.Info;

        return code switch
        {
            SparqlDiagnostics.OversizedLiteral => DiagnosticSeverity.Warning,
            _ => DiagnosticSeverity.Error
        };
    }
}
