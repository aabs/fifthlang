using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;
using static Fifth.DebugHelpers;

namespace code_generator.InstructionEmitter;

/// <summary>
/// Emits load instructions (ldc, ldloc, ldarg, ldfld, ldsfld, etc.)
/// This is a stateless class that accepts context explicitly.
/// </summary>
public class LoadInstructionEmitter
{
    /// <summary>
    /// Emit a load instruction
    /// </summary>
    /// <param name="il">Instruction encoder to emit to</param>
    /// <param name="loadInst">The load instruction to emit</param>
    /// <param name="context">Emission context containing metadata and state</param>
    public void Emit(InstructionEncoder il, LoadInstruction loadInst, EmissionContext context)
    {
        var opcode = loadInst.Opcode.ToLowerInvariant();
        
        switch (opcode)
        {
            case "ldc.i4":
                EmitLoadConstantI4(il, loadInst);
                break;
            case "ldc.r4":
                EmitLoadConstantR4(il, loadInst);
                break;
            case "ldc.r8":
                EmitLoadConstantR8(il, loadInst);
                break;
            case "ldstr":
                EmitLoadString(il, loadInst, context);
                break;
            case "ldnull":
                EmitLoadNull(il, context);
                break;
            case "ldloc":
                EmitLoadLocal(il, loadInst, context);
                break;
            case "ldarg":
                EmitLoadArgument(il, loadInst, context);
                break;
            case "ldfld":
                EmitLoadField(il, loadInst, context);
                break;
            case "ldsfld":
                EmitLoadStaticField(il, loadInst, context);
                break;
            case "newarr":
                EmitNewArray(il, context);
                break;
            case "ldelem.i4":
                il.OpCode(ILOpCode.Ldelem_i4);
                break;
            case "dup":
                il.OpCode(ILOpCode.Dup);
                break;
            case "box":
                EmitBox(il, loadInst, context);
                break;
            default:
                DebugLog($"WARNING: Unsupported load opcode '{loadInst.Opcode}'");
                break;
        }
    }
    
    private void EmitLoadConstantI4(InstructionEncoder il, LoadInstruction loadInst)
    {
        if (loadInst.Value is int intValue)
        {
            il.LoadConstantI4(intValue);
        }
    }
    
    private void EmitLoadConstantR4(InstructionEncoder il, LoadInstruction loadInst)
    {
        if (loadInst.Value is float floatValue)
        {
            il.LoadConstantR4(floatValue);
        }
    }
    
    private void EmitLoadConstantR8(InstructionEncoder il, LoadInstruction loadInst)
    {
        if (loadInst.Value is double doubleValue)
        {
            il.LoadConstantR8(doubleValue);
        }
    }
    
    private void EmitLoadString(InstructionEncoder il, LoadInstruction loadInst, EmissionContext context)
    {
        if (loadInst.Value is string stringValue)
        {
            // Remove surrounding quotes if present
            var clean = stringValue;
            if (clean.Length >= 2 && clean.StartsWith("\"") && clean.EndsWith("\""))
            {
                clean = clean.Substring(1, clean.Length - 2);
            }
            il.LoadString(context.MetadataBuilder.GetOrAddUserString(clean));
        }
    }
    
    private void EmitLoadNull(InstructionEncoder il, EmissionContext context)
    {
        il.OpCode(ILOpCode.Ldnull);
        var metadata = context.MetadataManager;
        metadata.LastLoadedLocal = null;
        metadata.PendingStackTopClassType = null;
        metadata.LastLoadedParam = null;
    }
    
    private void EmitLoadLocal(InstructionEncoder il, LoadInstruction loadInst, EmissionContext context)
    {
        if (loadInst.Value is not string varName || context.CurrentLocalVarNames == null)
        {
            // Fallback: push default int
            il.LoadConstantI4(0);
            ClearLoadState(context);
            return;
        }
        
        var index = context.CurrentLocalVarNames.IndexOf(varName);
        if (index >= 0)
        {
            il.LoadLocal(index);
            var metadata = context.MetadataManager;
            metadata.LastLoadedLocal = varName;
            
            // Propagate type information if available
            if (metadata.LocalVarClassTypeHandles.TryGetValue(varName, out var tdh))
            {
                metadata.PendingStackTopClassType = tdh;
            }
            else
            {
                metadata.PendingStackTopClassType = null;
            }
            metadata.LastLoadedParam = null;
        }
        else
        {
            DebugLog($"DEBUG: ldloc unknown local '{varName}'. available locals: {string.Join(',', context.CurrentLocalVarNames)}");
            il.LoadConstantI4(0);
            ClearLoadState(context);
        }
    }
    
