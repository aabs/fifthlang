using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using il_ast;
using static Fifth.DebugHelpers;
using code_generator.InstructionEmitter;

namespace code_generator;

/// <summary>
/// Emits Portable Executable files directly from IL metamodel, 
/// replacing the dependency on external ilasm.exe
/// </summary>
public partial class PEEmitter
{
    // Use shared DebugHelpers for debug logging.

    // Extracted instruction emitters for better modularization
    private readonly BranchInstructionEmitter _branchEmitter = new();
    private readonly ArithmeticInstructionEmitter _arithmeticEmitter = new();

    // Maps for types, fields, and constructors defined in this assembly
    private readonly Dictionary<string, TypeDefinitionHandle> _typeHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldDefinitionHandle> _fieldHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldDefinitionHandle> _fieldHandlesByTypeAndName = new(StringComparer.Ordinal);
    // Note: We avoid MemberReferences for fields within the same module; use FieldDefinitionHandles instead
    private readonly Dictionary<TypeDefinitionHandle, string> _typeNamesByHandle = new();
    private readonly Dictionary<string, MethodDefinitionHandle> _ctorHandles = new(StringComparer.Ordinal);
    // Track declared field types so we can propagate local class typing from ldfld results
    private readonly Dictionary<FieldDefinitionHandle, il_ast.TypeReference> _fieldDeclaredTypes = new();

    // Per-method emission state for simple local type inference
    private string? _lastLoadedLocal;
    private bool _lastWasNewobj;
    private Dictionary<string, SignatureTypeCode> _localVarTypeMap = new(StringComparer.Ordinal);
    private string? _pendingNewobjTypeName;
    private readonly Dictionary<string, TypeDefinitionHandle> _localVarClassTypeHandles = new(StringComparer.Ordinal);
    // Track the class type of the current stack top when known (e.g., result of ldfld/newobj)
    private TypeDefinitionHandle? _pendingStackTopClassType;
    // Track parameter class types per-method and the last loaded parameter name
    private readonly Dictionary<string, TypeDefinitionHandle> _paramClassTypeHandles = new(StringComparer.Ordinal);
    private string? _lastLoadedParam;

    // Cached type references for common system types used in emission
    private EntityHandle _systemInt32TypeRef;
    // Cache assembly and typeref handles to avoid creating duplicate metadata rows
    private readonly Dictionary<string, AssemblyReferenceHandle> _assemblyRefHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TypeReferenceHandle> _typeRefHandlesCache = new(StringComparer.Ordinal);

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

            // Add assembly reference to System.Runtime (core types) and System.Console (Console APIs)
            var systemRuntimeRef = metadataBuilder.AddAssemblyReference(
                metadataBuilder.GetOrAddString("System.Runtime"),
                new System.Version(8, 0, 0, 0),
                default,
                default,
                default,
                default);
            _assemblyRefHandles["system.runtime"] = systemRuntimeRef;

