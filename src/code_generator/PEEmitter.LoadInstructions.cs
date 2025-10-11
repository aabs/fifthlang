using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;
using static Fifth.DebugHelpers;

namespace code_generator;

/// <summary>
/// Partial class for PEEmitter - Load instruction emission
/// </summary>
public partial class PEEmitter
{
    /// <summary>
    /// Emit load instruction
    /// </summary>
    private void EmitLoadInstruction(InstructionEncoder il, il_ast.LoadInstruction loadInst, MetadataBuilder metadataBuilder, List<string>? localVarNames = null, Dictionary<string, int>? paramIndexMap = null)
    {
        switch (loadInst.Opcode.ToLowerInvariant())
        {
            case "ldc.i4":
                if (loadInst.Value is int intValue)
                {
                    il.LoadConstantI4(intValue);
                }
                break;
            case "newarr":
                il.OpCode(ILOpCode.Newarr);
                // Use the type from the instruction if available, otherwise fallback to Int32
                EntityHandle arrayElementTypeHandle = _systemInt32TypeRef;
                if (loadInst.Value is string arrayTypeName && !string.IsNullOrEmpty(arrayTypeName))
                {
                    // Try to get the type handle for the array element type
                    if (_typeHandles.TryGetValue(arrayTypeName, out var typeHandle))
                    {
                        arrayElementTypeHandle = typeHandle;
                        DebugLog($"DEBUG: newarr using type '{arrayTypeName}' -> handle {MetadataTokens.GetRowNumber(typeHandle)}");
                    }
                    else
                    {
                        DebugLog($"DEBUG: newarr could not resolve type '{arrayTypeName}', falling back to System.Int32");
                    }
                }
                il.Token(arrayElementTypeHandle);
                _lastWasNewobj = true;
                _pendingNewobjTypeName = null;
                break;
            case "ldelem.i4":
                il.OpCode(ILOpCode.Ldelem_i4);
                break;
            case "ldelem.ref":
                il.OpCode(ILOpCode.Ldelem_ref);
                // After loading an array element, propagate the element type to stack top
                if (_pendingArrayElementType.HasValue)
                {
                    _pendingStackTopClassType = _pendingArrayElementType.Value;
                    DebugLog($"DEBUG: ldelem.ref propagated element type to stack top");
                }
                _pendingArrayElementType = null;
                break;
            case "ldc.r4":
                if (loadInst.Value is float floatValue)
                {
                    il.LoadConstantR4(floatValue);
                }
                break;
            case "ldc.r8":
                if (loadInst.Value is double doubleValue)
                {
                    il.LoadConstantR8(doubleValue);
                }
                break;
            case "ldstr":
                if (loadInst.Value is string stringValue)
                {
                    var clean = stringValue;
                    if (clean.Length >= 2 && clean.StartsWith("\"") && clean.EndsWith("\""))
                    {
                        clean = clean.Substring(1, clean.Length - 2);
                    }
                    il.LoadString(metadataBuilder.GetOrAddUserString(clean));
                }
                break;
            case "ldnull":
                il.OpCode(ILOpCode.Ldnull);
                _lastLoadedLocal = null;
                _pendingStackTopClassType = null;
                _pendingArrayElementType = null;
                _lastLoadedParam = null;
                break;
            case "ldloc":
                EmitLoadLocal(il, loadInst, localVarNames);
                break;
            case "ldarg":
                EmitLoadArgument(il, loadInst, paramIndexMap);
                break;
            case "ldfld":
                EmitLoadField(il, loadInst);
                break;
            case "ldsfld":
                EmitLoadStaticField(il, loadInst);
                break;
            case "dup":
                il.OpCode(ILOpCode.Dup);
                break;
            case "box":
                EmitBox(il, loadInst, metadataBuilder);
                break;
        }
    }

    private void EmitLoadLocal(InstructionEncoder il, LoadInstruction loadInst, List<string>? localVarNames)
    {
        if (loadInst.Value is string varName && localVarNames != null)
        {
            var index = localVarNames.IndexOf(varName);
            if (index >= 0)
            {
                il.LoadLocal(index);
                _lastLoadedLocal = varName;
                if (_localVarClassTypeHandles.TryGetValue(varName, out var tdh))
                {
                    _pendingStackTopClassType = tdh;
                }
                else
                {
                    _pendingStackTopClassType = null;
                    _pendingArrayElementType = null;
                }
                _lastLoadedParam = null;
            }
            else
            {
                DebugLog($"DEBUG: ldloc unknown local '{varName}'. available locals: {string.Join(',', localVarNames)}");
                il.LoadConstantI4(0);
                _lastLoadedLocal = null;
                _pendingStackTopClassType = null;
                _pendingArrayElementType = null;
                _lastLoadedParam = null;
            }
        }
        else
        {
            il.LoadConstantI4(0);
            _lastLoadedLocal = null;
            _pendingStackTopClassType = null;
            _lastLoadedParam = null;
        }
    }

