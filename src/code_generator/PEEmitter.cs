using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using il_ast;

namespace code_generator;

/// <summary>
/// Emits Portable Executable files directly from IL metamodel, 
/// replacing the dependency on external ilasm.exe
/// </summary>
public class PEEmitter
{
    private static bool DebugEnabled =>
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("1", StringComparison.Ordinal) ||
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("true", StringComparison.OrdinalIgnoreCase) ||
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("on", StringComparison.OrdinalIgnoreCase);

    private static void DebugLog(string message)
    {
        if (DebugEnabled) Console.WriteLine(message);
    }
    // Maps for types, fields, and constructors defined in this assembly
    private readonly Dictionary<string, TypeDefinitionHandle> _typeHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldDefinitionHandle> _fieldHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldDefinitionHandle> _fieldHandlesByTypeAndName = new(StringComparer.Ordinal);
    // Note: We avoid MemberReferences for fields within the same module; use FieldDefinitionHandles instead
    private readonly Dictionary<TypeDefinitionHandle, string> _typeNamesByHandle = new();
    private readonly Dictionary<string, MethodDefinitionHandle> _ctorHandles = new(StringComparer.Ordinal);

    // Per-method emission state for simple local type inference
    private string? _lastLoadedLocal;
    private bool _lastWasNewobj;
    private Dictionary<string, SignatureTypeCode> _localVarTypeMap = new(StringComparer.Ordinal);
    private string? _pendingNewobjTypeName;
    private readonly Dictionary<string, TypeDefinitionHandle> _localVarClassTypeHandles = new(StringComparer.Ordinal);

    // Cached type references for common system types used in emission
    private EntityHandle _systemInt32TypeRef;

