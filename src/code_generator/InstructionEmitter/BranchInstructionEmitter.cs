using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;

namespace code_generator.InstructionEmitter;

/// <summary>
/// Handles emission of branch instructions
/// </summary>
internal class BranchInstructionEmitter
{
    /// <summary>
    /// Emit branch instruction
    /// </summary>
    public void Emit(InstructionEncoder il, BranchInstruction branchInst, Dictionary<string, LabelHandle>? labelMap)
    {
        if (labelMap == null || string.IsNullOrEmpty(branchInst.TargetLabel) || !labelMap.TryGetValue(branchInst.TargetLabel, out var target))
        {
            Console.WriteLine("WARNING: Branch target label not found; skipping branch emission.");
            return;
        }
        switch ((branchInst.Opcode ?? string.Empty).ToLowerInvariant())
        {
            case "br": il.Branch(ILOpCode.Br, target); break;
            case "brtrue": il.Branch(ILOpCode.Brtrue, target); break;
            case "brfalse": il.Branch(ILOpCode.Brfalse, target); break;
            default:
                Console.WriteLine($"WARNING: Unsupported branch opcode '{branchInst.Opcode}', emitting unconditional br");
                il.Branch(ILOpCode.Br, target);
                break;
        }
    }
}
