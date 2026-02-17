using ast;

namespace compiler.Validation.GuardValidation.Infrastructure;

/// <summary>
/// Represents an analyzed overload with its predicate classification and descriptor.
/// </summary>
internal record AnalyzedOverload(
    IOverloadableFunction Overload,
    PredicateType PredicateType,
    PredicateDescriptor PredicateDescriptor
);