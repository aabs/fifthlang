using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;
using static Fifth.DebugHelpers;

namespace code_generator.InstructionEmitter;

/// <summary>
/// Emits store instructions (stloc, starg, stfld, stsfld, etc.)
/// This is a stateless class that accepts context explicitly.
/// </summary>
public class StoreInstructionEmitter
{
    /// <summary>
    /// Emit a store instruction
    /// </summary>
    /// <param name="il">Instruction encoder to emit to</param>
    /// <param name="storeInst">The store instruction to emit</param>
    /// <param name="context">Emission context containing metadata and state</param>
    public void Emit(InstructionEncoder il, StoreInstruction storeInst, EmissionContext context)
    {
        var opcode = storeInst.Opcode.ToLowerInvariant();
        
        switch (opcode)
        {
            case "stloc":
                EmitStoreLocal(il, storeInst, context);
                break;
            case "starg":
                // Not fully supported: pop to keep stack balanced
                il.OpCode(ILOpCode.Pop);
                break;
            case "stelem.i4":
                il.OpCode(ILOpCode.Stelem_i4);
                break;
            case "stfld":
                EmitStoreField(il, storeInst, context);
                break;
            case "stsfld":
                EmitStoreStaticField(il, storeInst, context);
                break;
            default:
                DebugLog($"WARNING: Unsupported store opcode '{storeInst.Opcode}'");
                break;
        }
    }
    
    private void EmitStoreLocal(InstructionEncoder il, StoreInstruction storeInst, EmissionContext context)
    {
        if (storeInst.Target is not string varName || context.CurrentLocalVarNames == null)
        {
            // Fallback
            il.StoreLocal(0);
            ClearStoreState(context);
            return;
        }
        
        var index = context.CurrentLocalVarNames.IndexOf(varName);
        if (index >= 0)
        {
            il.StoreLocal(index);
            var metadata = context.MetadataManager;
            
            // Track type information from newobj or field load
            if (metadata.LastWasNewobj)
            {
                metadata.LocalVarTypeMap[varName] = SignatureTypeCode.Object;
                if (!string.IsNullOrEmpty(metadata.PendingNewobjTypeName) && 
                    metadata.TryGetType(metadata.PendingNewobjTypeName, out var tdh))
                {
                    metadata.LocalVarClassTypeHandles[varName] = tdh;
                }
            }
            
            // Propagate type from pending stack top (e.g., from ldfld)
            if (metadata.PendingStackTopClassType.HasValue)
            {
                metadata.LocalVarTypeMap[varName] = SignatureTypeCode.Object;
                metadata.LocalVarClassTypeHandles[varName] = metadata.PendingStackTopClassType.Value;
            }
        }
        else
        {
            // Fallback
            il.StoreLocal(0);
        }
        
        ClearStoreState(context);
    }
    
    private void EmitStoreField(InstructionEncoder il, StoreInstruction storeInst, EmissionContext context)
    {
        if (storeInst.Target is not string fieldName)
        {
            return;
        }
        
        var fieldToken = ResolveFieldToken(fieldName, context);
        
        if (fieldToken != null)
        {
            il.OpCode(ILOpCode.Stfld);
            il.Token(fieldToken.Value);
            
            var metadata = context.MetadataManager;
            if (!string.IsNullOrEmpty(metadata.LastLoadedLocal))
            {
                metadata.LocalVarTypeMap[metadata.LastLoadedLocal] = SignatureTypeCode.Object;
                metadata.LastLoadedLocal = null;
            }
            metadata.LastLoadedParam = null;
        }
        else
        {
            // Unknown field: consume both value and object to keep stack balanced
            il.OpCode(ILOpCode.Pop); // value
            il.OpCode(ILOpCode.Pop); // object
            
            var metadata = context.MetadataManager;
            metadata.LastLoadedLocal = null;
            metadata.LastLoadedParam = null;
        }
    }
    
