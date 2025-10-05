using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;
using static Fifth.DebugHelpers;

namespace code_generator;

/// <summary>
/// Partial class for PEEmitter - Call instruction emission
/// </summary>
public partial class PEEmitter
{
    /// <summary>
    /// Emit call instruction
    /// </summary>
    private void EmitCallInstruction(InstructionEncoder il, il_ast.CallInstruction callInst,
        MetadataBuilder metadataBuilder, Dictionary<string, MethodDefinitionHandle>? methodMap = null, Dictionary<string, int>? paramIndexMap = null, Dictionary<string, List<string>>? methodParamNames = null)
    {
        DebugLog($"DEBUG: EmitCallInstruction opcode='{callInst?.Opcode ?? "<null>"}', sig='{callInst?.MethodSignature ?? "<null>"}'");
        // Handle external static call signature produced by AstToIlTransformationVisitor
        if ((callInst?.MethodSignature ?? string.Empty).StartsWith("extcall:", StringComparison.Ordinal))
        {
            try
            {
                var extSig = callInst?.MethodSignature ?? string.Empty; // format: extcall:Asm=...;Ns=...;Type=...;Method=...;Params=...;Return=...
                var parts = extSig.Substring("extcall:".Length).Split(';', StringSplitOptions.RemoveEmptyEntries);
                var dict = parts.Select(p => p.Split('=')).Where(a => a.Length == 2).ToDictionary(a => a[0], a => a[1]);
                dict.TryGetValue("Asm", out var asmName);
                dict.TryGetValue("Ns", out var ns);
                dict.TryGetValue("Type", out var typeName);
                dict.TryGetValue("Method", out var extMethodName);
                dict.TryGetValue("Params", out var paramList);
                dict.TryGetValue("Return", out var returnToken);

                // Create an AssemblyRef for the external assembly (fallback to System.Runtime for metadata needs)
                // Choose appropriate assembly version for known framework libs
                System.Version ResolveAssemblyVersion(string? name)
                {
                    if (string.Equals(name, "System.Runtime", StringComparison.Ordinal)) return new System.Version(8, 0, 0, 0);
                    if (string.Equals(name, "System.Console", StringComparison.Ordinal)) return new System.Version(8, 0, 0, 0);
                    if (string.Equals(name, "dotNetRDF", StringComparison.Ordinal)) return new System.Version(3, 4, 0, 0);
                    if (string.Equals(name, "System.Private.CoreLib", StringComparison.Ordinal)) return new System.Version(8, 0, 0, 0);
                    if (string.Equals(name, "Fifth.System", StringComparison.Ordinal)) return new System.Version(1, 0, 0, 0);
                    return new System.Version(1, 0, 0, 0);
                }
                
                // Get public key token for known assemblies
                BlobHandle GetPublicKeyToken(string? name)
                {
                    byte[]? token = null;
                    if (string.Equals(name, "System.Runtime", StringComparison.Ordinal) ||
                        string.Equals(name, "System.Console", StringComparison.Ordinal))
                    {
                        // Microsoft public key token: b03f5f7f11d50a3a
                        token = new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a };
                    }
                    else if (string.Equals(name, "System.Private.CoreLib", StringComparison.Ordinal))
                    {
                        // CoreLib public key token: 7cec85d7bea7798e
                        token = new byte[] { 0x7c, 0xec, 0x85, 0xd7, 0xbe, 0xa7, 0x79, 0x8e };
                    }
                    
                    if (token != null)
                    {
                        return metadataBuilder.GetOrAddBlob(token);
                    }
                    return default;
                }

                var asmNameResolved = string.IsNullOrWhiteSpace(asmName) ? "Fifth.System" : asmName;
                var asmKey = asmNameResolved.ToLowerInvariant();
                AssemblyReferenceHandle asmRef;
                if (!_assemblyRefHandles.TryGetValue(asmKey, out asmRef))
                {
                    asmRef = metadataBuilder.AddAssemblyReference(
                        metadataBuilder.GetOrAddString(asmNameResolved!),
                        ResolveAssemblyVersion(asmNameResolved), default, GetPublicKeyToken(asmNameResolved), default, default);
                    _assemblyRefHandles[asmKey] = asmRef;
                }

                // Create a TypeRef for the external type
                var ownerNs = string.IsNullOrWhiteSpace(ns) ? "Fifth.System" : ns;
                var ownerName = typeName ?? "KG";
                var ownerKey = $"{asmNameResolved.ToLowerInvariant()}|{ownerNs}|{ownerName}";
                TypeReferenceHandle typeRef;
                if (!_typeRefHandlesCache.TryGetValue(ownerKey, out typeRef))
                {
                    typeRef = metadataBuilder.AddTypeReference(
                        asmRef,
                        metadataBuilder.GetOrAddString(ownerNs),
                        metadataBuilder.GetOrAddString(ownerName));
                    _typeRefHandlesCache[ownerKey] = typeRef;
                }

                // Helper to write a type token (e.g., System.Int32 or Namespace.TypeName@Asm) to signature
                void WriteTypeToken(BlobBuilder sigBuilder, string token, string? fallbackAsm)
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        sigBuilder.WriteByte((byte)SignatureTypeCode.Object);
                        return;
                    }

                    // Allow tokens to include an assembly suffix like 'Full.Type.Name@Asm'.
                    // Split that off first so we can correctly detect primitive/system types.
                    var t = token.Trim();
                    string asmFromToken = fallbackAsm ?? "Fifth.System";
                    string baseTypeName = t;
                    var atIdx = t.LastIndexOf('@');
                    if (atIdx > 0)
                    {
                        baseTypeName = t.Substring(0, atIdx);
                        asmFromToken = t.Substring(atIdx + 1);
                    }

                    // System primitives and common types should be encoded using SignatureTypeCode
                    switch (baseTypeName)
                    {
                        case "System.Void": sigBuilder.WriteByte((byte)SignatureTypeCode.Void); return;
                        case "System.Int32": sigBuilder.WriteByte((byte)SignatureTypeCode.Int32); return;
                        case "System.String": sigBuilder.WriteByte((byte)SignatureTypeCode.String); return;
                        case "System.Single": sigBuilder.WriteByte((byte)SignatureTypeCode.Single); return;
                        case "System.Double": sigBuilder.WriteByte((byte)SignatureTypeCode.Double); return;
                        case "System.Boolean": sigBuilder.WriteByte((byte)SignatureTypeCode.Boolean); return;
                        case "System.Object": sigBuilder.WriteByte((byte)SignatureTypeCode.Object); return;
                    }

                    // External/class types: use the parsed assembly from the token (or fallback)
                    string fullName = baseTypeName;
                    string asm = asmFromToken;
                    // Split namespace and type name
                    string typeNs = string.Empty;
                    string simpleName = fullName;
                    var lastDot = fullName.LastIndexOf('.');
                    if (lastDot > 0)
                    {
                        typeNs = fullName.Substring(0, lastDot);
                        simpleName = fullName.Substring(lastDot + 1);
                    }
                    var paramAsmKey = asm.ToLowerInvariant();
                    AssemblyReferenceHandle paramAsmRef;
                    if (!_assemblyRefHandles.TryGetValue(paramAsmKey, out paramAsmRef))
                    {
                        paramAsmRef = metadataBuilder.AddAssemblyReference(
                            metadataBuilder.GetOrAddString(asm),
                            ResolveAssemblyVersion(asm), default, GetPublicKeyToken(asm), default, default);
                        _assemblyRefHandles[paramAsmKey] = paramAsmRef;
                    }
                    var paramTypeKey = $"{paramAsmKey}|{typeNs}|{simpleName}";
                    TypeReferenceHandle paramTypeRef;
                    if (!_typeRefHandlesCache.TryGetValue(paramTypeKey, out paramTypeRef))
                    {
                        paramTypeRef = metadataBuilder.AddTypeReference(
                            paramAsmRef,
                            metadataBuilder.GetOrAddString(typeNs),
                            metadataBuilder.GetOrAddString(simpleName));
                        _typeRefHandlesCache[paramTypeKey] = paramTypeRef;
                    }

                    // Write CLASS + TypeDefOrRef coded index (TypeRef tag = 1)
                    sigBuilder.WriteByte(0x12);
                    var rowId = MetadataTokens.GetRowNumber((TypeReferenceHandle)paramTypeRef);
                    var codedIndex = (rowId << 2) | 1; // tag 1 = TypeRef
                    sigBuilder.WriteCompressedInteger(codedIndex);
                }

