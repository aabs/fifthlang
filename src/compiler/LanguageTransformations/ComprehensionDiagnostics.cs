namespace compiler;

/// <summary>
/// Diagnostic codes for list comprehension validation errors and warnings.
/// These codes follow the format "LCOMP00X: message text" for comprehension-specific issues.
/// </summary>
/// <remarks>
/// Code allocation:
/// - LCOMP001-003: Type and structure validation
/// - LCOMP004-006: Projection and constraint validation
/// - LCOMP007: Legacy syntax rejection
/// </remarks>
public static class ComprehensionDiagnostics
{
    /// <summary>
    /// LCOMP001: Invalid generator type.
    /// Emitted when the comprehension generator (source) does not typecheck to a list or tabular SELECT result.
    /// Example: "LCOMP001: Comprehension generator must be a list or tabular SELECT result, got 'int'"
    /// </summary>
    public const string InvalidGeneratorType = "LCOMP001";

    /// <summary>
    /// LCOMP002: Non-SELECT SPARQL query in generator.
    /// Emitted when generator expression is (or contains) a SPARQL literal whose parsed form is not SELECT.
    /// Example: "LCOMP002: SPARQL comprehension requires a SELECT query, got ASK query"
    /// </summary>
    public const string NonSelectQuery = "LCOMP002";

    /// <summary>
    /// LCOMP003: Unknown SPARQL variable.
    /// Emitted when object projection uses ?varName that is not projected by the SELECT clause.
    /// Example: "LCOMP003: SPARQL variable '?age' not found in SELECT projection; available: ?name, ?id"
    /// </summary>
    public const string UnknownSparqlVariable = "LCOMP003";

    /// <summary>
    /// LCOMP004: Non-boolean constraint.
    /// Emitted when a 'where' constraint expression does not typecheck to boolean.
    /// Example: "LCOMP004: Constraint expression must be boolean, got 'int'"
    /// </summary>
    public const string NonBooleanConstraint = "LCOMP004";

    /// <summary>
    /// LCOMP005: Invalid object projection binding.
    /// Emitted when in a SPARQL object projection, a property initializer RHS is not a SPARQL variable token ?varName.
    /// Example: "LCOMP005: SPARQL comprehension property value must be a SPARQL variable (?varName), got literal"
    /// </summary>
    public const string InvalidObjectProjectionBinding = "LCOMP005";

    /// <summary>
    /// LCOMP006: Unknown property in projection.
    /// Emitted when projection references a property that does not exist on the projected type.
    /// Example: "LCOMP006: Property 'Age' not found on type 'Person'"
    /// </summary>
    public const string UnknownProperty = "LCOMP006";

    /// <summary>
    /// LCOMP007: Legacy comprehension syntax rejected.
    /// Emitted when parser encounters 'in'/'#' comprehension syntax that must be migrated to 'from'/'where'.
    /// This may surface as a parse error rather than a compiler diagnostic.
    /// Example: "LCOMP007: Legacy comprehension syntax rejected; use 'from'/'where' instead of 'in'/'#'"
    /// </summary>
    public const string LegacySyntaxRejected = "LCOMP007";

    /// <summary>
    /// Creates a formatted diagnostic message for invalid generator type.
    /// </summary>
    public static string FormatInvalidGeneratorType(string actualType)
        => $"{InvalidGeneratorType}: Comprehension generator must be a list or tabular SELECT result, got '{actualType}'";

    /// <summary>
    /// Creates a formatted diagnostic message for non-SELECT SPARQL query.
    /// </summary>
    public static string FormatNonSelectQuery(string queryType)
        => $"{NonSelectQuery}: SPARQL comprehension requires a SELECT query, got {queryType} query";

    /// <summary>
    /// Creates a formatted diagnostic message for unknown SPARQL variable.
    /// </summary>
    public static string FormatUnknownSparqlVariable(string variable, string availableVariables)
        => $"{UnknownSparqlVariable}: SPARQL variable '{variable}' not found in SELECT projection; available: {availableVariables}";

    /// <summary>
    /// Creates a formatted diagnostic message for non-boolean constraint.
    /// </summary>
    public static string FormatNonBooleanConstraint(string actualType)
        => $"{NonBooleanConstraint}: Constraint expression must be boolean, got '{actualType}'";

    /// <summary>
    /// Creates a formatted diagnostic message for invalid object projection binding.
    /// </summary>
    public static string FormatInvalidObjectProjectionBinding()
        => $"{InvalidObjectProjectionBinding}: SPARQL comprehension property value must be a SPARQL variable (?varName), got literal or expression";

    /// <summary>
    /// Creates a formatted diagnostic message for unknown property.
    /// </summary>
    public static string FormatUnknownProperty(string propertyName, string typeName)
        => $"{UnknownProperty}: Property '{propertyName}' not found on type '{typeName}'";

    /// <summary>
    /// Creates a formatted diagnostic message for legacy syntax rejection.
    /// </summary>
    public static string FormatLegacySyntaxRejected()
        => $"{LegacySyntaxRejected}: Legacy comprehension syntax rejected; use 'from'/'where' instead of 'in'/'#'. See migration guide at specs/015-sparql-comprehensions/migration.md";
}
