namespace compiler.Validation.GuardValidation.Infrastructure;

/// <summary>
/// Classification of overload predicates for analysis purposes.
/// </summary>
internal enum PredicateType
{
    /// <summary>
    /// No constraints or tautology (always true).
    /// </summary>
    Base,

    /// <summary>
    /// Can be normalized to conjunction of atomic predicates.
    /// </summary>
    Analyzable,

    /// <summary>
    /// Cannot be analyzed due to complexity or unknown expressions.
    /// </summary>
    Unknown
}