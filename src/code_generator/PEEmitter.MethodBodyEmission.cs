using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;
using static Fifth.DebugHelpers;

namespace code_generator;

/// <summary>
/// Partial class for PEEmitter - Method body emission
/// </summary>
public partial class PEEmitter
{
    /// <summary>
    /// Generate IL for a method body
    /// </summary>
    private (InstructionEncoder il, List<string> localVariables, Dictionary<string, SignatureTypeCode> localTypes) GenerateMethodIL(il_ast.MethodDefinition ilMethod, MetadataBuilder metadataBuilder, Dictionary<string, MethodDefinitionHandle> methodMap, Dictionary<string, List<string>> methodParamNames)
    {
        var ilInstructions = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(ilInstructions, controlFlow);

        // Reset per-method inference state
        _lastLoadedLocal = null;
        _lastWasNewobj = false;
        _localVarTypeMap = new Dictionary<string, SignatureTypeCode>(StringComparer.Ordinal);
        _pendingNewobjTypeName = null;
        _pendingStackTopClassType = null;
        _localVarClassTypeHandles.Clear();
        _paramClassTypeHandles.Clear();
        _lastLoadedParam = null;

        // Use AstToIlTransformationVisitor to get instruction sequences for each statement
        var transformer = new AstToIlTransformationVisitor();
        // Inform the transformer of the current method name for better diagnostics
        transformer.SetCurrentMethodName(ilMethod?.Name);
        // Provide current parameter names to the transformer so it can emit ldarg for them
        var paramNames = ilMethod?.Signature?.ParameterSignatures?.Select(p => p.Name ?? "param").ToList() ?? new List<string>();
        transformer.SetCurrentParameters(paramNames);
        var paramInfos = ilMethod?.Signature?.ParameterSignatures
            ?.Select(p => (name: p.Name ?? "param", typeName: p.TypeReference != null ? $"{p.TypeReference.Namespace}.{p.TypeReference.Name}" : null))
            ?.ToList() ?? new List<(string name, string? typeName)>();
        transformer.SetCurrentParameterTypes(paramInfos);
        // Provide the expected return type to the transformer so it can emit conservative defaults on returns when needed
        transformer.SetCurrentReturnType(ilMethod?.Signature?.ReturnTypeSignature);
        // Build a map from parameter name to argument index
        var paramIndexMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < paramNames.Count; i++) paramIndexMap[paramNames[i]] = i;
        // Build parameter class type handles (for user-defined types only)
        var sigParams = ilMethod?.Signature?.ParameterSignatures ?? new List<il_ast.ParameterSignature>();
        foreach (var ps in sigParams)
        {
            var pName = ps.Name ?? string.Empty;
            var tr = ps.TypeReference;
            if (!string.IsNullOrEmpty(pName) && tr != null && !string.Equals(tr.Namespace, "System", StringComparison.Ordinal) && !string.IsNullOrEmpty(tr.Name))
            {
                if (_typeHandles.TryGetValue(tr.Name, out var th))
                {
                    _paramClassTypeHandles[pName] = th;
                }
            }
        }

        // Track local variables
        var localVariables = new List<string>();
        void AddLocal(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (paramIndexMap.ContainsKey(name)) return; // don't treat params as locals
            if (!localVariables.Contains(name))
            {
                localVariables.Add(name);
                if (!_localVarTypeMap.ContainsKey(name)) _localVarTypeMap[name] = SignatureTypeCode.Int32;
            }
        }

        // For debugging: collect a textual dump of the lowered instruction sequences when requested
        var dumpInstructionsForMain = DebugEnabled && string.Equals(ilMethod?.Name, "main", StringComparison.OrdinalIgnoreCase);
        var perStmtDumps = dumpInstructionsForMain ? new List<string>() : null;

        var bodyStatements = ilMethod?.Impl?.Body?.Statements ?? new List<ast.Statement>();
        DebugLog($"DEBUG: Generating method body for '{ilMethod?.Name ?? "Unnamed"}' with {bodyStatements.Count} statements");