    private void EmitLoadArgument(InstructionEncoder il, LoadInstruction loadInst, Dictionary<string, int>? paramIndexMap)
    {
        if (loadInst.Value is string argName && paramIndexMap != null && paramIndexMap.TryGetValue(argName, out var argIndex))
        {
            il.LoadArgument(argIndex);
            _lastLoadedLocal = null;
            _lastLoadedParam = argName;
            if (_paramClassTypeHandles.TryGetValue(argName, out var pth))
            {
                _pendingStackTopClassType = pth;
            }
            else
            {
                _pendingStackTopClassType = null;
            }
        }
        else
        {
            il.LoadConstantI4(0);
            _lastLoadedLocal = null;
            _lastLoadedParam = null;
            _pendingArrayElementType = null;
        }
    }

    private void EmitLoadField(InstructionEncoder il, LoadInstruction loadInst)
    {
        if (loadInst.Value is string fldName)
        {
            EntityHandle? fieldToken = ResolveFieldToken(fldName);

            if (fieldToken != null)
            {
                il.OpCode(ILOpCode.Ldfld);
                il.Token(fieldToken.Value);
                PropagateFieldType(fieldToken.Value);
                _lastLoadedLocal = null;
                _lastLoadedParam = null;
            }
            else
            {
                il.OpCode(ILOpCode.Pop);
                il.LoadConstantI4(0);
                _lastLoadedLocal = null;
                _lastLoadedParam = null;
                _pendingStackTopClassType = null;
                _pendingArrayElementType = null;
                LogFieldResolutionFailure(fldName);
            }
        }
    }

