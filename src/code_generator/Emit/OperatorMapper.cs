using ast;

namespace code_generator.Emit;

/// <summary>
/// Maps Fifth language operators to IL opcodes, eliminating string-based operator handling.
/// </summary>
public static class OperatorMapper
{
    /// <summary>
    /// Maps a binary operator to its IL opcode
    /// </summary>
    public static string GetBinaryOpCode(Operator op)
    {
        return op switch
        {
            Operator.ArithmeticAdd => "add",
            Operator.ArithmeticSubtract => "sub",
            Operator.ArithmeticMultiply => "mul",
            Operator.ArithmeticDivide => "div",
            Operator.ArithmeticRem => "rem",
            Operator.ArithmeticMod => "rem", // IL uses rem for modulo
            Operator.ArithmeticPow => "pow", // Will need special handling via Math.Pow
            Operator.Equal => "ceq",
            Operator.NotEqual => "ceq_neg", // composite: ceq + ldc.i4.0 + ceq
            Operator.LessThan => "clt",
            Operator.GreaterThan => "cgt",
            Operator.LessThanOrEqual => "cle", // composite: cgt + ldc.i4.0 + ceq
            Operator.GreaterThanOrEqual => "cge", // composite: clt + ldc.i4.0 + ceq
            Operator.LogicalAnd => "and",
            Operator.LogicalOr => "or",
            Operator.BitwiseOr => "or",
            Operator.BitwiseAnd => "and",
            Operator.LogicalXor => "xor",
            Operator.BitwiseLeftShift => "shl",
            Operator.BitwiseRightShift => "shr",
            _ => "nop"
        };
    }

    /// <summary>
    /// Maps a unary operator to its IL opcode
    /// </summary>
    public static string GetUnaryOpCode(Operator op)
    {
        return op switch
        {
            Operator.ArithmeticNegative => "neg",
            Operator.LogicalNot => "not", // composite: ldc.i4.0 + ceq
            _ => "nop"
        };
    }

    /// <summary>
    /// Checks if the operator is a comparison operator that returns bool
    /// </summary>
    public static bool IsComparisonOperator(Operator op)
    {
        return op switch
        {
            Operator.Equal => true,
            Operator.NotEqual => true,
            Operator.LessThan => true,
            Operator.GreaterThan => true,
            Operator.LessThanOrEqual => true,
            Operator.GreaterThanOrEqual => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks if the operator is a logical operator (&&, ||, xor)
    /// </summary>
    public static bool IsLogicalOperator(Operator op)
    {
        return op switch
        {
            Operator.LogicalAnd => true,
            Operator.LogicalOr => true,
            Operator.LogicalXor => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks if the operator requires composite instruction generation (multiple IL ops)
    /// </summary>
    public static bool IsCompositeOperator(Operator op)
    {
        return op switch
        {
            Operator.NotEqual => true,       // ceq + ldc.i4.0 + ceq
            Operator.LessThanOrEqual => true, // cgt + ldc.i4.0 + ceq
            Operator.GreaterThanOrEqual => true, // clt + ldc.i4.0 + ceq
            Operator.LogicalNot => true,     // ldc.i4.0 + ceq
            Operator.ArithmeticPow => true,  // call Math.Pow
            _ => false
        };
    }
    
    /// <summary>
    /// Checks if the operator requires an external method call (e.g., Math.Pow)
    /// </summary>
    public static bool RequiresExternalCall(Operator op)
    {
        return op switch
        {
            Operator.ArithmeticPow => true,
            _ => false
        };
    }
}
