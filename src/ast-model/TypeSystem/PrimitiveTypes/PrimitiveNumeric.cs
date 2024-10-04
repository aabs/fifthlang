namespace ast_model.TypeSystem.PrimitiveTypes;

/// <summary>
/// Priority of types for working out coercions and operator result types
/// </summary>
/// <remarks>
/// 0: short
/// 1: integer
/// 2: long
/// 3: float
/// 4: double
/// 5: decimal
/// </remarks>
public abstract class PrimitiveNumeric : PrimitiveAny
{
    public TypeCoertionSeniority Seniority { get; init; }
}
