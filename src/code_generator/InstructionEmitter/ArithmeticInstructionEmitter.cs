using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;

namespace code_generator.InstructionEmitter;

/// <summary>
/// Handles emission of arithmetic instructions
/// </summary>
internal class ArithmeticInstructionEmitter
{
    /// <summary>
    /// Emit arithmetic instruction
    /// </summary>
    public void Emit(InstructionEncoder il, ArithmeticInstruction arithInst)
    {
        switch (arithInst.Opcode.ToLowerInvariant())
        {
            case "add":
                il.OpCode(ILOpCode.Add);
                break;
            case "sub":
                il.OpCode(ILOpCode.Sub);
                break;
            case "mul":
                il.OpCode(ILOpCode.Mul);
                break;
            case "div":
                il.OpCode(ILOpCode.Div);
                break;
            case "ceq":
                il.OpCode(ILOpCode.Ceq);
                break;
            case "ceq_neg":
                // Invert equality: ceq -> ldc.i4.0 -> ceq
                il.OpCode(ILOpCode.Ceq);
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ceq);
                break;
            case "clt":
                il.OpCode(ILOpCode.Clt);
                break;
            case "cgt":
                il.OpCode(ILOpCode.Cgt);
                break;
            case "cle":
                // a <= b  ==>  cgt -> not
                il.OpCode(ILOpCode.Cgt);
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ceq);
                break;
            case "cge":
                // a >= b  ==>  clt -> not
                il.OpCode(ILOpCode.Clt);
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ceq);
                break;
            case "nop":
                // nop is a placeholder for unknown unary ops; treat as net 0 in simulation
                break;
            case "and":
                il.OpCode(ILOpCode.And);
                break;
            case "or":
                il.OpCode(ILOpCode.Or);
                break;
            case "xor":
                il.OpCode(ILOpCode.Xor);
                break;
            case "not":
                // Logical not for booleans: compare with 0
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ceq);
                break;
            case "neg":
                il.OpCode(ILOpCode.Neg);
                break;
            case "rem":
                il.OpCode(ILOpCode.Rem);
                break;
            case "shl":
                il.OpCode(ILOpCode.Shl);
                break;
            case "shr":
                il.OpCode(ILOpCode.Shr);
                break;
            case "conv.r8":
                il.OpCode(ILOpCode.Conv_r8);
                break;
            case "conv.i4":
                il.OpCode(ILOpCode.Conv_i4);
                break;
        }
    }
}
