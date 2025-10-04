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
public class PEEmitter
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

    /// <summary>
    /// Generate method body from IL metamodel method definition
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
        else
        {
            // If no statements, add a simple return for the method to be valid
            // For int return type, load a constant first
            var returnType = ilMethod?.Signature?.ReturnTypeSignature?.Name ?? string.Empty;
            if (returnType == "Int32")
            {
                il.LoadConstantI4(0); // Default return value
            }
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
        // already produce the required stack behaviour; only enforce default-return
        // stack adjustments when there is no explicit return.
        if (!hasExplicitReturn && cumulativeStack != expectedReturnStack)
        {
            // Attempt to repair by injecting conservative default return producers so the method
            // ends with the expected stack state. This is a temporary safety net while the
            // lowering stage is hardened to always produce the necessary producers.
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
                    // Ensure we don't double-quote: Trim existing quotes, then use as-is
                    var clean = stringValue;
                    if (clean.Length >= 2 && clean.StartsWith("\"") && clean.EndsWith("\""))
                    {
                        clean = clean.Substring(1, clean.Length - 2);
                    }
                    il.LoadString(metadataBuilder.GetOrAddUserString(clean));
                }
                break;
            case "ldnull":
                // Push a null reference onto the evaluation stack
                il.OpCode(ILOpCode.Ldnull);
                _lastLoadedLocal = null;
                _pendingStackTopClassType = null;
                _lastLoadedParam = null;
                break;

            case "ldloc":
                if (loadInst.Value is string varName && localVarNames != null)
                {
                    var index = localVarNames.IndexOf(varName);
                    if (index >= 0)
                    {
                        il.LoadLocal(index);
                        _lastLoadedLocal = varName;
                        // If this local has a known class type, propagate to pending stack top
                        if (_localVarClassTypeHandles.TryGetValue(varName, out var tdh))
                        {
                            _pendingStackTopClassType = tdh;
                        }
                        else
                        {
                            _pendingStackTopClassType = null;
                        }
                        _lastLoadedParam = null;
                    }
                    else
                    {
                        DebugLog($"DEBUG: ldloc unknown local '{varName}'. available locals: {string.Join(',', localVarNames)}");
                        // Unknown local; push default int to keep stack balanced
                        il.LoadConstantI4(0);
                        _lastLoadedLocal = null;
                        _pendingStackTopClassType = null;
                        _lastLoadedParam = null;
                    }
                }
                else
                {
                    // Fallback for non-string value or no local var names
                    il.LoadConstantI4(0);
                    _lastLoadedLocal = null;
                    _pendingStackTopClassType = null;
                    _lastLoadedParam = null;
                }
                break;
            case "ldarg":
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
                    // Unknown arg; push default int
                    il.LoadConstantI4(0);
                    _lastLoadedLocal = null;
                    _lastLoadedParam = null;
                }
                break;

            case "ldfld":
                if (loadInst.Value is string fldName)
                {
                    // Resolve FieldDefinitionHandle for the field. Prefer exact owner type if known.
                    EntityHandle? fieldToken = null;
                    // Try using pending stack top class type first (e.g., from ldarg/ldloc/newobj)
                    if (_pendingStackTopClassType.HasValue && _typeNamesByHandle.TryGetValue(_pendingStackTopClassType.Value, out var pTypeName))
                    {
                        var keyDef = $"{pTypeName}::{fldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef1))
                        {
                            fieldToken = fdef1;
                            DebugLog($"DEBUG: ldfld owner via pendingStack '{pTypeName}', field '{fldName}' -> def handle {MetadataTokens.GetRowNumber(fdef1)}");
                        }
                    }
                    // Then try last-loaded local's class type
                    if (fieldToken == null && !string.IsNullOrEmpty(_lastLoadedLocal) && _localVarClassTypeHandles.TryGetValue(_lastLoadedLocal, out var typeHandle) && _typeNamesByHandle.TryGetValue(typeHandle, out var typeName))
                    {
                        var keyDef = $"{typeName}::{fldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef))
                        {
                            fieldToken = fdef;
                            DebugLog($"DEBUG: ldfld owner via lastLoadedLocal '{typeName}', field '{fldName}' -> def handle {MetadataTokens.GetRowNumber(fdef)}");
                        }
                    }
                    // Then try last-loaded parameter's class type
                    if (fieldToken == null && !string.IsNullOrEmpty(_lastLoadedParam) && _paramClassTypeHandles.TryGetValue(_lastLoadedParam, out var pTypeHandle) && _typeNamesByHandle.TryGetValue(pTypeHandle, out var pName))
                    {
                        var keyDef = $"{pName}::{fldName}";
                        if (_fieldHandlesByTypeAndName.TryGetValue(keyDef, out var fdef2))
                        {
                            fieldToken = fdef2;
                            DebugLog($"DEBUG: ldfld owner via lastLoadedParam '{pName}', field '{fldName}' -> def handle {MetadataTokens.GetRowNumber(fdef2)}");
                        }
                    }

                    // Fallback by simple name
                    if (fieldToken == null && _fieldHandles.TryGetValue(fldName, out var fldHandle))
                    {
                        fieldToken = fldHandle;
                        DebugLog($"DEBUG: ldfld falling back by simple name '{fldName}' -> def handle {MetadataTokens.GetRowNumber(fldHandle)}");
                    }

                    if (fieldToken != null)
                    {
                        il.OpCode(ILOpCode.Ldfld);
                        il.Token(fieldToken.Value);
                        // If we know the declared field type and it's a user class, remember it for subsequent stloc typing
                        _pendingStackTopClassType = null;
                        if (fieldToken.HasValue)
                        {
                            var eh = fieldToken.Value;
                            if (eh.Kind == HandleKind.FieldDefinition)
                            {
                                var fdh = (FieldDefinitionHandle)eh;
                                if (_fieldDeclaredTypes.TryGetValue(fdh, out var declaredType))
                                {
                                    if (!string.Equals(declaredType.Namespace, "System", StringComparison.Ordinal) && !string.IsNullOrEmpty(declaredType.Name))
                                    {
                                        if (_typeHandles.TryGetValue(declaredType.Name, out var declTypeHandle))
                                        {
                                            _pendingStackTopClassType = declTypeHandle;
                                        }
                                    }
                                }
                            }
                        }
                        // Consuming local as object; reset last-loaded-local tracker
                        _lastLoadedLocal = null;
                        _lastLoadedParam = null;
                    }
                    else
                    {
                        // Unknown field: keep stack balanced but degrade gracefully
                        il.OpCode(ILOpCode.Pop);
                        il.LoadConstantI4(0);
                        _lastLoadedLocal = null;
                        _lastLoadedParam = null;
                        _pendingStackTopClassType = null;
                        // Emit debug context to help triage stfld owner resolution failures
                        try
                        {
                            var candidates = new List<string>();
                            if (!string.IsNullOrEmpty(_pendingNewobjTypeName)) candidates.Add($"pendingNewobj={_pendingNewobjTypeName}");
                            if (!string.IsNullOrEmpty(_lastLoadedLocal)) candidates.Add($"lastLoadedLocal={_lastLoadedLocal}");
                            if (!string.IsNullOrEmpty(_lastLoadedParam)) candidates.Add($"lastLoadedParam={_lastLoadedParam}");
                            // Include a few known matching keys for quick inspection
                            var sampleKeys = _fieldHandlesByTypeAndName.Keys.Take(10).ToList();
                            DebugLog($"DEBUG: stfld: could not resolve field. Candidates: {string.Join(',', candidates)}. Sample field keys: {string.Join(';', sampleKeys)}");
                        }
                        catch { }
                    }
                }
                break;

            case "ldsfld":
                if (loadInst.Value is string staticFieldToken)
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
                    if (fieldToken == null && _fieldHandles.TryGetValue(staticFieldName, out var fldHandle))
                    {
                        fieldToken = fldHandle;
                    }

                    if (fieldToken != null)
                    {
                        il.OpCode(ILOpCode.Ldsfld);
                        il.Token(fieldToken.Value);
                        // Static field yields a reference or primitive on stack; clear local trackers
                        _lastLoadedLocal = null;
                        _lastLoadedParam = null;
                        _pendingStackTopClassType = null;
                    }
                    else
                    {
                        // Unknown static field: push default null to keep stack balanced
                        il.OpCode(ILOpCode.Ldnull);
                        _lastLoadedLocal = null;
                        _lastLoadedParam = null;
                        _pendingStackTopClassType = null;
                    }
                }
                break;

            case "dup":
                // Duplicate the top value on the stack
                il.OpCode(ILOpCode.Dup);
                break;
            case "box":
                if (loadInst.Value is string boxToken)
                {
                    DebugLog($"DEBUG: EmitLoadInstruction - box token='{boxToken}'");
                    var bt = boxToken.Trim();
                    // Recognize IL-friendly primitive alias or System.* form
                    if (string.Equals(bt, "int32", StringComparison.OrdinalIgnoreCase) || string.Equals(bt, "System.Int32", StringComparison.Ordinal))
                    {
                        DebugLog("TRACE: Emitting ILOpCode.Box for int32");
                        il.OpCode(ILOpCode.Box);
                        DebugLog("TRACE: Emitted ILOpCode.Box, now emitting token");
                        il.Token(_systemInt32TypeRef);
                        DebugLog("TRACE: Emitted token for box int32");
                        string asm = "Fifth.System";
                        string fullName = bt;
                        var atIdx = bt.LastIndexOf('@');
                        if (atIdx > 0)
                        {
                            fullName = bt.Substring(0, atIdx);
                            asm = bt.Substring(atIdx + 1);
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
                        // If the top of the stack is a known class type (from ldfld), propagate that to the target local
                        if (_pendingStackTopClassType.HasValue)
                        {
                            _localVarTypeMap[varName] = SignatureTypeCode.Object;
                            _localVarClassTypeHandles[varName] = _pendingStackTopClassType.Value;
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

                var asmNameResolved = string.IsNullOrWhiteSpace(asmName) ? "Fifth.System" : asmName;
                var asmKey = asmNameResolved.ToLowerInvariant();
                AssemblyReferenceHandle asmRef;
                if (!_assemblyRefHandles.TryGetValue(asmKey, out asmRef))
                {
                    asmRef = metadataBuilder.AddAssemblyReference(
                        metadataBuilder.GetOrAddString(asmNameResolved!),
                        ResolveAssemblyVersion(asmNameResolved), default, default, default, default);
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
                            ResolveAssemblyVersion(asm), default, default, default, default);
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
                var methodSig = new BlobBuilder();
                methodSig.WriteByte(0x00); // DEFAULT

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
                if (resolved != null)
                {
                    try
                    {
                        var ps = resolved.GetParameters();
                        foreach (var p in ps) finalParamTokens.Add(TypeToToken(p.ParameterType));
                        finalReturnToken = TypeToToken(resolved.ReturnType);
                        DebugLog($"DEBUG: Resolved runtime method: {resolved.DeclaringType?.FullName}.{resolved.Name} params={ps.Length} return={resolved.ReturnType.FullName}");
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