        // Two-pass lowering/emission: first lower all statements and collect method-wide local variable list
        var instructionSequences = new List<il_ast.InstructionSequence>();
        bool hasExplicitReturn = false;
        int stmtIndex = 0;
        foreach (var statement in bodyStatements)
        {
            var instructionSequence = transformer.GenerateStatement(statement);
            instructionSequences.Add(instructionSequence);
            if (perStmtDumps != null)
            {
                var sbd = new System.Text.StringBuilder();
                sbd.AppendLine($"--- stmt#{stmtIndex} ({statement?.GetType().Name}) ---");
                foreach (var instr in instructionSequence.Instructions)
                {
                    sbd.AppendLine(instr.ToString());
                }
                perStmtDumps.Add(sbd.ToString());
            }
            if (instructionSequence.Instructions.Any(i => i is il_ast.ReturnInstruction)) hasExplicitReturn = true;
            // Collect local names from all sequences so the emission pass has the complete set
            foreach (var instruction in instructionSequence.Instructions)
            {
                if (instruction is il_ast.LoadInstruction loadInst && loadInst.Opcode.ToLowerInvariant() == "ldloc" && loadInst.Value is string loadVar)
                {
                    AddLocal(loadVar);
                }
                else if (instruction is il_ast.StoreInstruction storeInst && storeInst.Opcode.ToLowerInvariant() == "stloc" && storeInst.Target is string storeVar)
                {
                    AddLocal(storeVar);
                }
            }
            stmtIndex++;
        }

        // After processing all statements, get the inferred local variable types from the transformer
        // and update our type map to use the correct types instead of defaulting to Object
        var inferredLocalTypes = transformer.GetLocalVariableTypes();
        DebugLog($"DEBUG: Got {inferredLocalTypes.Count} inferred local types from transformer");
        foreach (var kvp in inferredLocalTypes)
        {
            var varName = kvp.Key;
            var systemType = kvp.Value;
            DebugLog($"DEBUG: Inferred type for '{varName}': {systemType.Name}");
            
            // Map System.Type to SignatureTypeCode
            if (systemType == typeof(int) || systemType == typeof(Int32))
                _localVarTypeMap[varName] = SignatureTypeCode.Int32;
            else if (systemType == typeof(long) || systemType == typeof(Int64))
                _localVarTypeMap[varName] = SignatureTypeCode.Int64;
            else if (systemType == typeof(float) || systemType == typeof(Single))
                _localVarTypeMap[varName] = SignatureTypeCode.Single;
            else if (systemType == typeof(double) || systemType == typeof(Double))
                _localVarTypeMap[varName] = SignatureTypeCode.Double;
            else if (systemType == typeof(bool) || systemType == typeof(Boolean))
                _localVarTypeMap[varName] = SignatureTypeCode.Boolean;
            else if (systemType == typeof(string) || systemType == typeof(String))
                _localVarTypeMap[varName] = SignatureTypeCode.String;
            else if (systemType == typeof(byte) || systemType == typeof(Byte))
                _localVarTypeMap[varName] = SignatureTypeCode.Byte;
            else if (systemType == typeof(sbyte) || systemType == typeof(SByte))
                _localVarTypeMap[varName] = SignatureTypeCode.SByte;
            else if (systemType == typeof(short) || systemType == typeof(Int16))
                _localVarTypeMap[varName] = SignatureTypeCode.Int16;
            else if (systemType == typeof(ushort) || systemType == typeof(UInt16))
                _localVarTypeMap[varName] = SignatureTypeCode.UInt16;
            else if (systemType == typeof(uint) || systemType == typeof(UInt32))
                _localVarTypeMap[varName] = SignatureTypeCode.UInt32;
            else if (systemType == typeof(ulong) || systemType == typeof(UInt64))
                _localVarTypeMap[varName] = SignatureTypeCode.UInt64;
            else if (systemType == typeof(char) || systemType == typeof(Char))
                _localVarTypeMap[varName] = SignatureTypeCode.Char;
            // For other types (classes, etc.), keep as Object - will be handled by class type tracking
        }
        DebugLog($"DEBUG: After type inference, _localVarTypeMap has {_localVarTypeMap.Count} entries");