    private void EmitStoreStaticField(InstructionEncoder il, StoreInstruction storeInst, EmissionContext context)
    {
        if (storeInst.Target is not string staticFieldToken)
        {
            return;
        }
        
        // Parse token: "TypeName::FieldName" or just "FieldName"
        string typeName = string.Empty;
        string fieldName = staticFieldToken;
        var parts = staticFieldToken.Split(new[] { "::" }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            typeName = parts[0];
            fieldName = parts[1];
        }
        
        EntityHandle? fieldToken = null;
        var metadata = context.MetadataManager;
        
        if (!string.IsNullOrEmpty(typeName))
        {
            var key = $"{typeName}::{fieldName}";
            if (metadata.TryGetFieldByTypeAndName(key, out var fh))
            {
                fieldToken = fh;
            }
        }
        
        if (fieldToken == null && metadata.TryGetField(fieldName, out var fldHandle))
        {
            fieldToken = fldHandle;
        }
        
        if (fieldToken != null)
        {
            il.OpCode(ILOpCode.Stsfld);
            il.Token(fieldToken.Value);
        }
        else
        {
            // Unknown static field: pop value to keep stack balanced
            il.OpCode(ILOpCode.Pop);
        }
    }
    
    // Helper methods
    
    private EntityHandle? ResolveFieldToken(string fieldName, EmissionContext context)
    {
        var metadata = context.MetadataManager;
        
        // Try pending stack top class type first
        if (metadata.PendingStackTopClassType.HasValue && 
            metadata.TryGetTypeName(metadata.PendingStackTopClassType.Value, out var pendingTypeName))
        {
            var keyPending = $"{pendingTypeName}::{fieldName}";
            if (metadata.TryGetFieldByTypeAndName(keyPending, out var fdefPending))
            {
                DebugLog($"DEBUG: stfld via pendingStackTop '{pendingTypeName}', field '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fdefPending)}");
                return fdefPending;
            }
        }
        
        // Try pending newobj type
        if (!string.IsNullOrEmpty(metadata.PendingNewobjTypeName) && 
            metadata.TryGetType(metadata.PendingNewobjTypeName, out var newObjTypeHandle) && 
            metadata.TryGetTypeName(newObjTypeHandle, out var newObjTypeName))
        {
            var keyNew = $"{newObjTypeName}::{fieldName}";
            if (metadata.TryGetFieldByTypeAndName(keyNew, out var fdefNew))
            {
                DebugLog($"DEBUG: stfld via pendingNewobj '{newObjTypeName}', field '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fdefNew)}");
                return fdefNew;
            }
        }
        
        // Try last-loaded local's class type
        if (!string.IsNullOrEmpty(metadata.LastLoadedLocal) && 
            metadata.LocalVarClassTypeHandles.TryGetValue(metadata.LastLoadedLocal, out var typeHandle) && 
            metadata.TryGetTypeName(typeHandle, out var typeName))
        {
            var keyDef = $"{typeName}::{fieldName}";
            if (metadata.TryGetFieldByTypeAndName(keyDef, out var fdef))
            {
                DebugLog($"DEBUG: stfld via lastLoadedLocal '{typeName}', field '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fdef)}");
                return fdef;
            }
        }
        
        // Try last-loaded parameter's class type
        if (!string.IsNullOrEmpty(metadata.LastLoadedParam) && 
            metadata.ParamClassTypeHandles.TryGetValue(metadata.LastLoadedParam, out var pTypeHandle) && 
            metadata.TryGetTypeName(pTypeHandle, out var pName))
        {
            var keyPar = $"{pName}::{fieldName}";
            if (metadata.TryGetFieldByTypeAndName(keyPar, out var fdefPar))
            {
                DebugLog($"DEBUG: stfld via lastLoadedParam '{pName}', field '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fdefPar)}");
                return fdefPar;
            }
        }
        
        // Fallback by simple name
        if (metadata.TryGetField(fieldName, out var fldHandle2))
        {
            DebugLog($"DEBUG: stfld fallback by simple name '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fldHandle2)}");
            return fldHandle2;
        }
        
        return null;
    }
    
    private void ClearStoreState(EmissionContext context)
    {
        var metadata = context.MetadataManager;
        metadata.LastWasNewobj = false;
        metadata.PendingNewobjTypeName = null;
        metadata.PendingStackTopClassType = null;
    }
}
