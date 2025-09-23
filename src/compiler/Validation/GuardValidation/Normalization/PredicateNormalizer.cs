using ast;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Normalization;

/// <summary>
/// Handles predicate analysis, classification, and normalization for guard clauses.
/// Implements the logic for determining if predicates are Base, Analyzable, or Unknown,
/// and converts them to normalized forms for analysis.
/// </summary>
internal class PredicateNormalizer
{
    /// <summary>
    /// Analyzes an overload to create an AnalyzedOverload with classified predicate.
    /// </summary>
    public AnalyzedOverload AnalyzeOverload(IOverloadableFunction overload)
    {
        var predicateType = ClassifyPredicate(overload);
        var predicateDescriptor = CreatePredicateDescriptor(overload, predicateType);

        return new AnalyzedOverload(overload, predicateType, predicateDescriptor);
    }

    /// <summary>
    /// Classifies a predicate as Base, Analyzable, or Unknown.
    /// </summary>
    public PredicateType ClassifyPredicate(IOverloadableFunction overload)
    {
        // Check if it's a base case (no constraints)
        if (!HasAnyConstraints(overload))
        {
            return PredicateType.Base;
        }

        // Try to normalize constraints
        var constraints = GetAllConstraints(overload);
        if (constraints.Count == 0)
        {
            return PredicateType.Base;
        }

        // Check for tautology (FR-052/055)
        if (constraints.Count == 1 && IsTautology(constraints[0]))
        {
            return PredicateType.Base;
        }

        // Attempt normalization per FR-038
        if (CanNormalizeToConjunction(constraints))
        {
            return PredicateType.Analyzable;
        }

        return PredicateType.Unknown;
    }

    /// <summary>
    /// Creates a predicate descriptor for the given overload and type.
    /// </summary>
    public PredicateDescriptor CreatePredicateDescriptor(IOverloadableFunction overload, PredicateType type)
    {
        return type switch
        {
            PredicateType.Base => PredicateDescriptor.Always,
            PredicateType.Unknown => PredicateDescriptor.Unknown,
            PredicateType.Analyzable => CreateAnalyzableDescriptor(overload),
            _ => PredicateDescriptor.Unknown
        };
    }

    private bool HasAnyConstraints(IOverloadableFunction overload)
    {
        return overload.Params.Any(p => p.ParameterConstraint != null);
    }

    private List<Expression> GetAllConstraints(IOverloadableFunction overload)
    {
        var constraints = new List<Expression>();
        foreach (var param in overload.Params)
        {
            if (param.ParameterConstraint != null)
            {
                constraints.Add(param.ParameterConstraint);
            }
        }
        return constraints;
    }

    private bool IsTautology(Expression expr)
    {
        // FR-055: Limited to literal true, parenthesized true, or compile-time boolean constant
        return expr switch
        {
            BooleanLiteralExp { Value: true } => true,
            // TODO: Handle parenthesized true and compile-time constants
            _ => false
        };
    }

    private bool CanNormalizeToConjunction(List<Expression> constraints)
    {
        // FR-038: Check if all constraints can be normalized to analyzable form
        foreach (var constraint in constraints)
        {
            if (!IsAnalyzableConstraint(constraint))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsAnalyzableConstraint(Expression expr)
    {
        // FR-038: Analyzable patterns
        return expr switch
        {
            BinaryExp { Operator: Operator.Equal } => true, // equality
            BinaryExp
            {
                Operator: Operator.LessThan or Operator.LessThanOrEqual or
                                    Operator.GreaterThan or Operator.GreaterThanOrEqual
            } => true, // comparisons
            BinaryExp { Operator: Operator.LogicalAnd } be =>
                IsAnalyzableConstraint(be.LHS) && IsAnalyzableConstraint(be.RHS), // conjunction
            // TODO: Add more patterns like destructuring field bindings
            _ => false
        };
    }

    private PredicateDescriptor CreateAnalyzableDescriptor(IOverloadableFunction overload)
    {
        // TODO: Implement detailed predicate analysis
        // For now, return a simple conjunction placeholder
        var constraints = GetAllConstraints(overload);
        return new PredicateDescriptor(PredicateType.Analyzable, constraints);
    }
}