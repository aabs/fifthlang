using il_ast;
using System.Reflection;

namespace code_generator.Emit;

/// <summary>
/// Emits constants and default values to instruction sequences
/// </summary>
public static class ConstantEmitter
{
    /// <summary>
    /// Emits the default value for a parameter (using its DefaultValue if available)
    /// </summary>
    public static void EmitDefaultForParameter(InstructionSequence sequence, ParameterInfo parameter)
    {
        object? defaultValue = null;
        try
        {
            defaultValue = parameter.DefaultValue;
        }
        catch
        {
            defaultValue = null;
        }

        if (defaultValue == System.DBNull.Value || defaultValue == System.Reflection.Missing.Value)
        {
            defaultValue = null;
        }

        if (defaultValue != null)
        {
            EmitConstant(sequence, parameter.ParameterType, defaultValue);
        }
        else
        {
            EmitDefaultForType(sequence, parameter.ParameterType);
        }
    }

    /// <summary>
    /// Emits the default value for a type (0, null, etc.)
    /// </summary>
    public static void EmitDefaultForType(InstructionSequence sequence, Type parameterType)
    {
        if (parameterType == typeof(void))
        {
            return;
        }

        if (!parameterType.IsValueType)
        {
            sequence.Add(new LoadInstruction("ldnull"));
            return;
        }

        if (parameterType == typeof(bool))
        {
            sequence.Add(new LoadInstruction("ldc.i4", 0));
            return;
        }

        if (parameterType == typeof(float))
        {
            sequence.Add(new LoadInstruction("ldc.r4", 0f));
            return;
        }

        if (parameterType == typeof(double))
        {
            sequence.Add(new LoadInstruction("ldc.r8", 0d));
            return;
        }

        if (parameterType == typeof(decimal))
        {
            sequence.Add(new LoadInstruction("ldstr", 0m.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=Decimal;Method=Parse;Params=System.String;Return=System.Decimal") { ArgCount = 1 });
            return;
        }

        sequence.Add(new LoadInstruction("ldc.i4", 0));
    }

    /// <summary>
    /// Emits a constant value with appropriate type handling
    /// </summary>
    public static void EmitConstant(InstructionSequence sequence, Type targetType, object? value)
    {
        if (targetType == typeof(string))
        {
            sequence.Add(new LoadInstruction("ldstr", value?.ToString() ?? string.Empty));
            return;
        }

        if (targetType == typeof(bool))
        {
            var boolVal = value is bool b && b ? 1 : 0;
            sequence.Add(new LoadInstruction("ldc.i4", boolVal));
            return;
        }

        if (targetType == typeof(float) || targetType == typeof(double))
        {
            var dbl = value != null ? Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture) : 0d;
            if (targetType == typeof(float))
            {
                sequence.Add(new LoadInstruction("ldc.r4", (float)dbl));
            }
            else
            {
                sequence.Add(new LoadInstruction("ldc.r8", dbl));
            }
            return;
        }

        if (targetType == typeof(decimal))
        {
            var decVal = value is decimal dec ? dec : Convert.ToDecimal(value ?? 0m, System.Globalization.CultureInfo.InvariantCulture);
            sequence.Add(new LoadInstruction("ldstr", decVal.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=Decimal;Method=Parse;Params=System.String;Return=System.Decimal") { ArgCount = 1 });
            return;
        }

        if (targetType == typeof(char))
        {
            var charVal = value is char c ? c : Convert.ToChar(value ?? '\0', System.Globalization.CultureInfo.InvariantCulture);
            sequence.Add(new LoadInstruction("ldc.i4", (int)charVal));
            return;
        }

        if (targetType.IsEnum)
        {
            var enumUnderlying = value != null ? Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture) : 0;
            sequence.Add(new LoadInstruction("ldc.i4", enumUnderlying));
            return;
        }

        if (targetType == typeof(long) || targetType == typeof(ulong) || 
            targetType == typeof(uint) || targetType == typeof(int) || 
            targetType == typeof(short) || targetType == typeof(byte) || 
            targetType == typeof(sbyte) || targetType == typeof(ushort))
        {
            var intValue = value != null ? Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture) : 0L;
            if (intValue < int.MinValue || intValue > int.MaxValue)
            {
                intValue = 0;
            }
            sequence.Add(new LoadInstruction("ldc.i4", (int)intValue));
            return;
        }

        if (!targetType.IsValueType)
        {
            sequence.Add(new LoadInstruction("ldnull"));
            return;
        }

        sequence.Add(new LoadInstruction("ldc.i4", 0));
    }
}