                // Build signature using accurate param and return types
                // Note: We'll rebuild the signature later with the correct calling convention after resolving the method
                var methodSig = new BlobBuilder();

                // Parse any explicit params advertised in the extcall token (may be coarse/granular)
                var parsedParamTokens = new List<string>();
                if (!string.IsNullOrWhiteSpace(paramList))
                {
                    parsedParamTokens = paramList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }

                // Local helper: translate a System.Type into a token string usable by WriteTypeToken.
                static string TypeToToken(Type t)
                {
                    if (t == null) return "System.Object";
                    if (t.IsByRef) t = t.GetElementType() ?? t;
                    if (t == typeof(void)) return "System.Void";
                    if (t == typeof(int)) return "System.Int32";
                    if (t == typeof(string)) return "System.String";
                    if (t == typeof(float)) return "System.Single";
                    if (t == typeof(double)) return "System.Double";
                    if (t == typeof(bool)) return "System.Boolean";
                    if (t == typeof(long)) return "System.Int64";
                    if (t == typeof(short)) return "System.Int16";
                    if (t == typeof(byte)) return "System.Byte";
                    if (t == typeof(sbyte)) return "System.SByte";
                    if (t == typeof(uint)) return "System.UInt32";
                    if (t == typeof(ushort)) return "System.UInt16";
                    if (t == typeof(ulong)) return "System.UInt64";
                    if (t == typeof(char)) return "System.Char";
                    // Non-primitive types: include assembly short name for disambiguation
                    return (t.FullName ?? t.Name) + "@" + (t.Assembly.GetName().Name ?? "");
                }