        // Second pass: simulate and emit using the final ordered local variable list
        var cumulativePerStatement = new List<int>();
        int cumulativeStack = 0;
        for (int i = 0; i < instructionSequences.Count; i++)
        {
            var instructionSequence = instructionSequences[i];
            DebugLog($"DEBUG: Statement generated {instructionSequence.Instructions.Count} instructions");
            if (DebugEnabled)
            {
                foreach (var instr in instructionSequence.Instructions) DebugLog($"  - {instr.GetType().Name}: {instr}");
            }

            // Simulation and conservative fallback insertion for returns (same logic as before)
            try
            {
                var (delta, error) = SimulateInstructionSequence(instructionSequence);
                if (error != null)
                {
                    var summary = $"PEEmitter: Stack simulation failed for method '{ilMethod?.Name}' at statement #{i}: {error}. Sequence: {string.Join("; ", instructionSequence.Instructions.Select(inst => inst.ToString()))}";
                    Console.WriteLine(summary);
                    throw new InvalidOperationException(summary);
                }

                var containsReturnInstr = instructionSequence.Instructions.Any(inst => inst is il_ast.ReturnInstruction);
                if (containsReturnInstr && delta <= 0)
                {
                    // Choose fallback based on declared return type
                    var retNameLocal = ilMethod?.Signature?.ReturnTypeSignature?.Name ?? string.Empty;
                    il_ast.LoadInstruction fallbackLoad;
                    switch (retNameLocal)
                    {
                        case "Int32":
                        case "Boolean":
                        case "Char":
                        case "SByte":
                        case "Byte":
                        case "Int16":
                        case "UInt16":
                        case "UInt32":
                        case "UInt64":
                        case "Int64":
                            fallbackLoad = new il_ast.LoadInstruction("ldc.i4", 0);
                            break;
                        case "Single": fallbackLoad = new il_ast.LoadInstruction("ldc.r4", 0f); break;
                        case "Double": fallbackLoad = new il_ast.LoadInstruction("ldc.r8", 0.0); break;
                        case "String": case "Object": case "": fallbackLoad = new il_ast.LoadInstruction("ldnull"); break;
                        default: fallbackLoad = new il_ast.LoadInstruction("ldc.i4", 0); break;
                    }
                    var retIdx = instructionSequence.Instructions.FindIndex(inst => inst is il_ast.ReturnInstruction);
                    if (retIdx >= 0) instructionSequence.Instructions.Insert(retIdx, fallbackLoad); else instructionSequence.Instructions.Add(fallbackLoad);
                    Console.WriteLine($"WARNING: Fallback default push inserted for ReturnStatement in method '{ilMethod?.Name}' at statement #{i} due to zero-length lowered expression; inserted '{fallbackLoad.Opcode}'");
                    DebugLog($"DEBUG: Inserted fallback '{fallbackLoad.Opcode}' before return for method '{ilMethod?.Name}', stmt#{i}");
                    var simAfter = SimulateInstructionSequence(instructionSequence);
                    if (simAfter.error != null)
                    {
                        var summary = $"PEEmitter: Stack simulation failed after inserting fallback for method '{ilMethod?.Name}' at statement #{i}: {simAfter.error}. Sequence: {string.Join("; ", instructionSequence.Instructions.Select(inst => inst.ToString()))}";
                        Console.WriteLine(summary);
                        throw new InvalidOperationException(summary);
                    }
                    delta = simAfter.delta;
                }

                if (cumulativeStack + delta < 0)
                {
                    var summary = $"PEEmitter: Stack underflow in method '{ilMethod?.Name}' at statement #{i}. cumulative={cumulativeStack}, delta={delta}. Sequence: {string.Join("; ", instructionSequence.Instructions.Select(inst => inst.ToString()))}";
                    Console.WriteLine(summary);
                    throw new InvalidOperationException(summary);
                }
                cumulativeStack += delta;
                cumulativePerStatement.Add(cumulativeStack);
            }
            catch { throw; }

            // Emit using the final per-method localVariables list so ldloc indices are stable
            EmitInstructionSequence(il, instructionSequence, metadataBuilder, methodMap, localVariables, paramIndexMap, null, methodParamNames);
        }

        // Write per-statement dump now so it exists even if final-stack validation fails
        var hasStatements = instructionSequences.Count > 0;
        if (dumpInstructionsForMain && perStmtDumps != null)
        {
            try
            {
                var dumpDir = Path.Combine(Directory.GetCurrentDirectory(), "build_debug_il");
                Directory.CreateDirectory(dumpDir);
                var dumpPath = Path.Combine(dumpDir, $"method_{ilMethod?.Name}_{Guid.NewGuid():N}.txt");
                File.WriteAllLines(dumpPath, perStmtDumps.SelectMany(s => new[] { s, "" }));
                DebugLog($"DEBUG: Wrote per-statement instruction dump for method '{ilMethod?.Name}' to: {dumpPath}");
            }
            catch { /* best-effort */ }
        }
        else if (!hasStatements)
        {
            // If no statements, add a simple return for the method to be valid
            // For int return type, load a constant first
            var returnType = ilMethod?.Signature?.ReturnTypeSignature?.Name ?? string.Empty;
            if (returnType == "Int32")
            {
                il.LoadConstantI4(0); // Default return value
            }
            else if (!string.Equals(returnType, "Void", StringComparison.Ordinal))
            {
                il.OpCode(ILOpCode.Ldnull);
            }
            il.OpCode(ILOpCode.Ret);
        }