    /// <summary>
    /// Generate a PE assembly directly from IL metamodel
    /// </summary>
    /// <param name="ilAssembly">IL metamodel assembly</param>
    /// <param name="outputPath">Output assembly path</param>
    /// <returns>True if emission succeeded</returns>
    public bool EmitAssembly(AssemblyDeclaration ilAssembly, string outputPath)
    {
        try
        {
            // Generate PE assembly directly from IL metamodel
            var metadataBuilder = new MetadataBuilder();
            var blobBuilder = new BlobBuilder();

            // Collect method bodies for PE builder using MethodBodyStreamEncoder
            var methodBodyStream = new BlobBuilder();
            var methodBodyEncoder = new MethodBodyStreamEncoder(methodBodyStream);
            MethodDefinitionHandle? entryPointMethodHandle = null;

            // Ensure we have a valid assembly name - use output filename if IL assembly name is empty
            var assemblyName = string.IsNullOrWhiteSpace(ilAssembly.Name)
                ? Path.GetFileNameWithoutExtension(outputPath)
                : ilAssembly.Name;

            // Add assembly reference to mscorlib/System.Runtime
            var systemRuntimeRef = metadataBuilder.AddAssemblyReference(
                metadataBuilder.GetOrAddString("System.Runtime"),
                new System.Version(8, 0, 0, 0),
                default,
                default,
                default,
                default);

            // Add module
            var moduleHandle = metadataBuilder.AddModule(
                0,
                metadataBuilder.GetOrAddString(assemblyName),
                metadataBuilder.GetOrAddGuid(Guid.NewGuid()),
                default,
                default);

            // Add the special <Module> type definition as the first TypeDef
            var moduleTypeHandle = metadataBuilder.AddTypeDefinition(
                TypeAttributes.Class | TypeAttributes.SpecialName | TypeAttributes.RTSpecialName | TypeAttributes.NotPublic,
                default,
                metadataBuilder.GetOrAddString("<Module>"),
                default,
                MetadataTokens.FieldDefinitionHandle(1),
                MetadataTokens.MethodDefinitionHandle(1));

            // Add assembly definition
            var assemblyHandle = metadataBuilder.AddAssembly(
                metadataBuilder.GetOrAddString(assemblyName),
                new System.Version(1, 0, 0, 0),
                default,
                default,
                default,
                AssemblyHashAlgorithm.None);

            // Create a simple Program class with Main method
            var objectTypeRef = metadataBuilder.AddTypeReference(
                systemRuntimeRef,
                metadataBuilder.GetOrAddString("System"),
                metadataBuilder.GetOrAddString("Object"));

            var consoleTypeRef = metadataBuilder.AddTypeReference(
                systemRuntimeRef,
                metadataBuilder.GetOrAddString("System"),
                metadataBuilder.GetOrAddString("Console"));

            // Cache System.Int32 type reference for array element types
            _systemInt32TypeRef = metadataBuilder.AddTypeReference(
                systemRuntimeRef,
                metadataBuilder.GetOrAddString("System"),
                metadataBuilder.GetOrAddString("Int32"));

            var writeLineSignatureBlob = new BlobBuilder();
            writeLineSignatureBlob.WriteByte(0x00); // calling convention
            writeLineSignatureBlob.WriteByte(0x01); // parameter count
            writeLineSignatureBlob.WriteByte((byte)SignatureTypeCode.Void); // return type
            writeLineSignatureBlob.WriteByte((byte)SignatureTypeCode.String); // parameter type

            var writeLineMethodRef = metadataBuilder.AddMemberReference(
                consoleTypeRef,
                metadataBuilder.GetOrAddString("WriteLine"),
                metadataBuilder.GetOrAddBlob(writeLineSignatureBlob));

            // Add a reference to System.Object::.ctor for calling base constructor from our ctors
            var objectCtorSig = new BlobBuilder();
            objectCtorSig.WriteByte(0x20); // HASTHIS calling convention for instance methods
            objectCtorSig.WriteByte(0x00); // parameter count = 0
            objectCtorSig.WriteByte((byte)SignatureTypeCode.Void); // return type
            var objectCtorMemberRef = metadataBuilder.AddMemberReference(
                objectTypeRef,
                metadataBuilder.GetOrAddString(".ctor"),
                metadataBuilder.GetOrAddBlob(objectCtorSig));

            // If there are classes, emit them with fields and a default .ctor
            if (ilAssembly.PrimeModule != null && ilAssembly.PrimeModule.Classes.Count > 0)
            {
                foreach (var cls in ilAssembly.PrimeModule.Classes)
                {
                    var ns = cls.Namespace ?? "Fifth.Generated";
                    var name = cls.Name ?? "Unnamed";

                    // Determine starting Field and Method row ids for this type
                    var firstFieldRowId = metadataBuilder.GetRowCount(TableIndex.Field) + 1;
                    var firstFieldHandle = MetadataTokens.FieldDefinitionHandle(firstFieldRowId);
                    var firstMethodRowId = metadataBuilder.GetRowCount(TableIndex.MethodDef) + 1;
                    var firstMethodHandleForType = MetadataTokens.MethodDefinitionHandle(firstMethodRowId);

                    // Define the type
                    var typeHandle = metadataBuilder.AddTypeDefinition(
                        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
                        metadataBuilder.GetOrAddString(ns),
                        metadataBuilder.GetOrAddString(name),
                        objectTypeRef,
                        firstFieldHandle,
                        firstMethodHandleForType);
                    _typeHandles[name] = typeHandle;
                    _typeNamesByHandle[typeHandle] = name;

                    // Add fields
                    foreach (var field in cls.Fields)
                    {
                        var fieldSig = new BlobBuilder();
                        fieldSig.WriteByte(0x06); // FIELD signature
                        fieldSig.WriteByte((byte)GetSignatureTypeCode(field.TheType));
                        var fh = metadataBuilder.AddFieldDefinition(
                            FieldAttributes.Public,
                            metadataBuilder.GetOrAddString(field.Name ?? "field"),
                            metadataBuilder.GetOrAddBlob(fieldSig));
                        // Store by simple field name (assume unique across types for now)
                        _fieldHandles[field.Name ?? "field"] = fh;
                        _fieldHandlesByTypeAndName[$"{name}::{field.Name}"] = fh;
                    }

                    // Emit a default parameterless instance constructor
                    var ctorSig = new BlobBuilder();
                    ctorSig.WriteByte(0x20); // HASTHIS
                    ctorSig.WriteByte(0x00); // param count 0
                    ctorSig.WriteByte((byte)SignatureTypeCode.Void);

                    // Body: ldarg.0; call instance void [System.Runtime]System.Object::.ctor(); ret
                    var ilInstructions = new BlobBuilder();
                    var cf = new ControlFlowBuilder();
                    var ctorIl = new InstructionEncoder(ilInstructions, cf);
                    ctorIl.LoadArgument(0);
                    ctorIl.Call(objectCtorMemberRef);
                    ctorIl.OpCode(ILOpCode.Ret);
                    var bodyOffset = new MethodBodyStreamEncoder(methodBodyStream).AddMethodBody(
                        ctorIl,
                        maxStack: 8,
                        localVariablesSignature: default,
                        attributes: MethodBodyAttributes.None);

                    var ctorHandle = metadataBuilder.AddMethodDefinition(
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                        MethodImplAttributes.IL,
                        metadataBuilder.GetOrAddString(".ctor"),
                        metadataBuilder.GetOrAddBlob(ctorSig),
                        bodyOffset,
                        // No parameters; ParamList should point at next Param row
                        MetadataTokens.ParameterHandle(metadataBuilder.GetRowCount(TableIndex.Param) + 1));

                    _ctorHandles[name] = ctorHandle;
                }
            }

            // Process methods from IL metamodel
            if (ilAssembly.PrimeModule == null || ilAssembly.PrimeModule.Functions.Count == 0)
            {
                // Add empty Program type as fallback
                var programTypeHandle = metadataBuilder.AddTypeDefinition(
                    TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    default,
                    metadataBuilder.GetOrAddString("Program"),
                    objectTypeRef,
                    // Program has no fields; FieldList should point past the last existing field
                    MetadataTokens.FieldDefinitionHandle(metadataBuilder.GetRowCount(TableIndex.Field) + 1),
                    MetadataTokens.MethodDefinitionHandle(1));
            }
            else
            {
                // Process all functions in the module
                var functions = ilAssembly.PrimeModule.Functions.ToList();

                if (functions.Any())
                {
                    // Reserve the method row id that will be the first method of Program
                    var firstMethodRowId = metadataBuilder.GetRowCount(TableIndex.MethodDef) + 1;
                    var firstMethodHandle = MetadataTokens.MethodDefinitionHandle(firstMethodRowId);

                    // Add Program type with correct MethodList pointing at first future method
                    var programTypeHandle = metadataBuilder.AddTypeDefinition(
                        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                        default,
                        metadataBuilder.GetOrAddString("Program"),
                        objectTypeRef,
                        // Program has no fields; FieldList should point past the last existing field
                        MetadataTokens.FieldDefinitionHandle(metadataBuilder.GetRowCount(TableIndex.Field) + 1),
                        firstMethodHandle);

                    // Pre-compute MethodDefinitionHandles so calls can reference them before they're defined
                    var functionList = functions.ToList();
                    var methodMap = new Dictionary<string, MethodDefinitionHandle>(StringComparer.Ordinal);
                    for (int i = 0; i < functionList.Count; i++)
                    {
                        var fn = functionList[i];
                        var methodDefHandle = MetadataTokens.MethodDefinitionHandle(firstMethodRowId + i);
                        methodMap[fn.Name] = methodDefHandle;
                    }

                    // Generate method bodies and capture their offsets (RVAs within method body stream)
                    var bodyOffsets = new Dictionary<string, int>(StringComparer.Ordinal);
                    foreach (var function in functionList)
                    {
                        var (il, localVariables, localTypes) = GenerateMethodIL(function, metadataBuilder, writeLineMethodRef, methodMap);

                        StandaloneSignatureHandle localVarSigToken = default;
                        if (localVariables.Any())
                        {
                            localVarSigToken = (StandaloneSignatureHandle)CreateLocalVariableSignature(metadataBuilder, localVariables, localTypes);
                        }

                        var bodyOffset = methodBodyEncoder.AddMethodBody(
                            il,
                            maxStack: 8,
                            localVariablesSignature: localVarSigToken,
                            attributes: localVariables.Any() ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None);

                        bodyOffsets[function.Name] = bodyOffset;
                        DebugLog($"DEBUG: Added body for '{function.Name}' at offset {bodyOffset}");
                    }

                    // Create Parameter rows and remember ParamList start for each method
                    var paramListStartMap = new Dictionary<string, ParameterHandle>(StringComparer.Ordinal);
                    foreach (var function in functionList)
                    {
                        var currentParamRowCount = metadataBuilder.GetRowCount(TableIndex.Param);
                        var startRowId = currentParamRowCount + 1;

                        for (ushort seq = 1; seq <= function.Signature.ParameterSignatures.Count; seq++)
                        {
                            var paramSig = function.Signature.ParameterSignatures[seq - 1];
                            var paramName = paramSig.Name ?? string.Empty;
                            metadataBuilder.AddParameter(
                                ParameterAttributes.None,
                                metadataBuilder.GetOrAddString(paramName),
                                seq);
                        }

                        paramListStartMap[function.Name] = MetadataTokens.ParameterHandle(startRowId);
                    }

                    // Now add MethodDefinitions pointing at the generated bodies with ParamList
                    foreach (var function in functionList)
                    {
                        var methodSignatureBlob = new BlobBuilder();
                        methodSignatureBlob.WriteByte(0x00); // DEFAULT calling convention
                        methodSignatureBlob.WriteByte((byte)function.Signature.ParameterSignatures.Count); // parameter count

                        // Return type
                        WriteTypeInSignature(methodSignatureBlob, function.Signature.ReturnTypeSignature);
                        // Parameter types
                        foreach (var paramSig in function.Signature.ParameterSignatures)
                        {
                            WriteTypeInSignature(methodSignatureBlob, paramSig.TypeReference);
                        }

                        var methodHandle = metadataBuilder.AddMethodDefinition(
                            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                            MethodImplAttributes.IL,
                            metadataBuilder.GetOrAddString(function.Header.IsEntrypoint ? "Main" : function.Name),
                            metadataBuilder.GetOrAddBlob(methodSignatureBlob),
                            bodyOffsets[function.Name],
                            paramListStartMap[function.Name]);

                        if (function.Header.IsEntrypoint)
                        {
                            entryPointMethodHandle = methodHandle;
                        }
                    }
                }
                else
                {
                    // Add empty Program type as fallback
                    var programTypeHandle = metadataBuilder.AddTypeDefinition(
                        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                        default,
                        metadataBuilder.GetOrAddString("Program"),
                        objectTypeRef,
                        // Program has no fields; FieldList should point past the last existing field
                        MetadataTokens.FieldDefinitionHandle(metadataBuilder.GetRowCount(TableIndex.Field) + 1),
                        MetadataTokens.MethodDefinitionHandle(1));
                }
            }

            // Build PE with method bodies using original approach but fixed method format
            var peHeaderBuilder = new PEHeaderBuilder(imageCharacteristics: Characteristics.ExecutableImage);

            var peBuilder = new ManagedPEBuilder(
                peHeaderBuilder,
                new MetadataRootBuilder(metadataBuilder),
                methodBodyStream,
                entryPoint: entryPointMethodHandle ?? default);

            var peBlob = new BlobBuilder();
            peBuilder.Serialize(peBlob);

            // Write to file
            using var stream = File.Create(outputPath);
            peBlob.WriteContentTo(stream);

            return true;
        }
        catch (System.Exception ex)
        {
            // Log error for debugging
            Console.WriteLine($"PE emission failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Generate method body from IL metamodel method definition
    /// </summary>
    private (InstructionEncoder il, List<string> localVariables, Dictionary<string, SignatureTypeCode> localTypes) GenerateMethodIL(il_ast.MethodDefinition ilMethod, MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, Dictionary<string, MethodDefinitionHandle> methodMap)
    {
        var ilInstructions = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(ilInstructions, controlFlow);

        // Reset per-method inference state
        _lastLoadedLocal = null;
        _lastWasNewobj = false;
        _localVarTypeMap = new Dictionary<string, SignatureTypeCode>(StringComparer.Ordinal);
        _pendingNewobjTypeName = null;
        _localVarClassTypeHandles.Clear();

        // Use AstToIlTransformationVisitor to get instruction sequences for each statement
        var transformer = new AstToIlTransformationVisitor();
        // Provide current parameter names to the transformer so it can emit ldarg for them
        var paramNames = ilMethod?.Signature?.ParameterSignatures?.Select(p => p.Name ?? "param").ToList() ?? new List<string>();
        transformer.SetCurrentParameters(paramNames);
        // Build a map from parameter name to argument index
        var paramIndexMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < paramNames.Count; i++) paramIndexMap[paramNames[i]] = i;

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

        var bodyStatements = ilMethod?.Impl?.Body?.Statements ?? new List<ast.Statement>();
        DebugLog($"DEBUG: Generating method body for '{ilMethod?.Name ?? "Unnamed"}' with {bodyStatements.Count} statements");

        // Generate instructions from the method's body statements
        if (bodyStatements.Any())
        {
            foreach (var statement in bodyStatements)
            {
                var instructionSequence = transformer.GenerateStatement(statement);

                DebugLog($"DEBUG: Statement generated {instructionSequence.Instructions.Count} instructions");
                if (DebugEnabled)
                {
                    foreach (var instr in instructionSequence.Instructions)
                    {
                        Console.WriteLine($"  - {instr.GetType().Name}: {instr}");
                    }
                }

                // Collect local variable information
                foreach (var instruction in instructionSequence.Instructions)
                {
                    if (instruction is il_ast.LoadInstruction loadInst &&
                            loadInst.Opcode.ToLowerInvariant() == "ldloc" &&
                            loadInst.Value is string loadVar)
                    {
                        AddLocal(loadVar);
                    }
                    else if (instruction is il_ast.StoreInstruction storeInst &&
                            storeInst.Opcode.ToLowerInvariant() == "stloc" &&
                            storeInst.Target is string storeVar)
                    {
                        AddLocal(storeVar);
                    }
                }

                EmitInstructionSequence(il, instructionSequence, metadataBuilder, writeLineMethodRef, methodMap, localVariables, paramIndexMap);
            }
        }
        else
        {
            // If no statements, add a simple return for the method to be valid
            // For int return type, load a constant first
            var returnType = ilMethod?.Signature?.ReturnTypeSignature?.Name ?? string.Empty;
            if (returnType == "Int32")
            {
                il.LoadConstantI4(42); // Default return value
            }
        }

        // Always ensure a final return exists with a sensible default value
        var retTypeName = ilMethod?.Signature?.ReturnTypeSignature?.Name ?? "Void";
        switch (retTypeName)
        {
            case "Void":
                il.OpCode(ILOpCode.Ret);
                break;
            case "Int32":
            case "Boolean":
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ret);
                break;
            case "Single":
                il.LoadConstantR4(0);
                il.OpCode(ILOpCode.Ret);
                break;
            case "Double":
                il.LoadConstantR8(0);
                il.OpCode(ILOpCode.Ret);
                break;
            case "String":
            default:
                il.OpCode(ILOpCode.Ldnull);
                il.OpCode(ILOpCode.Ret);
                break;
        }
        return (il, localVariables, _localVarTypeMap);
    }

    /// <summary>
    /// Create a local variable signature for the method
    /// </summary>
    private EntityHandle CreateLocalVariableSignature(MetadataBuilder metadataBuilder, List<string> localVariables, Dictionary<string, SignatureTypeCode> localTypes)
    {
        var localsSignature = new BlobBuilder();
        localsSignature.WriteByte(0x07); // LOCAL_SIG
        localsSignature.WriteCompressedInteger(localVariables.Count); // Number of locals

        // Use inferred types where available (default Int32)
        foreach (var localVar in localVariables)
        {
            // Prefer explicit class type if known (from newobj)
            if (_localVarClassTypeHandles.TryGetValue(localVar, out var typeDefHandle))
            {
                // ELEMENT_TYPE_CLASS (0x12) then TypeDefOrRef coded index for the type
                localsSignature.WriteByte(0x12);
                var rowId = MetadataTokens.GetRowNumber(typeDefHandle);
                var codedIndex = (rowId << 2) | 0; // TypeDefOrRef tag 0 = TypeDef
                localsSignature.WriteCompressedInteger(codedIndex);
            }
            else
            {
                if (!localTypes.TryGetValue(localVar, out var sigType))
                {
                    sigType = SignatureTypeCode.Int32;
                }
                localsSignature.WriteByte((byte)sigType);
            }
        }

        return metadataBuilder.AddStandaloneSignature(metadataBuilder.GetOrAddBlob(localsSignature));
    }

    /// <summary>
    /// Emit instruction sequence using InstructionEncoder
    /// </summary>
    private void EmitInstructionSequence(InstructionEncoder il, il_ast.InstructionSequence sequence,
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, Dictionary<string, MethodDefinitionHandle>? methodMap = null, List<string>? orderedLocals = null, Dictionary<string, int>? paramIndexMap = null, Dictionary<string, EntityHandle>? methodMemberRefMap = null)
    {
        // Predefine labels present in this sequence so branches can target them
        var labelMap = new Dictionary<string, LabelHandle>(StringComparer.Ordinal);
        foreach (var ins in sequence.Instructions)
        {
            if (ins is il_ast.LabelInstruction li && !string.IsNullOrEmpty(li.Label) && !labelMap.ContainsKey(li.Label))
            {
                labelMap[li.Label] = il.DefineLabel();
            }
        }

        // Use provided locals ordering if available; otherwise derive per-sequence
        var localVarNames = orderedLocals != null ? new List<string>(orderedLocals) : new List<string>();
        if (orderedLocals == null)
        {
            foreach (var instruction in sequence.Instructions)
            {
                if (instruction is il_ast.LoadInstruction loadInst &&
                    loadInst.Opcode.ToLowerInvariant() == "ldloc" &&
                    loadInst.Value is string loadVar &&
                    !localVarNames.Contains(loadVar))
                {
                    localVarNames.Add(loadVar);
                }
                else if (instruction is il_ast.StoreInstruction storeInst &&
                        storeInst.Opcode.ToLowerInvariant() == "stloc" &&
                        storeInst.Target is string storeVar &&
                        !localVarNames.Contains(storeVar))
                {
                    localVarNames.Add(storeVar);
                }
            }
        }

        foreach (var instruction in sequence.Instructions)
        {
            try
            {
                EmitInstruction(il, instruction, metadataBuilder, writeLineMethodRef, localVarNames, methodMap, labelMap, paramIndexMap, methodMemberRefMap);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed emitting instruction {instruction.GetType().Name} (Opcode='{instruction.Opcode}') : {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Emit a single instruction using InstructionEncoder
    /// </summary>
    private void EmitInstruction(InstructionEncoder il, il_ast.CilInstruction instruction,
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, List<string>? localVarNames = null, Dictionary<string, MethodDefinitionHandle>? methodMap = null, Dictionary<string, LabelHandle>? labelMap = null, Dictionary<string, int>? paramIndexMap = null, Dictionary<string, EntityHandle>? methodMemberRefMap = null)
    {
        switch (instruction)
        {
            case il_ast.LoadInstruction loadInst:
                EmitLoadInstruction(il, loadInst, metadataBuilder, localVarNames ?? new List<string>(), paramIndexMap ?? new Dictionary<string, int>(StringComparer.Ordinal));
                break;

            case il_ast.StoreInstruction storeInst:
                EmitStoreInstruction(il, storeInst, localVarNames ?? new List<string>());
                break;

            case il_ast.ArithmeticInstruction arithInst:
                EmitArithmeticInstruction(il, arithInst);
                break;

            case il_ast.CallInstruction callInst:
                EmitCallInstruction(il, callInst, metadataBuilder, writeLineMethodRef, methodMap);
                break;

            case il_ast.BranchInstruction branchInst:
                EmitBranchInstruction(il, branchInst, labelMap);
                break;

            case il_ast.ReturnInstruction:
                il.OpCode(ILOpCode.Ret);
                break;

            case il_ast.LabelInstruction labelInst:
                if (labelMap != null && !string.IsNullOrEmpty(labelInst.Label) && labelMap.TryGetValue(labelInst.Label, out var lh))
                {
                    il.MarkLabel(lh);
                }
                break;

            case il_ast.StackInstruction stackInst:
                if (string.Equals(stackInst.Opcode, "pop", StringComparison.OrdinalIgnoreCase))
                {
                    il.OpCode(ILOpCode.Pop);
                }
                break;
        }
    }

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
                // Expect Value to be element type hint like "int32" for now
                // Stack: (size) -> (array ref)
                il.OpCode(ILOpCode.Newarr);
                // Use cached System.Int32 for now
                il.Token(_systemInt32TypeRef);
                // Mark that a reference type is produced for subsequent stloc typing as object
                _lastWasNewobj = true;
                _pendingNewobjTypeName = null;
                break;
            case "ldelem.i4":
                il.OpCode(ILOpCode.Ldelem_i4);
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
                    // Remove quotes if present
                    var cleanString = stringValue.Trim('"');
                    il.LoadString(metadataBuilder.GetOrAddUserString(cleanString));
                }
                break;

            case "ldloc":
                if (loadInst.Value is string varName && localVarNames != null)
                {
                    var index = localVarNames.IndexOf(varName);
                    if (index >= 0)
                    {
                        il.LoadLocal(index);
                        _lastLoadedLocal = varName;
                    }
                    else
                    {
                        // Unknown local; push default int to keep stack balanced
                        il.LoadConstantI4(0);
                        _lastLoadedLocal = null;
                    }
                }
                else
                {
                    // Fallback for non-string value or no local var names
                    il.LoadConstantI4(0);
                    _lastLoadedLocal = null;
                }
                break;
            case "ldarg":
                if (loadInst.Value is string argName && paramIndexMap != null && paramIndexMap.TryGetValue(argName, out var argIndex))
                {
                    il.LoadArgument(argIndex);
                    _lastLoadedLocal = null;
                }
                else
                {
                    // Unknown arg; push default int
                    il.LoadConstantI4(0);
                    _lastLoadedLocal = null;
                }
                break;

            case "ldfld":
                if (loadInst.Value is string fldName)
                {
                    // Resolve FieldDefinitionHandle for the field. Prefer exact owner type if known.
                    EntityHandle? fieldToken = null;
                    if (!string.IsNullOrEmpty(_lastLoadedLocal) && _localVarClassTypeHandles.TryGetValue(_lastLoadedLocal, out var typeHandle) && _typeNamesByHandle.TryGetValue(typeHandle, out var typeName))
                    {
                        var keyDef = $"{typeName}::{fldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef))
                        {
                            fieldToken = fdef;
                        }
                    }

                    // Fallback by simple name
                    if (fieldToken == null && _fieldHandles.TryGetValue(fldName, out var fldHandle))
                    {
                        fieldToken = fldHandle;
                    }

                    if (fieldToken != null)
                    {
                        il.OpCode(ILOpCode.Ldfld);
                        il.Token(fieldToken.Value);
                        if (!string.IsNullOrEmpty(_lastLoadedLocal))
                        {
                            _localVarTypeMap[_lastLoadedLocal] = SignatureTypeCode.Object;
                            _lastLoadedLocal = null;
                        }
                    }
                    else
                    {
                        // Unknown field: keep stack balanced but degrade gracefully
                        il.OpCode(ILOpCode.Pop);
                        il.LoadConstantI4(0);
                        _lastLoadedLocal = null;
                    }
                }
                break;

            case "dup":
                // Duplicate the top value on the stack
                il.OpCode(ILOpCode.Dup);
                break;
        }
    }

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
                            _localVarTypeMap[varName] = SignatureTypeCode.Object;
                            if (!string.IsNullOrEmpty(_pendingNewobjTypeName) && _typeHandles.TryGetValue(_pendingNewobjTypeName, out var tdh))
                            {
                                _localVarClassTypeHandles[varName] = tdh;
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
                }
                else
                {
                    // Fallback for non-string value or no local var names
                    il.StoreLocal(0);
                    _lastWasNewobj = false;
                    _pendingNewobjTypeName = null;
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
                    if (!string.IsNullOrEmpty(_lastLoadedLocal) && _localVarClassTypeHandles.TryGetValue(_lastLoadedLocal, out var typeHandle) && _typeNamesByHandle.TryGetValue(typeHandle, out var typeName))
                    {
                        var keyDef = $"{typeName}::{fieldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef))
                        {
                            fieldToken = fdef;
                        }
                    }

                    // Fallback by simple name
                    if (fieldToken == null && _fieldHandles.TryGetValue(fieldName, out var fldHandle2))
                    {
                        fieldToken = fldHandle2;
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
                    }
                    else
                    {
                        // Unknown field: consume value and obj to avoid corrupting the stack
                        il.OpCode(ILOpCode.Pop); // value
                        il.OpCode(ILOpCode.Pop); // obj
                        _lastLoadedLocal = null;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Emit arithmetic instruction
    /// </summary>
    private void EmitArithmeticInstruction(InstructionEncoder il, il_ast.ArithmeticInstruction arithInst)
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
            case "and":
                il.OpCode(ILOpCode.And);
                break;
            case "not":
                // Logical not for booleans: compare with 0
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ceq);
                break;
            case "neg":
                il.OpCode(ILOpCode.Neg);
                break;
        }
    }

    /// <summary>
    /// Emit call instruction
    /// </summary>
    private void EmitCallInstruction(InstructionEncoder il, il_ast.CallInstruction callInst,
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, Dictionary<string, MethodDefinitionHandle>? methodMap = null)
    {
        DebugLog($"DEBUG: EmitCallInstruction opcode='{callInst.Opcode}', sig='{callInst.MethodSignature}'");
        // Handle external static call signature produced by AstToIlTransformationVisitor
        if ((callInst.MethodSignature ?? string.Empty).StartsWith("extcall:", StringComparison.Ordinal))
        {
            try
            {
                var extSig = callInst.MethodSignature!; // format: extcall:Asm=...;Ns=...;Type=...;Method=...;Params=...;Return=...
                var parts = extSig.Substring("extcall:".Length).Split(';', StringSplitOptions.RemoveEmptyEntries);
                var dict = parts.Select(p => p.Split('=')).Where(a => a.Length == 2).ToDictionary(a => a[0], a => a[1]);
                dict.TryGetValue("Asm", out var asmName);
                dict.TryGetValue("Ns", out var ns);
                dict.TryGetValue("Type", out var typeName);
                dict.TryGetValue("Method", out var extMethodName);
                dict.TryGetValue("Params", out var paramList);
                dict.TryGetValue("Return", out var returnToken);

                // Create an AssemblyRef for the external assembly (fallback to System.Runtime for metadata needs)
                var asmRef = metadataBuilder.AddAssemblyReference(
                    metadataBuilder.GetOrAddString(string.IsNullOrWhiteSpace(asmName) ? "Fifth.System" : asmName),
                    new System.Version(1, 0, 0, 0), default, default, default, default);

                // Create a TypeRef for the external type
                var typeRef = metadataBuilder.AddTypeReference(
                    asmRef,
                    metadataBuilder.GetOrAddString(string.IsNullOrWhiteSpace(ns) ? "Fifth.System" : ns),
                    metadataBuilder.GetOrAddString(typeName ?? "KG"));

                // Helper to write a type token (e.g., System.Int32 or Namespace.Type@Asm) to signature
                void WriteTypeToken(BlobBuilder sigBuilder, string token, string? fallbackAsm)
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        sigBuilder.WriteByte((byte)SignatureTypeCode.Object);
                        return;
                    }
                    var t = token.Trim();
                    // System primitives and common types
                    switch (t)
                    {
                        case "System.Void": sigBuilder.WriteByte((byte)SignatureTypeCode.Void); return;
                        case "System.Int32": sigBuilder.WriteByte((byte)SignatureTypeCode.Int32); return;
                        case "System.String": sigBuilder.WriteByte((byte)SignatureTypeCode.String); return;
                        case "System.Single": sigBuilder.WriteByte((byte)SignatureTypeCode.Single); return;
                        case "System.Double": sigBuilder.WriteByte((byte)SignatureTypeCode.Double); return;
                        case "System.Boolean": sigBuilder.WriteByte((byte)SignatureTypeCode.Boolean); return;
                        case "System.Object": sigBuilder.WriteByte((byte)SignatureTypeCode.Object); return;
                    }

                    // External/class types: expect "Full.Name@Asm" or "Full.Name" (use fallback assembly)
                    string fullName = t;
                    string asm = fallbackAsm ?? "Fifth.System";
                    var atIdx = t.LastIndexOf('@');
                    if (atIdx > 0)
                    {
                        fullName = t.Substring(0, atIdx);
                        asm = t.Substring(atIdx + 1);
                    }

                    // Split namespace and type name
                    string typeNs = string.Empty;
                    string simpleName = fullName;
                    var lastDot = fullName.LastIndexOf('.');
                    if (lastDot > 0)
                    {
                        typeNs = fullName.Substring(0, lastDot);
                        simpleName = fullName.Substring(lastDot + 1);
                    }
                    // Build AssemblyRef and TypeRef
                    var paramAsmRef = metadataBuilder.AddAssemblyReference(
                        metadataBuilder.GetOrAddString(asm),
                        new System.Version(1, 0, 0, 0), default, default, default, default);
                    var paramTypeRef = metadataBuilder.AddTypeReference(
                        paramAsmRef,
                        metadataBuilder.GetOrAddString(typeNs),
                        metadataBuilder.GetOrAddString(simpleName));

                    // Write CLASS + TypeDefOrRef coded index (TypeRef tag = 1)
                    sigBuilder.WriteByte(0x12);
                    var rowId = MetadataTokens.GetRowNumber((TypeReferenceHandle)paramTypeRef);
                    var codedIndex = (rowId << 2) | 1; // tag 1 = TypeRef
                    sigBuilder.WriteCompressedInteger(codedIndex);
                }

                // Build signature using accurate param and return types
                var methodSig = new BlobBuilder();
                methodSig.WriteByte(0x00); // DEFAULT
                var paramTokens = new List<string>();
                if (!string.IsNullOrWhiteSpace(paramList))
                {
                    paramTokens = paramList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                methodSig.WriteByte((byte)paramTokens.Count);
                WriteTypeToken(methodSig, string.IsNullOrWhiteSpace(returnToken) ? "System.Object" : returnToken!, asmName);
                foreach (var pTok in paramTokens)
                {
                    WriteTypeToken(methodSig, pTok, asmName);
                }

                var memberRef = metadataBuilder.AddMemberReference(
                    typeRef,
                    metadataBuilder.GetOrAddString(extMethodName ?? "Unknown"),
                    metadataBuilder.GetOrAddBlob(methodSig));

                il.Call(memberRef);
                return;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"WARNING: Failed to emit external call: {ex.Message}");
                // Fall through to unresolved behavior below
            }
        }
        // Handle constructor calls (newobj)
        if (callInst.Opcode?.ToLowerInvariant() == "newobj")
        {
            DebugLog($"DEBUG: Emitting newobj for: {callInst.MethodSignature}");
            // Try to extract type name and resolve our emitted constructor
            var typeName = ExtractCtorTypeName(callInst.MethodSignature ?? string.Empty);
            if (!string.IsNullOrEmpty(typeName) && _ctorHandles.TryGetValue(typeName, out var ctorHandle))
            {
                il.OpCode(ILOpCode.Newobj);
                il.Token(ctorHandle);
                _lastWasNewobj = true;
                _pendingNewobjTypeName = typeName;
                return;
            }
            // Fallback: push null
            il.OpCode(ILOpCode.Ldnull);
            _lastWasNewobj = true;
            _pendingNewobjTypeName = null;
            return;
        }