    private EntityHandle? ResolveFieldToken(string fieldName)
    {
        // Try pending stack top class type
        if (_pendingStackTopClassType.HasValue && _typeNamesByHandle.TryGetValue(_pendingStackTopClassType.Value, out var pTypeName))
        {
            var keyDef = $"{pTypeName}::{fieldName}";
            if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef1))
            {
                DebugLog($"DEBUG: ldfld owner via pendingStack '{pTypeName}', field '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fdef1)}");
                return fdef1;
            }
        }
        
        // Try last-loaded local's class type
        if (!string.IsNullOrEmpty(_lastLoadedLocal) && _localVarClassTypeHandles.TryGetValue(_lastLoadedLocal, out var typeHandle) && _typeNamesByHandle.TryGetValue(typeHandle, out var typeName))
        {
            var keyDef = $"{typeName}::{fieldName}";
            if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef))
            {
                DebugLog($"DEBUG: ldfld owner via lastLoadedLocal '{typeName}', field '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fdef)}");
                return fdef;
            }
        }
        
        // Try last-loaded parameter's class type
        if (!string.IsNullOrEmpty(_lastLoadedParam) && _paramClassTypeHandles.TryGetValue(_lastLoadedParam, out var pTypeHandle) && _typeNamesByHandle.TryGetValue(pTypeHandle, out var pName))
        {
            var keyDef = $"{pName}::{fieldName}";
            if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef2))
            {
                DebugLog($"DEBUG: ldfld owner via lastLoadedParam '{pName}', field '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fdef2)}");
                return fdef2;
            }
        }

        // Fallback by simple name
        if (_fieldHandles.TryGetValue(fieldName, out var fldHandle))
        {
            DebugLog($"DEBUG: ldfld falling back by simple name '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fldHandle)}");
            return fldHandle;
        }

        return null;
    }

    private void PropagateFieldType(EntityHandle fieldToken)
    {
        _pendingStackTopClassType = null;
        _pendingArrayElementType = null;
        if (fieldToken.Kind == HandleKind.FieldDefinition)
        {
            var fdh = (FieldDefinitionHandle)fieldToken;
            if (_fieldDeclaredTypes.TryGetValue(fdh, out var declaredType))
            {
                if (!string.Equals(declaredType.Namespace, "System", StringComparison.Ordinal) && !string.IsNullOrEmpty(declaredType.Name))
                {
                    // Check if this is an array type (e.g., "Bar[]")
                    if (declaredType.Name.EndsWith("[]"))
                    {
                        // Extract element type name by removing "[]" suffix
                        var elementTypeName = declaredType.Name.Substring(0, declaredType.Name.Length - 2);
                        if (_typeHandles.TryGetValue(elementTypeName, out var elementTypeHandle))
                        {
                            // Track the array element type for ldelem operations
                            _pendingArrayElementType = elementTypeHandle;
                            DebugLog($"DEBUG: ldfld propagated array element type '{elementTypeName}' for ldelem");
                        }
                    }
                    else if (_typeHandles.TryGetValue(declaredType.Name, out var declTypeHandle))
                    {
                        _pendingStackTopClassType = declTypeHandle;
                    }
                }
            }
        }
    }

    private void LogFieldResolutionFailure(string fieldName)
    {
        try
        {
            var candidates = new List<string>();
            if (!string.IsNullOrEmpty(_pendingNewobjTypeName)) candidates.Add($"pendingNewobj={_pendingNewobjTypeName}");
            if (!string.IsNullOrEmpty(_lastLoadedLocal)) candidates.Add($"lastLoadedLocal={_lastLoadedLocal}");
            if (!string.IsNullOrEmpty(_lastLoadedParam)) candidates.Add($"lastLoadedParam={_lastLoadedParam}");
            var sampleKeys = _fieldHandlesByTypeAndName.Keys.Take(10).ToList();
            DebugLog($"DEBUG: ldfld: could not resolve field '{fieldName}'. Candidates: {string.Join(',', candidates)}. Sample field keys: {string.Join(';', sampleKeys)}");
        }
        catch { }
    }

    private void EmitLoadStaticField(InstructionEncoder il, LoadInstruction loadInst)
    {
        if (loadInst.Value is string staticFieldToken)
        {
            string typeName = string.Empty;
            string staticFieldName = staticFieldToken;
            var parts = staticFieldToken.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                typeName = parts[0];
                staticFieldName = parts[1];
            }

            EntityHandle? fieldToken = null;
            if (!string.IsNullOrEmpty(typeName))
            {
                var key = $"{typeName}::{staticFieldName}";
                if (_fieldHandlesByTypeAndName.TryGetValue(key, out var fh))
                {
                    fieldToken = fh;
                }
            }
            if (fieldToken == null && _fieldHandles.TryGetValue(staticFieldName, out var fldHandle))
            {
                fieldToken = fldHandle;
            }

            if (fieldToken != null)
            {
                il.OpCode(ILOpCode.Ldsfld);
                il.Token(fieldToken.Value);
                _lastLoadedLocal = null;
                _lastLoadedParam = null;
                _pendingStackTopClassType = null;
                _pendingArrayElementType = null;
            }
            else
            {
                il.OpCode(ILOpCode.Ldnull);
                _lastLoadedLocal = null;
                _lastLoadedParam = null;
                _pendingStackTopClassType = null;
                _pendingArrayElementType = null;
            }
        }
    }

    private void EmitBox(InstructionEncoder il, LoadInstruction loadInst, MetadataBuilder metadataBuilder)
    {
        if (loadInst.Value is string boxToken)
        {
            DebugLog($"DEBUG: EmitLoadInstruction - box token='{boxToken}'");
            var bt = boxToken.Trim();
            if (string.Equals(bt, "int32", StringComparison.OrdinalIgnoreCase) || string.Equals(bt, "System.Int32", StringComparison.Ordinal))
            {
                DebugLog("TRACE: Emitting ILOpCode.Box for int32");
                il.OpCode(ILOpCode.Box);
                il.Token(_systemInt32TypeRef);
                DebugLog("TRACE: Emitted token for box int32");
            }
            else
            {
                EmitBoxForCustomType(il, bt, metadataBuilder);
            }
        }
    }

    private void EmitBoxForCustomType(InstructionEncoder il, string boxToken, MetadataBuilder metadataBuilder)
    {
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
        if (!_assemblyRefHandles.TryGetValue(asmKey, out var asmRef))
        {
            asmRef = metadataBuilder.AddAssemblyReference(
                metadataBuilder.GetOrAddString(asm),
                new System.Version(1, 0, 0, 0),
                default, default, default, default);
            _assemblyRefHandles[asmKey] = asmRef;
        }
        var trKey = $"{asmKey}|{typeNs}|{simpleName}";
        if (!_typeRefHandlesCache.TryGetValue(trKey, out var paramTypeRef))
        {
            paramTypeRef = metadataBuilder.AddTypeReference(
                asmRef,
                metadataBuilder.GetOrAddString(typeNs),
                metadataBuilder.GetOrAddString(simpleName));
            _typeRefHandlesCache[trKey] = paramTypeRef;
        }
        il.OpCode(ILOpCode.Box);
        il.Token(paramTypeRef);
    }
}
