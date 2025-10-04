using ast;

namespace code_generator.Emit;

/// <summary>
/// Infers System.Type from Fifth AST expressions for type-safe method resolution and code generation.
/// </summary>
public class TypeInference
{
    private readonly EmitContext _context;

    public TypeInference(EmitContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Infers the System.Type of an AST expression using available context
    /// </summary>
    public Type? InferExpressionType(Expression? expr)
    {
        if (expr == null) return null;

        return expr switch
        {
            Int32LiteralExp => typeof(int),
            StringLiteralExp => typeof(string),
            BooleanLiteralExp => typeof(bool),
            Float4LiteralExp => typeof(float),
            Float8LiteralExp => typeof(double),
            Float16LiteralExp => typeof(decimal),
            VarRefExp v => InferVarRefType(v),
            BinaryExp be => InferBinaryExpType(be),
            UnaryExp ue => InferUnaryExpType(ue),
            MemberAccessExp ma => InferMemberAccessType(ma),
            FuncCallExp fc => InferFuncCallType(fc),
            _ => null
        };
    }

    private Type? InferVarRefType(VarRefExp varRef)
    {
        if (_context.LocalVariableTypes.TryGetValue(varRef.VarName, out var localType))
        {
            return localType;
        }
        if (_context.ParameterTypes.TryGetValue(varRef.VarName, out var paramType))
        {
            return paramType;
        }
        return null;
    }

    private Type? InferBinaryExpType(BinaryExp be)
    {
        var leftType = be.LHS != null ? InferExpressionType(be.LHS) : null;
        var rightType = be.RHS != null ? InferExpressionType(be.RHS) : null;

        // Comparison/logical operators always return bool
        switch (be.Operator)
        {
            case Operator.Equal:
            case Operator.NotEqual:
            case Operator.LessThan:
            case Operator.GreaterThan:
            case Operator.LessThanOrEqual:
            case Operator.GreaterThanOrEqual:
            case Operator.LogicalAnd:
            case Operator.LogicalOr:
            case Operator.LogicalXor:
                return typeof(bool);
        }

        // Addition: string concatenation if either operand is string
        if (be.Operator == Operator.ArithmeticAdd && (leftType == typeof(string) || rightType == typeof(string)))
        {
            return typeof(string);
        }

        // Numeric promotion for arithmetic operators
        if (leftType != null && rightType != null && IsNumeric(leftType) && IsNumeric(rightType))
        {
            return PromoteNumeric(leftType, rightType);
        }

        return null;
    }

    private Type? InferUnaryExpType(UnaryExp ue)
    {
        var operandType = InferExpressionType(ue.Operand);
        return ue.Operator switch
        {
            Operator.ArithmeticNegative => operandType, // preserve numeric type
            Operator.LogicalNot => typeof(bool),
            _ => operandType
        };
    }

    private Type? InferMemberAccessType(MemberAccessExp ma)
    {
        // If RHS is a function call, try to infer its return type
        if (ma.RHS is FuncCallExp callExp)
        {
            var qualifierType = ma.LHS != null ? InferExpressionType(ma.LHS) : null;
            
            // Check for external type annotation
            if (callExp.Annotations != null && 
                callExp.Annotations.TryGetValue("ExternalType", out var extTypeObj) && 
                extTypeObj is Type extType)
            {
                var methodName = ExternalMethodResolver.ExtractExternalMethodName(callExp);
                var args = callExp.InvocationArguments ?? new List<Expression>();
                var resolver = new ExternalMethodResolver();
                var chosen = resolver.ResolveExternalMethod(this, extType, methodName, args, qualifierType);
                if (chosen != null)
                {
                    return chosen.ReturnType;
                }
            }

            // Check function def return type
            if (callExp.FunctionDef?.ReturnType?.Name.Value is string funcReturn)
            {
                return TypeMapper.MapBuiltinFifthTypeNameToSystem(funcReturn);
            }

            return null;
        }

        // If RHS is a var ref, try reflection on LHS type
        if (ma.RHS is VarRefExp memberRef)
        {
            var lhsType = ma.LHS != null ? InferExpressionType(ma.LHS) : null;
            if (lhsType != null)
            {
                try
                {
                    var member = lhsType.GetMember(memberRef.VarName).FirstOrDefault();
                    if (member is System.Reflection.PropertyInfo prop) return prop.PropertyType;
                    if (member is System.Reflection.FieldInfo field) return field.FieldType;
                    if (member is System.Reflection.MethodInfo method) return method.ReturnType;
                }
                catch
                {
                    // Reflection lookups are best-effort; ignore failures
                }
            }
            return null;
        }

        // Try to infer from RHS
        if (ma.RHS != null)
        {
            return InferExpressionType(ma.RHS);
        }

        return ma.LHS != null ? InferExpressionType(ma.LHS) : null;
    }

    private Type? InferFuncCallType(FuncCallExp fc)
    {
        if (fc.FunctionDef?.ReturnType?.Name.Value is string returnTypeName)
        {
            return TypeMapper.MapBuiltinFifthTypeNameToSystem(returnTypeName);
        }
        return null;
    }

    /// <summary>
    /// Determines if a type is numeric (supports arithmetic operations)
    /// </summary>
    public static bool IsNumeric(Type t)
    {
        return t == typeof(int) || t == typeof(long) || t == typeof(float) || 
               t == typeof(double) || t == typeof(decimal);
    }

    /// <summary>
    /// Promotes two numeric types to their common type for arithmetic operations
    /// </summary>
    public static Type PromoteNumeric(Type a, Type b)
    {
        if (a == typeof(double) || b == typeof(double)) return typeof(double);
        if (a == typeof(float) || b == typeof(float)) return typeof(float);
        if (a == typeof(long) || b == typeof(long)) return typeof(long);
        if (a == typeof(decimal) || b == typeof(decimal)) return typeof(decimal);
        return typeof(int);
    }

    /// <summary>
    /// Checks if implicit numeric widening is allowed from src to dest type
    /// </summary>
    public static bool IsImplicitNumericWidening(Type src, Type dest)
    {
        if (src == typeof(int))
        {
            return dest == typeof(long) || dest == typeof(float) || 
                   dest == typeof(double) || dest == typeof(decimal);
        }
        if (src == typeof(float))
        {
            return dest == typeof(double);
        }
        if (src == typeof(long))
        {
            return dest == typeof(float) || dest == typeof(double) || dest == typeof(decimal);
        }
        return false;
    }
}