    private void EmitLoadArgument(InstructionEncoder il, LoadInstruction loadInst, EmissionContext context)
    {
        if (loadInst.Value is not string argName || 
            context.CurrentParamIndexMap == null || 
            !context.CurrentParamIndexMap.TryGetValue(argName, out var argIndex))
        {
            // Fallback: push default int
            il.LoadConstantI4(0);
            ClearLoadState(context);
            return;
        }
        
        il.LoadArgument(argIndex);
        var metadata = context.MetadataManager;
        metadata.LastLoadedLocal = null;
        metadata.LastLoadedParam = argName;
        
        if (metadata.ParamClassTypeHandles.TryGetValue(argName, out var pth))
        {
            metadata.PendingStackTopClassType = pth;
        }
        else
        {
            metadata.PendingStackTopClassType = null;
        }
    }
    
    private void EmitLoadField(InstructionEncoder il, LoadInstruction loadInst, EmissionContext context)
    {
        if (loadInst.Value is not string fieldName)
        {
            return;
        }
        
        var fieldToken = ResolveFieldToken(fieldName, context);
        
        if (fieldToken != null)
        {
            il.OpCode(ILOpCode.Ldfld);
            il.Token(fieldToken.Value);
            PropagateFieldType(fieldToken.Value, context);
            ClearLoadState(context);
        }
        else
        {
            // Unknown field: keep stack balanced
            il.OpCode(ILOpCode.Pop);
            il.LoadConstantI4(0);
            ClearLoadState(context);
            context.MetadataManager.PendingStackTopClassType = null;
            LogFieldResolutionFailure(fieldName, context);
        }
    }
    
    private void EmitLoadStaticField(InstructionEncoder il, LoadInstruction loadInst, EmissionContext context)
    {
        if (loadInst.Value is not string staticFieldToken)
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
            il.OpCode(ILOpCode.Ldsfld);
            il.Token(fieldToken.Value);
        }
        else
        {
            il.OpCode(ILOpCode.Ldnull);
        }
        
