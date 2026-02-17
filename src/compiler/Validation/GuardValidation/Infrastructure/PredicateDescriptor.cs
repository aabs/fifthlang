using ast;

namespace compiler.Validation.GuardValidation.Infrastructure;

/// <summary>
/// Describes a predicate for analysis, containing its type classification
/// and the normalized constraint expressions.
/// </summary>
internal class PredicateDescriptor
{
    public static readonly PredicateDescriptor Always = new(PredicateType.Base, new List<Expression>());
    public static readonly PredicateDescriptor Unknown = new(PredicateType.Unknown, new List<Expression>());

    public PredicateType Type { get; }
    public List<Expression> Constraints { get; }

    public PredicateDescriptor(PredicateType type, List<Expression> constraints)
    {
        Type = type;
        Constraints = constraints;
    }
}