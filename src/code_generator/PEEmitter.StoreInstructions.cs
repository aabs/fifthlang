using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;
using static Fifth.DebugHelpers;

namespace code_generator;

/// <summary>
/// Partial class for PEEmitter - Store instruction emission
/// </summary>
public partial class PEEmitter
{
    /// <summary>
    /// Emit store instruction
    /// </summary>
    private void EmitStoreInstruction(InstructionEncoder il, il_ast.StoreInstruction storeInst, List<string>? localVarNames = null)
    {
        switch (storeInst.Opcode.ToLowerInvariant())
        {
            case "stloc":
                if (storeInst.Target is string varName && localVarNames != null)
                {
                    var index = localVarNames.IndexOf(varName);
                    if (index >= 0)
                    {
                        il.StoreLocal(index);
                        if (_lastWasNewobj)
                        {
                            // Only override type if it wasn't explicitly set (e.g., from variable declaration)
                            if (!_localVarTypeMap.ContainsKey(varName) || _localVarTypeMap[varName] == SignatureTypeCode.Int32)
                            {
                                DebugLog($"DEBUG: stloc override from newobj for '{varName}' to Object");
                                _localVarTypeMap[varName] = SignatureTypeCode.Object;
                            }
                            else
                            {
                                DebugLog($"DEBUG: stloc preserving existing type for '{varName}': {_localVarTypeMap[varName]}");
                            }
                            if (!string.IsNullOrEmpty(_pendingNewobjTypeName) && _typeHandles.TryGetValue(_pendingNewobjTypeName, out var tdh))
                            {
                                _localVarClassTypeHandles[varName] = tdh;
                            }
                        }
                        // If the top of the stack is a known class type (from ldfld), propagate that to the target local
                        // But only if the type wasn't explicitly declared
                        if (_pendingStackTopClassType.HasValue)
                        {
                            if (!_localVarTypeMap.ContainsKey(varName) || _localVarTypeMap[varName] == SignatureTypeCode.Int32)
                            {
                                DebugLog($"DEBUG: stloc override from stack class type for '{varName}' to Object");
                                _localVarTypeMap[varName] = SignatureTypeCode.Object;
                                _localVarClassTypeHandles[varName] = _pendingStackTopClassType.Value;
                            }
                            else
                            {
                                DebugLog($"DEBUG: stloc preserving existing type for '{varName}': {_localVarTypeMap[varName]}");
                            }
                        }
                    }
                    else
                    {
                        // Fallback to index 0 if variable not found
                        il.StoreLocal(0);
                    }
                    _lastWasNewobj = false;
                    _pendingNewobjTypeName = null;
                    _pendingStackTopClassType = null;
                }
                else
                {
                    // Fallback for non-string value or no local var names
                    il.StoreLocal(0);
                    _lastWasNewobj = false;
                    _pendingNewobjTypeName = null;
                    _pendingStackTopClassType = null;
                }
                break;
            case "starg":
                // Not supported yet; pop to keep stack balanced
                il.OpCode(ILOpCode.Pop);
                break;
            case "stelem.i4":
                il.OpCode(ILOpCode.Stelem_i4);
                break;

            case "stfld":
                if (storeInst.Target is string fieldName)
                {
                    // Use FieldDefinitionHandle directly; prefer exact owner type if known
                    EntityHandle? fieldToken = null;
                    // Prefer pendingStackTop class type if present (e.g., ldfld/newobj produced a known class)
                    if (_pendingStackTopClassType.HasValue && _typeNamesByHandle.TryGetValue(_pendingStackTopClassType.Value, out var pendingTypeName))
                    {
                        var keyPending = $"{pendingTypeName}::{fieldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyPending, out var fdefPending))
                        {
                            fieldToken = fdefPending;
                            DebugLog($"DEBUG: stfld owner via pendingStackTop '{pendingTypeName}', field '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fdefPending)}");
                        }
                    }
                    // First, if we have a pending newobj type, use it as the owner
                    if (!string.IsNullOrEmpty(_pendingNewobjTypeName) && _typeHandles.TryGetValue(_pendingNewobjTypeName, out var newObjTypeHandle) && _typeNamesByHandle.TryGetValue(newObjTypeHandle, out var newObjTypeName))
                    {
                        var keyNew = $"{newObjTypeName}::{fieldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyNew, out var fdefNew))
                        {
                            fieldToken = fdefNew;
                            DebugLog($"DEBUG: stfld owner via pendingNewobj '{newObjTypeName}', field '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fdefNew)}");
                        }
                    }
                    // Then try last-loaded local's class type
                    if (fieldToken == null && !string.IsNullOrEmpty(_lastLoadedLocal) && _localVarClassTypeHandles.TryGetValue(_lastLoadedLocal, out var typeHandle) && _typeNamesByHandle.TryGetValue(typeHandle, out var typeName))
                    {
                        var keyDef = $"{typeName}::{fieldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef))
                        {
                            fieldToken = fdef;
                            DebugLog($"DEBUG: stfld owner via lastLoadedLocal '{typeName}', field '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fdef)}");
                        }
                    }
                    // Then try last-loaded parameter's class type
                    if (fieldToken == null && !string.IsNullOrEmpty(_lastLoadedParam) && _paramClassTypeHandles.TryGetValue(_lastLoadedParam, out var pTypeHandle) && _typeNamesByHandle.TryGetValue(pTypeHandle, out var pName))
                    {
                        var keyPar = $"{pName}::{fieldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyPar, out var fdefPar))
                        {
                            fieldToken = fdefPar;
                            DebugLog($"DEBUG: stfld owner via lastLoadedParam '{pName}', field '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fdefPar)}");
                        }
                    }

                    // Fallback by simple name
                    if (fieldToken == null && _fieldHandles.TryGetValue(fieldName, out var fldHandle2))
                    {
                        fieldToken = fldHandle2;
                        DebugLog($"DEBUG: stfld falling back by simple name '{fieldName}' -> def handle {MetadataTokens.GetRowNumber(fldHandle2)}");
                    }

                    if (fieldToken != null)
                    {
                        // Stack: ..., obj, value -> store into obj.field
                        il.OpCode(ILOpCode.Stfld);
                        il.Token(fieldToken.Value);
                        if (!string.IsNullOrEmpty(_lastLoadedLocal))
                        {
                            _localVarTypeMap[_lastLoadedLocal] = SignatureTypeCode.Object;
                            _lastLoadedLocal = null;
                        }
                        _lastLoadedParam = null;
                    }
                    else
                    {
                        // Unknown field: consume value and obj to avoid corrupting the stack
                        il.OpCode(ILOpCode.Pop); // value
                        il.OpCode(ILOpCode.Pop); // obj
                        _lastLoadedLocal = null;
                        _lastLoadedParam = null;
                    }
                }
                break;

            case "stsfld":
                if (storeInst.Target is string staticFieldToken)
                {
                    // Expect token in form 'TypeName::FieldName' or just 'FieldName'
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
                    if (fieldToken == null && _fieldHandles.TryGetValue(staticFieldName, out var fldHandle2))
                    {
                        fieldToken = fldHandle2;
                    }

                    if (fieldToken != null)
                    {
                        // Static store consumes a single value
                        il.OpCode(ILOpCode.Stsfld);
                        il.Token(fieldToken.Value);
                    }
                    else
                    {
                        // Unknown static field: pop the value to keep stack balanced
                        il.OpCode(ILOpCode.Pop);
                    }
                }
                break;
        }
    }
}