        // Clear tracking state
        metadata.LastLoadedLocal = null;
        metadata.LastLoadedParam = null;
        metadata.PendingStackTopClassType = null;
    }
    
    private void EmitNewArray(InstructionEncoder il, EmissionContext context)
    {
        il.OpCode(ILOpCode.Newarr);
        il.Token(context.MetadataManager.SystemInt32TypeRef);
        
        var metadata = context.MetadataManager;
        metadata.LastWasNewobj = true;
        metadata.PendingNewobjTypeName = null;
    }
    
    private void EmitBox(InstructionEncoder il, LoadInstruction loadInst, EmissionContext context)
    {
        if (loadInst.Value is not string boxToken)
        {
            return;
        }
        
        DebugLog($"DEBUG: EmitBox - token='{boxToken}'");
        var bt = boxToken.Trim();
        
        if (string.Equals(bt, "int32", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(bt, "System.Int32", StringComparison.Ordinal))
        {
            il.OpCode(ILOpCode.Box);
            il.Token(context.MetadataManager.SystemInt32TypeRef);
        }
        else
        {
            EmitBoxForCustomType(il, bt, context);
        }
    }
    
    private void EmitBoxForCustomType(InstructionEncoder il, string boxToken, EmissionContext context)
    {
        // Parse assembly and type from token
        string asm = "Fifth.System";
        string fullName = boxToken;
        var atIdx = boxToken.LastIndexOf('@');
        if (atIdx > 0)
        {
            fullName = boxToken.Substring(0, atIdx);
            asm = boxToken.Substring(atIdx + 1);
        }
        
        var typeNs = string.Empty;
        var simpleName = fullName;
        var lastDot = fullName.LastIndexOf('.');
        if (lastDot > 0)
        {
            typeNs = fullName.Substring(0, lastDot);
            simpleName = fullName.Substring(lastDot + 1);
        }
        
        var asmKey = asm.ToLowerInvariant();
        var metadata = context.MetadataManager;
        var metadataBuilder = context.MetadataBuilder;
        
        if (!metadata.TryGetAssemblyReference(asmKey, out var asmRef))
        {
            asmRef = metadataBuilder.AddAssemblyReference(
                metadataBuilder.GetOrAddString(asm),
                new System.Version(1, 0, 0, 0),
                default, default, default, default);
            metadata.RegisterAssemblyReference(asmKey, asmRef);
        }
        
        var trKey = $"{asmKey}|{typeNs}|{simpleName}";
        if (!metadata.TryGetTypeReference(trKey, out var paramTypeRef))
        {
            paramTypeRef = metadataBuilder.AddTypeReference(
                asmRef,
                metadataBuilder.GetOrAddString(typeNs),
                metadataBuilder.GetOrAddString(simpleName));
            metadata.RegisterTypeReference(trKey, paramTypeRef);
        }
        
        il.OpCode(ILOpCode.Box);
        il.Token(paramTypeRef);
    }
    
    // Helper methods
    
    private EntityHandle? ResolveFieldToken(string fieldName, EmissionContext context)
    {
        var metadata = context.MetadataManager;
        
        // Try pending stack top class type first
        if (metadata.PendingStackTopClassType.HasValue && 
            metadata.TryGetTypeName(metadata.PendingStackTopClassType.Value, out var pTypeName))
        {
            var keyDef = $"{pTypeName}::{fieldName}";
            if (metadata.TryGetFieldByTypeAndName(keyDef, out var fdef1))
            {
                DebugLog($"DEBUG: ldfld via pendingStack '{pTypeName}', field '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fdef1)}");
                return fdef1;
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
                DebugLog($"DEBUG: ldfld via lastLoadedLocal '{typeName}', field '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fdef)}");
                return fdef;
            }
        }
        
        // Try last-loaded parameter's class type
        if (!string.IsNullOrEmpty(metadata.LastLoadedParam) && 
            metadata.ParamClassTypeHandles.TryGetValue(metadata.LastLoadedParam, out var pTypeHandle) && 
            metadata.TryGetTypeName(pTypeHandle, out var pName))
        {
            var keyDef = $"{pName}::{fieldName}";
            if (metadata.TryGetFieldByTypeAndName(keyDef, out var fdef2))
            {
                DebugLog($"DEBUG: ldfld via lastLoadedParam '{pName}', field '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fdef2)}");
                return fdef2;
            }
        }
        
        // Fallback by simple name
        if (metadata.TryGetField(fieldName, out var fldHandle))
        {
            DebugLog($"DEBUG: ldfld fallback by simple name '{fieldName}' -> handle {MetadataTokens.GetRowNumber(fldHandle)}");
            return fldHandle;
        }
        
        return null;
    }
    
    private void PropagateFieldType(EntityHandle fieldToken, EmissionContext context)
    {
        var metadata = context.MetadataManager;
        metadata.PendingStackTopClassType = null;
        
        if (fieldToken.Kind == HandleKind.FieldDefinition)
        {
            var fdh = (FieldDefinitionHandle)fieldToken;
            if (metadata.TryGetFieldType(fdh, out var declaredType))
            {
                if (!string.Equals(declaredType.Namespace, "System", StringComparison.Ordinal) && 
                    !string.IsNullOrEmpty(declaredType.Name))
                {
                    if (metadata.TryGetType(declaredType.Name, out var declTypeHandle))
                    {
                        metadata.PendingStackTopClassType = declTypeHandle;
                    }
                }
            }
        }
    }
    
    private void ClearLoadState(EmissionContext context)
    {
        var metadata = context.MetadataManager;
        metadata.LastLoadedLocal = null;
        metadata.LastLoadedParam = null;
    }
    
    private void LogFieldResolutionFailure(string fieldName, EmissionContext context)
    {
        try
        {
            var metadata = context.MetadataManager;
            var candidates = new List<string>();
            if (!string.IsNullOrEmpty(metadata.PendingNewobjTypeName)) 
                candidates.Add($"pendingNewobj={metadata.PendingNewobjTypeName}");
            if (!string.IsNullOrEmpty(metadata.LastLoadedLocal)) 
                candidates.Add($"lastLoadedLocal={metadata.LastLoadedLocal}");
            if (!string.IsNullOrEmpty(metadata.LastLoadedParam)) 
                candidates.Add($"lastLoadedParam={metadata.LastLoadedParam}");
            
            DebugLog($"DEBUG: ldfld could not resolve field '{fieldName}'. Candidates: {string.Join(',', candidates)}");
        }
        catch { }
    }
}