        // After emitting all statements, validate final stack matches return expectation
        int expectedReturnStack = 0;
        var retTypeName = ilMethod?.Signature?.ReturnTypeSignature?.Name ?? "Void";
        switch (retTypeName)
        {
            case "Void": expectedReturnStack = 0; break;
            case "Int32":
            case "Boolean":
            case "Single":
            case "Double":
            case "String":
            default: expectedReturnStack = 1; break;
        }

        // If the method contains an explicit return statement, the return instruction(s)
        // Ensure all methods end with a return instruction
        // Methods without explicit returns and with correct stack still need ret
        if (!hasExplicitReturn)
        {
            if (cumulativeStack == expectedReturnStack)
            {
                // Stack is correct, just add ret
                il.OpCode(ILOpCode.Ret);
                DebugLog($"DEBUG: Added ret instruction for method '{ilMethod?.Name}' without explicit return");
            }
            else
            {
                // Stack mismatch - attempt to repair
                var needed = expectedReturnStack - cumulativeStack;
                if (needed > 0)
                {
                    // Insert required producers directly into the emitted IL
                    for (int n = 0; n < needed; n++)
                    {
                        switch (retTypeName)
                        {
                            case "Int32":
                            case "Boolean":
                            case "Char":
                            case "SByte":
                            case "Byte":
                            case "Int16":
                            case "UInt16":
                            case "UInt32":
                            case "UInt64":
                            case "Int64":
                                il.LoadConstantI4(0);
                                break;
                            case "Single":
                                il.LoadConstantR4(0f);
                                break;
                            case "Double":
                                il.LoadConstantR8(0.0);
                                break;
                            case "String":
                            case "Object":
                            default:
                                il.OpCode(ILOpCode.Ldnull);
                                break;
                        }
                    }
                    // Append a terminating return so the method is well-formed
                    il.OpCode(ILOpCode.Ret);
                    Console.WriteLine($"WARNING: Injected {needed} default return value(s) for method '{ilMethod?.Name}' due to final-stack mismatch (expected {expectedReturnStack}, got {cumulativeStack}).");
                    DebugLog($"DEBUG: Injected {needed} fallback load(s) + ret for method '{ilMethod?.Name}' to satisfy expected return stack.");
                    cumulativeStack += needed;
                    cumulativePerStatement.Add(cumulativeStack);
                }
                else if (needed < 0)
                {
                    // Too many values on the stack; pop extras to recover
                    for (int p = 0; p < -needed; p++)
                    {
                        il.OpCode(ILOpCode.Pop);
                    }
                    DebugLog($"DEBUG: Popped {-needed} extra stack value(s) for method '{ilMethod?.Name}' to satisfy expected return stack.");
                    cumulativeStack += needed; // needed is negative
                    cumulativePerStatement.Add(cumulativeStack);
                }

                // If we still don't match, emit a diagnostic and fail loudly so it can be fixed
                if (cumulativeStack != expectedReturnStack)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine($"PEEmitter: Method '{ilMethod?.Name}' expected final stack {expectedReturnStack} but got {cumulativeStack}. Statement stack history (last 10 entries):");
                    var start = Math.Max(0, cumulativePerStatement.Count - 10);
                    for (int i = start; i < cumulativePerStatement.Count; i++)
                    {
                        sb.AppendLine($"  stmt#{i}: cumulative={cumulativePerStatement[i]}");
                    }
                    var msg = sb.ToString();
                    var fullMsg = $"PEEmitter: Final stack mismatch for method '{ilMethod?.Name}': expected {expectedReturnStack} but simulation resulted in {cumulativeStack}.{msg}";
                    Console.WriteLine(fullMsg);
                    throw new InvalidOperationException(fullMsg);
                }
            }
        }
        // Dump raw IL bytes produced for this method (for debug)
        try
        {
            var arr = ilInstructions.ToArray();
            DebugLog($"TRACE: Generated IL bytes for method '{ilMethod?.Name}': {BitConverter.ToString(arr)}");
        }
        catch { }
        return (il, localVariables, _localVarTypeMap);
    }

    /// <summary>
    /// Create a local variable signature for the method
    /// </summary>
}