                // Try to resolve the runtime MethodInfo for this external type/method so we can derive exact parameter/return types
                MethodInfo? resolved = null;
                try
                {
                    // Attempt to find the Type in already-loaded assemblies first
                    Type? extType = null;
                    var candidateFullName = string.IsNullOrWhiteSpace(ownerNs) ? ownerName : (ownerNs + "." + ownerName);
                    // Check loaded assemblies
                    var loaded = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var a in loaded)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(asmNameResolved) && !string.Equals(a.GetName().Name, asmNameResolved, StringComparison.OrdinalIgnoreCase)) continue;
                            var t = a.GetType(candidateFullName, throwOnError: false, ignoreCase: false);
                            if (t != null) { extType = t; break; }
                        }
                        catch { }
                    }
                    // Fallback: try Type.GetType with assembly-qualified name
                    if (extType == null)
                    {
                        try { extType = Type.GetType(candidateFullName + ", " + asmNameResolved, throwOnError: false, ignoreCase: false); } catch { }
                    }







                    if (extType != null)
                    {
                        // Find candidate methods by name
                        var cand = extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Where(m => string.Equals(m.Name, extMethodName, StringComparison.Ordinal)).ToArray();
                        if (cand.Length > 0)
                        {
                            // Prefer exact arity match to callInst.ArgCount (if provided)
                            MethodInfo? pick = null;
                            if (callInst?.ArgCount >= 0)
                            {
                                pick = cand.FirstOrDefault(m => m.GetParameters().Length == callInst.ArgCount);
                            }
                            // Next prefer parsed param count
                            if (pick == null && parsedParamTokens.Count > 0)
                            {
                                pick = cand.FirstOrDefault(m => m.GetParameters().Length == parsedParamTokens.Count);
                            }
                            // Finally pick the first candidate
                            pick ??= cand[0];
                            resolved = pick;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLog($"DEBUG: Runtime method resolution failed for extcall {ownerNs}.{ownerName}::{extMethodName}: {ex.Message}");
                }

                // Derive parameter tokens and return token from resolved MethodInfo when available
                var finalParamTokens = new List<string>();
                string finalReturnToken = string.IsNullOrWhiteSpace(returnToken) ? "System.Object" : returnToken!;
                bool isStaticMethod = true; // Assume static unless proven otherwise
                if (resolved != null)
                {
                    try
                    {
                        isStaticMethod = resolved.IsStatic;
                        var ps = resolved.GetParameters();
                        foreach (var p in ps) finalParamTokens.Add(TypeToToken(p.ParameterType));
                        finalReturnToken = TypeToToken(resolved.ReturnType);
                        DebugLog($"DEBUG: Resolved runtime method: {resolved.DeclaringType?.FullName}.{resolved.Name} params={ps.Length} return={resolved.ReturnType.FullName} isStatic={isStaticMethod}");
                    }
                    catch { /* if reflection fails, fallback to parsed tokens */ }
                }
                // If reflection did not yield a result, fall back to parsed tokens; if none and callInst reports args, pad with System.Object
                if (finalParamTokens.Count == 0)
                {
                    if (parsedParamTokens.Count > 0)
                    {
                        finalParamTokens = parsedParamTokens;
                    }
                    else if ((callInst?.ArgCount ?? 0) > 0)
                    {
                        for (int pi = 0; pi < callInst!.ArgCount; pi++) finalParamTokens.Add("System.Object@System.Runtime");
                    }
                }
                // If CallInstruction reports ArgCount but finalParamTokens differ, log a debug warning but continue using finalParamTokens for the MemberRef
                if (callInst != null && callInst.ArgCount >= 0 && finalParamTokens.Count != callInst.ArgCount)
                {
                    DebugLog($"WARNING: External call arity mismatch (tolerated) for method '{extMethodName}' in assembly '{asmNameResolved}'. CallInstruction.ArgCount={callInst.ArgCount}, resolvedParams={finalParamTokens.Count}, extSig='{extSig}'");
                }

                // Rebuild the method signature with the correct calling convention
                methodSig = new BlobBuilder();
                // For static methods, use DEFAULT (0x00). For instance methods, use DEFAULT | HASTHIS (0x00 | 0x20 = 0x20)
                byte callingConvention = isStaticMethod ? (byte)0x00 : (byte)0x20;
                methodSig.WriteByte(callingConvention);
                
                // Use the final tokens to write signature
                methodSig.WriteByte((byte)finalParamTokens.Count);
                WriteTypeToken(methodSig, finalReturnToken, asmName);
                foreach (var pTok in finalParamTokens)
                {
                    WriteTypeToken(methodSig, pTok, asmName);
                }

                // Create a MemberRef for the external type/method using the constructed signature
                try
                {
                    var methodNameForRef = string.IsNullOrWhiteSpace(extMethodName) ? "<unknown>" : extMethodName;
                    var memberRef = metadataBuilder.AddMemberReference(typeRef, metadataBuilder.GetOrAddString(methodNameForRef), metadataBuilder.GetOrAddBlob(methodSig));
                    DebugLog($"DEBUG: Created MemberRef for external call: {ownerNs}.{ownerName}::{methodNameForRef} params={finalParamTokens.Count} return={finalReturnToken}");
                    il.Call(memberRef);
                    return;
                }
                catch (Exception ex)
                {
                    DebugLog($"DEBUG: Failed to add MemberRef for external call {ownerNs}.{ownerName}::{extMethodName}: {ex.Message}");
                    // Fall through to unresolved behavior below so the emitter remains tolerant
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"WARNING: Failed to emit external call: {ex.Message}");
                // Fall through to unresolved behavior below
            }
        }
        // Support bracketed assembly-style signatures like: "void [System.Console]System.Console::WriteLine(object)"
        var sigStr = callInst?.MethodSignature ?? string.Empty;
        if (!string.IsNullOrEmpty(sigStr) && sigStr.Contains("[") && sigStr.Contains("]") && sigStr.Contains("::"))
        {
            try
            {
                // Example: "void [System.Console]System.Console::WriteLine(object)"
                var bracketSig = sigStr.Trim();
                // Extract return type (before first space)
                var firstSpace = bracketSig.IndexOf(' ');
                var returnToken = firstSpace > 0 ? bracketSig.Substring(0, firstSpace).Trim() : "System.Void";
                // Extract assembly between [ and ]
                var lb = bracketSig.IndexOf('[');
                var rb = bracketSig.IndexOf(']');
                var asmName = (lb >= 0 && rb > lb) ? bracketSig.Substring(lb + 1, rb - lb - 1).Trim() : "Fifth.System";
                // The remainder after ']' contains TypeName::Method(params)
                var after = bracketSig.Substring(rb + 1).Trim();
                var sep = after.IndexOf("::", StringComparison.Ordinal);
                if (sep < 0) throw new InvalidOperationException("Invalid bracketed signature format");
                var fullTypeName = after.Substring(0, sep).Trim();
                var methodAndParams = after.Substring(sep + 2).Trim();
                var paren = methodAndParams.IndexOf('(');
                var bracketMethodName = paren > 0 ? methodAndParams.Substring(0, paren).Trim() : methodAndParams;
                var paramsList = new List<string>();
                if (paren >= 0)
                {
                    var paramsSection = methodAndParams.Substring(paren + 1).Trim();
                    if (paramsSection.EndsWith(")")) paramsSection = paramsSection.Substring(0, paramsSection.Length - 1);
                    if (!string.IsNullOrWhiteSpace(paramsSection)) paramsList = paramsSection.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }

                // Ensure assembly reference exists
                var asmKey = asmName.ToLowerInvariant();
                AssemblyReferenceHandle asmRef;
                if (!_assemblyRefHandles.TryGetValue(asmKey, out asmRef))
                {
                    asmRef = metadataBuilder.AddAssemblyReference(metadataBuilder.GetOrAddString(asmName), new System.Version(8, 0, 0, 0), default, default, default, default);
                    _assemblyRefHandles[asmKey] = asmRef;
                }

                // Split fullTypeName into namespace and type
                var lastDot = fullTypeName.LastIndexOf('.');
                var typeNs = lastDot > 0 ? fullTypeName.Substring(0, lastDot) : string.Empty;
                var simpleType = lastDot > 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
                var typeKey = $"{asmKey}|{typeNs}|{simpleType}";
                TypeReferenceHandle typeRef;
                if (!_typeRefHandlesCache.TryGetValue(typeKey, out typeRef))
                {
                    typeRef = metadataBuilder.AddTypeReference(asmRef, metadataBuilder.GetOrAddString(typeNs), metadataBuilder.GetOrAddString(simpleType));
                    _typeRefHandlesCache[typeKey] = typeRef;
                }

                // Build method signature blob (simple mapping for common system types)
                var methodSig = new BlobBuilder();
                methodSig.WriteByte(0x00); // DEFAULT
                methodSig.WriteByte((byte)paramsList.Count);
                // Map return token
                void writeSimpleType(BlobBuilder bb, string tok)
                {
                    var t = tok.Trim();
                    switch (t.ToLowerInvariant())
                    {
                        case "void": bb.WriteByte((byte)SignatureTypeCode.Void); return;
                        case "int": case "int32": bb.WriteByte((byte)SignatureTypeCode.Int32); return;
                        case "string": bb.WriteByte((byte)SignatureTypeCode.String); return;
                        case "bool": case "boolean": bb.WriteByte((byte)SignatureTypeCode.Boolean); return;
                        case "float": case "float32": bb.WriteByte((byte)SignatureTypeCode.Single); return;
                        case "double": case "float64": bb.WriteByte((byte)SignatureTypeCode.Double); return;
                        case "long": case "int64": bb.WriteByte((byte)SignatureTypeCode.Int64); return;
                        case "short": case "int16": bb.WriteByte((byte)SignatureTypeCode.Int16); return;
                        case "byte": case "uint8": bb.WriteByte((byte)SignatureTypeCode.Byte); return;
                        case "sbyte": case "int8": bb.WriteByte((byte)SignatureTypeCode.SByte); return;
                        case "uint": case "uint32": bb.WriteByte((byte)SignatureTypeCode.UInt32); return;
                        case "ushort": case "uint16": bb.WriteByte((byte)SignatureTypeCode.UInt16); return;
                        case "ulong": case "uint64": bb.WriteByte((byte)SignatureTypeCode.UInt64); return;
                        case "char": case "char16": bb.WriteByte((byte)SignatureTypeCode.Char); return;
                        default:
                            // Fallback to object
                            bb.WriteByte((byte)SignatureTypeCode.Object); return;
                    }
                }
                writeSimpleType(methodSig, string.IsNullOrWhiteSpace(returnToken) ? "void" : returnToken);
                foreach (var p in paramsList) writeSimpleType(methodSig, p);

                var memberRef = metadataBuilder.AddMemberReference(typeRef, metadataBuilder.GetOrAddString(bracketMethodName), metadataBuilder.GetOrAddBlob(methodSig));
                il.Call(memberRef);
                return;
            }
            catch { /* best-effort; fall through to unresolved behavior */ }
        }

        // Handle constructor calls (newobj)
        if (((callInst?.Opcode) ?? string.Empty).ToLowerInvariant() == "newobj")
        {
            DebugLog($"DEBUG: Emitting newobj for: {callInst?.MethodSignature ?? "<null>"}");
            // Try to extract type name and resolve our emitted constructor
            var typeName = ExtractCtorTypeName(callInst?.MethodSignature ?? string.Empty);
            if (!string.IsNullOrEmpty(typeName) && _ctorHandles.TryGetValue(typeName, out var ctorHandle))
            {
                il.OpCode(ILOpCode.Newobj);
                il.Token(ctorHandle);
                _lastWasNewobj = true;
                _pendingNewobjTypeName = typeName;
                return;
            }

            // Attempt to parse bracketed-style external constructor signatures and emit a MemberRef-backed newobj
            try
            {
                // Examples to support:
                //  - "void [AssemblyName]Namespace.Type::.ctor()"
                //  - "instance void Namespace.Type::.ctor()"
                var rawSig = (callInst?.MethodSignature ?? string.Empty).Trim();
                string? asmName = null;
                string? fullTypeName = null;

                if (rawSig.Contains(']') && rawSig.Contains('[') && rawSig.Contains("::"))
                {
                    var lb = rawSig.IndexOf('[');
                    var rb = rawSig.IndexOf(']');
                    if (rb > lb)
                    {
                        asmName = rawSig.Substring(lb + 1, rb - lb - 1).Trim();
                        var after = rawSig.Substring(rb + 1).Trim();
                        var sep = after.IndexOf("::", StringComparison.Ordinal);
                        if (sep > 0) fullTypeName = after.Substring(0, sep).Trim();
                    }
                }
                else if (rawSig.Contains("::"))
                {
                    // No bracketed asm; find type between return-type and '::'
                    var sep = rawSig.IndexOf("::", StringComparison.Ordinal);
                    if (sep > 0)
                    {
                        // Find last space before sep to skip return-type token
                        var lastSpaceBeforeSep = rawSig.LastIndexOf(' ', sep);
                        var start = lastSpaceBeforeSep >= 0 ? lastSpaceBeforeSep + 1 : 0;
                        fullTypeName = rawSig.Substring(start, sep - start).Trim();
                    }
                }

                if (!string.IsNullOrEmpty(fullTypeName))
                {
                    // Determine simple type name portion
                    var simpleType = fullTypeName.Contains('.') ? fullTypeName.Split('.').Last() : fullTypeName;

                    // Ensure assembly ref exists (fallback to Fifth.System when not specified)
                    var asmKey = (string.IsNullOrWhiteSpace(asmName) ? "Fifth.System" : asmName).ToLowerInvariant();
                    AssemblyReferenceHandle asmRef;
                    if (!_assemblyRefHandles.TryGetValue(asmKey, out asmRef))
                    {
                        asmRef = metadataBuilder.AddAssemblyReference(metadataBuilder.GetOrAddString(asmKey == "fifth.system" ? "Fifth.System" : asmName ?? "Fifth.System"), new System.Version(1, 0, 0, 0), default, default, default, default);
                        _assemblyRefHandles[asmKey] = asmRef;
                    }

                    // Split namespace and type
                    var lastDot = fullTypeName.LastIndexOf('.');
                    var typeNs = lastDot > 0 ? fullTypeName.Substring(0, lastDot) : string.Empty;
                    var simpleName = lastDot > 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
                    var typeKey = $"{asmKey}|{typeNs}|{simpleName}";
                    TypeReferenceHandle typeRef;
                    if (!_typeRefHandlesCache.TryGetValue(typeKey, out typeRef))
                    {
                        typeRef = metadataBuilder.AddTypeReference(asmRef, metadataBuilder.GetOrAddString(typeNs), metadataBuilder.GetOrAddString(simpleName));
                        _typeRefHandlesCache[typeKey] = typeRef;
                    }

                    // Build MemberRef signature for a parameterless ctor (void returning, zero params)
                    var ctorSig = new BlobBuilder();
                    ctorSig.WriteByte(0x20); // HASTHIS
                    ctorSig.WriteByte(0x00); // param count 0
                    ctorSig.WriteByte((byte)SignatureTypeCode.Void);

                    var memberRef = metadataBuilder.AddMemberReference(typeRef, metadataBuilder.GetOrAddString(".ctor"), metadataBuilder.GetOrAddBlob(ctorSig));
                    il.OpCode(ILOpCode.Newobj);
                    il.Token(memberRef);

                    _lastWasNewobj = true;
                    _pendingNewobjTypeName = simpleType;
                    return;
                }
            }
            catch (Exception ex)
            {
                DebugLog($"DEBUG: Failed to synthesize MemberRef for external ctor: {ex.Message}");
            }

            // Fallback: push null so subsequent stloc/stfld won't underflow
            il.OpCode(ILOpCode.Ldnull);
            _lastWasNewobj = true;
            _pendingNewobjTypeName = null;
            return;
        }

        // Extract method name from the signature
        var methodName = ExtractMethodName(callInst?.MethodSignature ?? "");

        DebugLog($"Trying to resolve method call: '{callInst?.MethodSignature ?? "<null>"}' -> '{methodName}'");
        // Try to resolve internal method calls using the method map
        if (methodMap != null && methodMap.TryGetValue(methodName, out var methodHandle))
        {
            DebugLog($"DEBUG: Found method '{methodName}' in method map");
            // If call instruction reports fewer args than the target method expects, try to push missing args from caller's params
            if (methodParamNames != null && methodParamNames.TryGetValue(methodName, out var targetParams) && (callInst?.ArgCount ?? -1) >= 0 && (callInst?.ArgCount ?? -1) < targetParams.Count)
            {
                var currentArgCount = callInst?.ArgCount ?? 0;
                var missing = targetParams.Count - currentArgCount;
                DebugLog($"DEBUG: Auto-inserting {missing} missing arg(s) for call '{methodName}' (declared params: {string.Join(',', targetParams)})");
                for (int mi = currentArgCount; mi < targetParams.Count; mi++)
                {
                    var pname = targetParams[mi];
                    if (paramIndexMap != null && paramIndexMap.TryGetValue(pname, out var argIdx))
                    {
                        DebugLog($"DEBUG:   Inserting ldarg for param '{pname}' -> arg index {argIdx}");
                        il.LoadArgument(argIdx);
                    }
                    else
                    {
                        DebugLog($"DEBUG:   Could not find caller arg index for '{pname}', inserting default 0");
                        // Fallback: push default int
                        il.LoadConstantI4(0);
                    }
                }
            }
            il.Call(methodHandle);
            return;
        }

        // Also try to handle subclause method calls by checking for the pattern
        // This handles cases where the IL calls subclause methods directly
        if (methodName.Contains("_subclause") && methodMap != null)
        {
            if (methodMap.TryGetValue(methodName, out var subclaueueHandle))
            {
                DebugLog($"DEBUG: Found subclause method '{methodName}' in method map");
                il.Call(subclaueueHandle);
                return;
            }
        }

        // For unresolved method calls, try to find any method that starts with the same base name
        if (methodMap != null)
        {
            var baseName = methodName.Split('_')[0]; // Get base name before any suffix

            var matchingMethod = methodMap.FirstOrDefault(kvp => kvp.Key.StartsWith(baseName));
            if (!matchingMethod.Equals(default(KeyValuePair<string, MethodDefinitionHandle>)))
            {
                DebugLog($"DEBUG: Found matching method by base name '{baseName}': '{matchingMethod.Key}'");
                il.Call(matchingMethod.Value);
                return;
            }
        }

        // If still unresolved, emit a warning, consume any previously pushed args, and push a default return value based on the signature to keep stack balanced
        Console.WriteLine($"WARNING: Skipping unresolved method call: {callInst?.MethodSignature ?? "<null>"}");

        var argCount = callInst?.ArgCount ?? 0;
        for (int i = 0; i < argCount; i++)
        {
            il.OpCode(ILOpCode.Pop);
        }
        var sig = callInst?.MethodSignature ?? string.Empty;
        var retType = sig.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        switch (retType)
        {
            case "void":
                // nothing to push for void return
                break;
            case "int32":
            case "int":
            case "bool":
                il.LoadConstantI4(0);
                break;
            case "float32":
                il.LoadConstantR4(0);
                break;
            case "float64":
                il.LoadConstantR8(0);
                break;
            case "string":
            case "object":
            default:
                il.OpCode(ILOpCode.Ldnull);
                break;
        }
        // Pad with NOPs to approximate size of a call instruction (5 bytes)
        il.OpCode(ILOpCode.Nop);
        il.OpCode(ILOpCode.Nop);
        il.OpCode(ILOpCode.Nop);
        il.OpCode(ILOpCode.Nop);
    }

    /// <summary>
    /// Extract method name from method signature string
    /// </summary>
}