            var systemConsoleRef = metadataBuilder.AddAssemblyReference(
                metadataBuilder.GetOrAddString("System.Console"),
                new System.Version(8, 0, 0, 0),
                default,
                default,
                default,
                default);
            _assemblyRefHandles["system.console"] = systemConsoleRef;

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
                systemConsoleRef,
                metadataBuilder.GetOrAddString("System"),
                metadataBuilder.GetOrAddString("Console"));

            // Cache System.Int32 type reference for array element types
            _systemInt32TypeRef = metadataBuilder.AddTypeReference(
                systemRuntimeRef,
                metadataBuilder.GetOrAddString("System"),
                metadataBuilder.GetOrAddString("Int32"));

            // Note: do not prebuild a single WriteLine MemberRef here. Instead create
            // MemberRefs for Console.WriteLine per-call so the signature matches the
            // actual overload (e.g., WriteLine(string) vs WriteLine(object)).

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
                        // Encode precise type: primitives via SignatureTypeCode; user classes via CLASS TypeDefOrRef
                        if (field.TheType.Namespace == "System")
                        {
                            fieldSig.WriteByte((byte)GetSignatureTypeCode(field.TheType));
                        }
                        else if (!string.IsNullOrEmpty(field.TheType.Name) && _typeHandles.TryGetValue(field.TheType.Name, out var fTypeDef))
                        {
                            fieldSig.WriteByte(0x12); // ELEMENT_TYPE_CLASS
                            var rowId = MetadataTokens.GetRowNumber(fTypeDef);
                            var codedIndex = (rowId << 2) | 0; // TypeDefOrRef tag 0 = TypeDef
                            fieldSig.WriteCompressedInteger(codedIndex);
                        }
                        else
                        {
                            // Fallback
                            fieldSig.WriteByte((byte)SignatureTypeCode.Object);
                        }
                        var fh = metadataBuilder.AddFieldDefinition(
                            FieldAttributes.Public,
                            metadataBuilder.GetOrAddString(field.Name ?? "field"),
                            metadataBuilder.GetOrAddBlob(fieldSig));
                        // Store by simple field name (assume unique across types for now)
                        _fieldHandles[field.Name ?? "field"] = fh;
                        _fieldHandlesByTypeAndName[$"{name}::{field.Name}"] = fh;
                        // Remember declared field type for later local typing propagation
                        _fieldDeclaredTypes[fh] = field.TheType;
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
                    // Initialize user-defined class fields to non-null instances
                    foreach (var field in cls.Fields)
                    {
                        try
                        {
                            if (field.TheType != null && !string.Equals(field.TheType.Namespace, "System", StringComparison.Ordinal) && !string.IsNullOrEmpty(field.TheType.Name))
                            {
                                if (_ctorHandles.TryGetValue(field.TheType.Name!, out var depCtor))
                                {
                                    // this.<field> = new <Type>()
                                    ctorIl.LoadArgument(0);
                                    ctorIl.OpCode(ILOpCode.Newobj);
                                    ctorIl.Token(depCtor);
                                    if (_fieldHandlesByTypeAndName.TryGetValue($"{name}::{field.Name}", out var fHandle))
                                    {
                                        ctorIl.OpCode(ILOpCode.Stfld);
                                        ctorIl.Token(fHandle);
                                    }
                                }
                            }
                        }
                        catch { /* best-effort init; skip on failure */ }
                    }
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
                // Add Program type with a default static int Main() { return 0; }
                var firstMethodRowId = metadataBuilder.GetRowCount(TableIndex.MethodDef) + 1;
                var firstMethodHandle = MetadataTokens.MethodDefinitionHandle(firstMethodRowId);

                var programTypeHandle = metadataBuilder.AddTypeDefinition(
                    TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    default,
                    metadataBuilder.GetOrAddString("Program"),
                    objectTypeRef,
                    // Program has no fields; FieldList should point past the last existing field
                    MetadataTokens.FieldDefinitionHandle(metadataBuilder.GetRowCount(TableIndex.Field) + 1),
                    firstMethodHandle);

                // Build method body: ldc.i4.0; ret
                var ilInstructions = new BlobBuilder();
                var cf = new ControlFlowBuilder();
                var il = new InstructionEncoder(ilInstructions, cf);
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ret);
                var bodyOffset = methodBodyEncoder.AddMethodBody(
                    il,
                    maxStack: 8,
                    localVariablesSignature: default,
                    attributes: MethodBodyAttributes.None);

                // Method signature: static int32 Main()
                var methodSignatureBlob = new BlobBuilder();
                methodSignatureBlob.WriteByte(0x00); // DEFAULT calling convention
                methodSignatureBlob.WriteByte(0x00); // parameter count
                methodSignatureBlob.WriteByte((byte)SignatureTypeCode.Int32); // return type

                var methodHandle = metadataBuilder.AddMethodDefinition(
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                    MethodImplAttributes.IL,
                    metadataBuilder.GetOrAddString("Main"),
                    metadataBuilder.GetOrAddBlob(methodSignatureBlob),
                    bodyOffset,
                    MetadataTokens.ParameterHandle(metadataBuilder.GetRowCount(TableIndex.Param) + 1));

                entryPointMethodHandle = methodHandle;
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
                    var methodParamNames = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                    for (int i = 0; i < functionList.Count; i++)
                    {
                        var fn = functionList[i];
                        var methodDefHandle = MetadataTokens.MethodDefinitionHandle(firstMethodRowId + i);
                        methodMap[fn.Name] = methodDefHandle;
                        // capture parameter names for later emission-time arg insertion
                        var pnameList = fn.Signature?.ParameterSignatures?.Select(p => p.Name ?? "param").ToList() ?? new List<string>();
                        methodParamNames[fn.Name] = pnameList;
                    }

                    // Generate method bodies and capture their offsets (RVAs within method body stream)
                    var bodyOffsets = new Dictionary<string, int>(StringComparer.Ordinal);
                    foreach (var function in functionList)
                    {
                        var (il, localVariables, localTypes) = GenerateMethodIL(function, metadataBuilder, methodMap, methodParamNames);

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
                            metadataBuilder.GetOrAddString(function.Name),
                            metadataBuilder.GetOrAddBlob(methodSignatureBlob),
                            bodyOffsets[function.Name],
                            paramListStartMap[function.Name]);
                        // Do not set entry point here; we'll add a wrapper Main that always returns 0
                    }

                    // Add a wrapper entrypoint: static int32 Main() { call main(); if int/no-params return value else 0 }
                    // Only if a 'main' function exists
                    if (methodMap.TryGetValue("main", out var fifthMainHandle))
                    {
                        var fifthMainFunc = functionList.FirstOrDefault(f => string.Equals(f.Name, "main", StringComparison.Ordinal));
                        var mainReturnsInt32 = string.Equals(fifthMainFunc?.Signature?.ReturnTypeSignature?.Namespace, "System", StringComparison.Ordinal)
                            && string.Equals(fifthMainFunc?.Signature?.ReturnTypeSignature?.Name, "Int32", StringComparison.Ordinal);
                        var mainHasNoParams = (fifthMainFunc?.Signature?.ParameterSignatures?.Count ?? 0) == 0;
                        // Build body
                        var wrapperIl = new BlobBuilder();
                        var wrapperCf = new ControlFlowBuilder();
                        var wrapper = new InstructionEncoder(wrapperIl, wrapperCf);
                        // call main
                        wrapper.Call(fifthMainHandle);
                        if (mainReturnsInt32 && mainHasNoParams)
                        {
                            // Directly return the value produced by main
                            wrapper.OpCode(ILOpCode.Ret);
                        }
                        else
                        {
                            // Ensure stack is clean then return 0 as a safe default
                            wrapper.OpCode(ILOpCode.Pop);
                            wrapper.LoadConstantI4(0);
                            wrapper.OpCode(ILOpCode.Ret);
                        }

                        var wrapperBodyOffset = methodBodyEncoder.AddMethodBody(
                            wrapper,
                            maxStack: 8,
                            localVariablesSignature: default,
                            attributes: MethodBodyAttributes.None);

                        // Signature: static int32 Main()
                        var wrapperSig = new BlobBuilder();
                        wrapperSig.WriteByte(0x00); // DEFAULT
                        wrapperSig.WriteByte(0x00); // param count
                        wrapperSig.WriteByte((byte)SignatureTypeCode.Int32); // return type

                        var wrapperHandle = metadataBuilder.AddMethodDefinition(
                            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                            MethodImplAttributes.IL,
                            metadataBuilder.GetOrAddString("Main"),
                            metadataBuilder.GetOrAddBlob(wrapperSig),
                            wrapperBodyOffset,
                            MetadataTokens.ParameterHandle(metadataBuilder.GetRowCount(TableIndex.Param) + 1));

                        entryPointMethodHandle = wrapperHandle;
                    }
                    else
                    {
                        // No 'main' found; create a default Main that returns 0 to satisfy entrypoint
                        var ilInstructions = new BlobBuilder();
                        var cf = new ControlFlowBuilder();
                        var il = new InstructionEncoder(ilInstructions, cf);
                        il.LoadConstantI4(0);
                        il.OpCode(ILOpCode.Ret);
                        var bodyOffset = methodBodyEncoder.AddMethodBody(
                            il,
                            maxStack: 8,
                            localVariablesSignature: default,
                            attributes: MethodBodyAttributes.None);

                        var methodSignatureBlob = new BlobBuilder();
                        methodSignatureBlob.WriteByte(0x00); // DEFAULT calling convention
                        methodSignatureBlob.WriteByte(0x00); // parameter count
                        methodSignatureBlob.WriteByte((byte)SignatureTypeCode.Int32); // return type

                        var methodHandle = metadataBuilder.AddMethodDefinition(
                            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                            MethodImplAttributes.IL,
                            metadataBuilder.GetOrAddString("Main"),
                            metadataBuilder.GetOrAddBlob(methodSignatureBlob),
                            bodyOffset,
                            MetadataTokens.ParameterHandle(metadataBuilder.GetRowCount(TableIndex.Param) + 1));

                        entryPointMethodHandle = methodHandle;
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

    private EntityHandle CreateLocalVariableSignature(MetadataBuilder metadataBuilder, List<string> localVariables, Dictionary<string, SignatureTypeCode> localTypes)
    {
        var localsSignature = new BlobBuilder();
        localsSignature.WriteByte(0x07); // LOCAL_SIG
        localsSignature.WriteCompressedInteger(localVariables.Count); // Number of locals

        // Use inferred types where available (default Int32)
        foreach (var localVar in localVariables)
        {
            DebugLog($"DEBUG: Creating signature for local '{localVar}', classHandle={_localVarClassTypeHandles.ContainsKey(localVar)}, typeMap={(_localVarTypeMap.ContainsKey(localVar) ? _localVarTypeMap[localVar].ToString() : "not set")}");
            // Prefer explicit class type if known (from newobj)
            if (_localVarClassTypeHandles.TryGetValue(localVar, out var typeDefHandle))
            {
                // ELEMENT_TYPE_CLASS (0x12) then TypeDefOrRef coded index for the type
                DebugLog($"DEBUG: Using class type for '{localVar}'");
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
                DebugLog($"DEBUG: Using type code {sigType} for '{localVar}'");
                localsSignature.WriteByte((byte)sigType);
            }
        }

        return metadataBuilder.AddStandaloneSignature(metadataBuilder.GetOrAddBlob(localsSignature));
    }

    /// <summary>
    /// Emit instruction sequence using InstructionEncoder
    /// </summary>
    private void EmitInstructionSequence(InstructionEncoder il, il_ast.InstructionSequence sequence,
        MetadataBuilder metadataBuilder, Dictionary<string, MethodDefinitionHandle>? methodMap = null, List<string>? orderedLocals = null, Dictionary<string, int>? paramIndexMap = null, Dictionary<string, EntityHandle>? methodMemberRefMap = null, Dictionary<string, List<string>>? methodParamNames = null)
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
                EmitInstruction(il, instruction, metadataBuilder, localVarNames, methodMap, labelMap, paramIndexMap, methodMemberRefMap, methodParamNames);
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
        MetadataBuilder metadataBuilder, List<string>? localVarNames = null, Dictionary<string, MethodDefinitionHandle>? methodMap = null, Dictionary<string, LabelHandle>? labelMap = null, Dictionary<string, int>? paramIndexMap = null, Dictionary<string, EntityHandle>? methodMemberRefMap = null, Dictionary<string, List<string>>? methodParamNames = null)
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
                EmitCallInstruction(il, callInst, metadataBuilder, methodMap, paramIndexMap, methodParamNames);
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
    /// Emit arithmetic instruction
    /// </summary>
    /// <summary>
    /// Emit arithmetic instruction
    /// </summary>
    private void EmitArithmeticInstruction(InstructionEncoder il, il_ast.ArithmeticInstruction arithInst)
    {
        _arithmeticEmitter.Emit(il, arithInst);
    }

    private string ExtractMethodName(string methodSignature)
    {
        return SignatureUtilities.ExtractMethodName(methodSignature);
    }
    /// Extract type name from a constructor signature like "instance void TypeName::.ctor()"
    /// </summary>
    private string ExtractCtorTypeName(string ctorSignature)
    {
        return SignatureUtilities.ExtractCtorTypeName(ctorSignature);
    }

    /// <summary>
    /// Simulate the stack delta for an instruction sequence conservatively.
    /// Returns (netDelta, errorMessage). errorMessage is non-null when an unexpected pattern is found.
    /// </summary>
    private (int delta, string? error) SimulateInstructionSequence(il_ast.InstructionSequence seq)
    {
        int delta = 0;
        try
        {
            foreach (var ins in seq.Instructions)
            {
                switch (ins)
                {
                    case il_ast.LoadInstruction li:
                        var lop = (li.Opcode ?? string.Empty).ToLowerInvariant();
                        if (lop == "ldc.i4" || lop == "ldc.r4" || lop == "ldc.r8" || lop == "ldstr" || lop == "ldnull" || lop == "ldloc" || lop == "ldarg" || lop == "ldfld" || lop == "ldsfld" || lop == "newarr")
                        {
                            delta += 1;
                        }
                        else if (lop == "dup")
                        {
                            delta += 1; // dup increases stack by 1
                        }
                        break;
                    case il_ast.StoreInstruction si:
                        var sop = (si.Opcode ?? string.Empty).ToLowerInvariant();
                        if (sop == "stloc" || sop == "starg" || sop == "stsfld")
                        {
                            delta -= 1;
                        }
                        else if (sop == "stfld")
                        {
                            // stfld consumes obj and value
                            delta -= 2;
                        }
                        else if (sop == "stelem.i4")
                        {
                            // consumes array, index, value
                            delta -= 3;
                        }
                        break;
                    case il_ast.ArithmeticInstruction ai:
                        var aop = (ai.Opcode ?? string.Empty).ToLowerInvariant();
                        if (aop == "add" || aop == "sub" || aop == "mul" || aop == "div" || aop == "and" || aop == "or" || aop == "xor" || aop == "ceq" || aop == "clt" || aop == "cgt")
                        {
                            delta -= 1; // net -1 (2 consumed, 1 produced)
                        }
                        else if (aop == "not" || aop == "neg")
                        {
                            // unary ops: net 0
                        }
                        break;
                    case il_ast.CallInstruction ci:
                        var argCount = ci.ArgCount >= 0 ? ci.ArgCount : 0;
                        var ret = (ci.MethodSignature ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
                        var retLower = ret.ToLowerInvariant();
                        if (retLower == "void")
                        {
                            delta -= argCount;
                        }
                        else
                        {
                            delta -= argCount;
                            delta += 1;
                        }
                        break;
                    case il_ast.StackInstruction ss:
                        if (string.Equals(ss.Opcode, "pop", StringComparison.OrdinalIgnoreCase)) delta -= 1;
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            return (0, ex.Message);
        }
        return (delta, null);
    }

    /// <summary>
    /// Emit branch instruction
    /// </summary>
    private void EmitBranchInstruction(InstructionEncoder il, il_ast.BranchInstruction branchInst, Dictionary<string, LabelHandle>? labelMap)
    {
        _branchEmitter.Emit(il, branchInst, labelMap);
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
                "SByte" => SignatureTypeCode.SByte,
                "Byte" => SignatureTypeCode.Byte,
                "Int16" => SignatureTypeCode.Int16,
                "UInt16" => SignatureTypeCode.UInt16,
                "Int64" => SignatureTypeCode.Int64,
                "UInt32" => SignatureTypeCode.UInt32,
                "UInt64" => SignatureTypeCode.UInt64,
                "Char" => SignatureTypeCode.Char,
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