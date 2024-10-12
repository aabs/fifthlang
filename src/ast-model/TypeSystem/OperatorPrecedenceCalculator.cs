using static ast_model.TypeSystem.Maybe<ast.TypeCoertionSeniority>;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace ast_model.TypeSystem;

public static class OperatorPrecedenceCalculator
{
    /// <summary>
    ///     Work out the result of an operation, and indicate what coercions if any are required to perform it
    /// </summary>
    /// <param name="op">the operation to perform</param>
    /// <param name="lhs">the tid of the lhs operand</param>
    /// <param name="rhs">the tid of the rhs operand</param>
    /// <returns>tuple (result tid, coercion type for lhs, coercion type for rhs)</returns>
    public static (TypeId, TypeId?, TypeId?) GetResultType(Operator? op, TypeId lhs, TypeId rhs)
    {
        if (!op.HasValue)
        {
            throw new ArgumentNullException(nameof(op));
        }

        return GetResultType(op.Value, lhs, rhs);
    }
    public static (TypeId, TypeId?, TypeId?) GetResultType(Operator op, TypeId lhs, TypeId rhs)
    {
        if (IsRelational(op) && TypeRegistry.DefaultRegistry.TryLookupTypeId(typeof(bool), out var tid))
        {
            return (tid, null, null);
        }

        if (lhs == rhs)
        {
            return (lhs, null, null);
        }

        var lhsSeniority = GetSeniority(lhs);
        var rhsSeniority = GetSeniority(rhs);

        if (lhsSeniority is Some lhss && rhsSeniority is Some rhss)
        {

                if ((ushort)lhss.Value > (ushort)rhss.Value)
                {
                    return (lhs, null, lhs);
                }

                return (rhs, rhs, null);
        }

        throw new TypeCheckingException("could not resolve numerical types ofr coercion");
    }

    public static Maybe<TypeCoertionSeniority> GetSeniority(TypeId tid)
    {
        if (TypeRegistry.DefaultRegistry.TryLookupType(tid, out var fifthType) &&
            fifthType is FifthType.NetType netType &&
            TypeRegistry.NumericPrimitive.TryGetValue(netType.TheType, out var seniority))
        {
            return new Some(seniority);
        }

        return new None();
    }

    private static bool IsRelational(Operator op)
    {
        return op switch
        {
            Operator.LogicalAnd => true,
            Operator.LogicalOr => true,
            Operator.LogicalNot => true,
            Operator.LogicalNand => true,
            Operator.LogicalNor => true,
            Operator.LogicalXor => true,
            Operator.ArithmeticEqual => true,
            Operator.ArithmeticNotEqual => true,
            Operator.ArithmeticLessThan => true,
            Operator.ArithmeticGreaterThan => true,
            Operator.ArithmeticLessThanOrEqual => true,
            Operator.ArithmeticGreaterThanOrEqual => true,
            _ => false
        };
    }

    public static bool IsNumeric(this FifthType ft)
    {
        return ft.MatchNetType<bool>(
            type => TypeRegistry.NumericPrimitive.ContainsKey(type.TheType),
            () => false
        );
    }

    private static bool IsNumerical(Operator op)
    {
        return !IsRelational(op);
    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