        // For external calls, redirect print calls to Console.WriteLine
        if (callInst.MethodSignature?.Contains("Console") == true ||
            callInst.MethodSignature?.Contains("WriteLine") == true ||
            callInst.MethodSignature?.Contains("print") == true)
        {
            il.Call(writeLineMethodRef);
            return;
        }

        // Extract method name from the signature
        var methodName = ExtractMethodName(callInst.MethodSignature ?? "");

        DebugLog($"DEBUG: Trying to resolve method call: '{callInst.MethodSignature}' -> '{methodName}'");
        if (methodMap != null && DebugEnabled)
        {
            Console.WriteLine("DEBUG: Available methods:");
            foreach (var k in methodMap.Keys)
            {
                Console.WriteLine($"  - {k}");
            }
        }

        // Try to resolve internal method calls using the method map
        if (methodMap != null && methodMap.TryGetValue(methodName, out var methodHandle))
        {
            DebugLog($"DEBUG: Found method '{methodName}' in method map");
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

        // If still unresolved, emit a warning and push a default value based on the signature to keep stack balanced
        Console.WriteLine($"WARNING: Skipping unresolved method call: {callInst.MethodSignature}");
        var sig = callInst.MethodSignature ?? string.Empty;
        var retType = sig.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant() ?? string.Empty;
        switch (retType)
        {
            case "void":
                // nothing to push
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
    private string ExtractMethodName(string methodSignature)
    {
        // Simple extraction - assume method signature format like "methodName" or "Program::methodName"
        if (methodSignature.Contains("::"))
        {
            return methodSignature.Split("::").Last();
        }

        // If it looks like a simple method name, return as-is
        if (!methodSignature.Contains(" ") && !methodSignature.Contains("("))
        {
            return methodSignature;
        }

        // For more complex signatures, try to extract the method name before parentheses
        var parenIndex = methodSignature.IndexOf('(');
        if (parenIndex > 0)
        {
            var beforeParen = methodSignature.Substring(0, parenIndex).Trim();
            var spaceIndex = beforeParen.LastIndexOf(' ');
            if (spaceIndex >= 0)
            {
                return beforeParen.Substring(spaceIndex + 1);
            }
            return beforeParen;
        }

        return methodSignature;
    }

    /// <summary>
    /// Extract type name from a constructor signature like "instance void TypeName::.ctor()"
    /// </summary>
    private string ExtractCtorTypeName(string ctorSignature)
    {
        try
        {
            // Expect format: "instance void TypeName::.ctor()" or "void Namespace.TypeName::.ctor()"
            var sep = ctorSignature.IndexOf("::", StringComparison.Ordinal);
            if (sep > 0)
            {
                // Find the last space before '::' to skip return type
                var lastSpaceBeforeSep = ctorSignature.LastIndexOf(' ', sep);
                var start = lastSpaceBeforeSep >= 0 ? lastSpaceBeforeSep + 1 : 0;
                var typePart = ctorSignature.Substring(start, sep - start).Trim();
                // Remove any namespace qualification if present; our keys are simple names
                var simple = typePart.Contains('.') ? typePart.Split('.').Last() : typePart;
                return simple;
            }
        }
        catch { }
        return string.Empty;
    }

    /// <summary>
    /// Emit branch instruction
    /// </summary>
    private void EmitBranchInstruction(InstructionEncoder il, il_ast.BranchInstruction branchInst, Dictionary<string, LabelHandle>? labelMap)
    {
        DebugLog($"DEBUG: EmitBranchInstruction called with opcode: {branchInst.Opcode}, label: {branchInst.TargetLabel ?? "null"}");

        if (labelMap == null || string.IsNullOrEmpty(branchInst.TargetLabel) || !labelMap.TryGetValue(branchInst.TargetLabel, out var target))
        {
            Console.WriteLine("WARNING: Branch target label not found; skipping branch emission.");
            return;
        }

        switch (branchInst.Opcode.ToLowerInvariant())
        {
            case "br":
                il.Branch(ILOpCode.Br, target);
                break;
            case "brtrue":
                il.Branch(ILOpCode.Brtrue, target);
                break;
            case "brfalse":
                il.Branch(ILOpCode.Brfalse, target);
                break;
            default:
                Console.WriteLine($"WARNING: Unsupported branch opcode '{branchInst.Opcode}', emitting unconditional br");
                il.Branch(ILOpCode.Br, target);
                break;
        }
    }

    /// <summary>
    /// Map IL metamodel type to SignatureTypeCode
    /// </summary>
    private SignatureTypeCode GetSignatureTypeCode(il_ast.TypeReference typeRef)
    {
        if (typeRef.Namespace == "System")
        {
            return typeRef.Name switch
            {
                "Void" => SignatureTypeCode.Void,
                "Int32" => SignatureTypeCode.Int32,
                "String" => SignatureTypeCode.String,
                "Single" => SignatureTypeCode.Single,
                "Double" => SignatureTypeCode.Double,
                "Boolean" => SignatureTypeCode.Boolean,
                _ => SignatureTypeCode.Object
            };
        }

        return SignatureTypeCode.Object;
    }

    private void WriteTypeInSignature(BlobBuilder builder, il_ast.TypeReference typeRef)
    {
        if (typeRef.Namespace == "System")
        {
            builder.WriteByte((byte)GetSignatureTypeCode(typeRef));
            return;
        }

        // User-defined types: encode as CLASS with TypeDefOrRef coded index
        if (!string.IsNullOrEmpty(typeRef.Name) && _typeHandles.TryGetValue(typeRef.Name, out var typeHandle))
        {
            builder.WriteByte(0x12); // ELEMENT_TYPE_CLASS
            var rowId = MetadataTokens.GetRowNumber(typeHandle);
            var codedIndex = (rowId << 2) | 0; // TypeDefOrRef tag 0 = TypeDef
            builder.WriteCompressedInteger(codedIndex);
            return;
        }

        // Fallback to object if type not found
        builder.WriteByte((byte)SignatureTypeCode.Object);
    }
}