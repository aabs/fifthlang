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
            
            // Collect method bodies for PE builder
            var methodBodyStream = new BlobBuilder();
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

            var writeLineSignatureBlob = new BlobBuilder();
            writeLineSignatureBlob.WriteByte(0x00); // calling convention
            writeLineSignatureBlob.WriteByte(0x01); // parameter count
            writeLineSignatureBlob.WriteByte((byte)SignatureTypeCode.Void); // return type
            writeLineSignatureBlob.WriteByte((byte)SignatureTypeCode.String); // parameter type
            
            var writeLineMethodRef = metadataBuilder.AddMemberReference(
                consoleTypeRef,
                metadataBuilder.GetOrAddString("WriteLine"),
                metadataBuilder.GetOrAddBlob(writeLineSignatureBlob));

            // Process methods from IL metamodel
            if (ilAssembly.PrimeModule == null || ilAssembly.PrimeModule.Functions.Count == 0)
            {
                // Add empty Program type as fallback
                var programTypeHandle = metadataBuilder.AddTypeDefinition(
                    TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    default,
                    metadataBuilder.GetOrAddString("Program"),
                    objectTypeRef,
                    MetadataTokens.FieldDefinitionHandle(1),
                    MetadataTokens.MethodDefinitionHandle(1));
            }
            else
            {
                // Process all functions in the module
                var functions = ilAssembly.PrimeModule.Functions.ToList();
                
                if (functions.Any())
                {
                    // Add Program type
                    var programTypeHandle = metadataBuilder.AddTypeDefinition(
                        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                        default,
                        metadataBuilder.GetOrAddString("Program"),
                        objectTypeRef,
                        MetadataTokens.FieldDefinitionHandle(1),
                        MetadataTokens.MethodDefinitionHandle(1));
                    
                    // Two-pass approach with pre-calculated RVAs
                    var methodMap = new Dictionary<string, MethodDefinitionHandle>();
                    var functionList = functions.ToList();
                    var methodBodies = new List<BlobBuilder>();
                    var methodRVAs = new List<int>();
                    
                    Console.WriteLine("DEBUG: Functions to process:");
                    foreach (var function in functionList)
                    {
                        Console.WriteLine($"  - {function.Name} (IsEntrypoint: {function.Header.IsEntrypoint})");
                    }
                    
                    // Pre-pass: Generate all method bodies to calculate RVAs
                    var currentOffset = 0;
                    foreach (var function in functionList)
                    {
                        // Generate method body without method map for now (calls will be skipped)
                        var methodBody = GenerateMethodBody(function, metadataBuilder, writeLineMethodRef, null);
                        methodBodies.Add(methodBody);
                        methodRVAs.Add(currentOffset);
                        currentOffset += methodBody.Count;
                        
                        Console.WriteLine($"DEBUG: Pre-calculated RVA for '{function.Name}': {methodRVAs.Last()}");
                    }
                    
                    // Pass 1: Create all method definitions with correct RVAs and populate method map
                    for (int i = 0; i < functionList.Count; i++)
                    {
                        var function = functionList[i];
                        
                        // Create method signature from IL metamodel
                        var methodSignatureBlob = new BlobBuilder();
                        methodSignatureBlob.WriteByte(0x00); // calling convention
                        methodSignatureBlob.WriteByte((byte)function.Signature.ParameterSignatures.Count); // parameter count
                        
                        // Set return type based on IL metamodel
                        var returnTypeCode = GetSignatureTypeCode(function.Signature.ReturnTypeSignature);
                        methodSignatureBlob.WriteByte((byte)returnTypeCode);
                        
                        // Add parameter types
                        foreach (var paramSig in function.Signature.ParameterSignatures)
                        {
                            var paramTypeCode = GetSignatureTypeCode(paramSig.TypeReference);
                            methodSignatureBlob.WriteByte((byte)paramTypeCode);
                        }
                        
                        var methodHandle = metadataBuilder.AddMethodDefinition(
                            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                            MethodImplAttributes.IL,
                            metadataBuilder.GetOrAddString(function.Header.IsEntrypoint ? "Main" : function.Name),
                            metadataBuilder.GetOrAddBlob(methodSignatureBlob),
                            methodRVAs[i], // Use pre-calculated RVA
                            default); // parameterList
                        
                        // Add to method map for method resolution
                        methodMap[function.Name] = methodHandle;
                        Console.WriteLine($"DEBUG: Added method '{function.Name}' to method map with RVA {methodRVAs[i]}");
                        
                        // Set entrypoint to the main method
                        if (function.Header.IsEntrypoint)
                        {
                            entryPointMethodHandle = methodHandle;
                        }
                    }
                    
                    Console.WriteLine("DEBUG: Complete method map before regenerating bodies:");
                    foreach (var kvp in methodMap)
                    {
                        Console.WriteLine($"  - {kvp.Key}");
                    }
                    
                    // Pass 2: Regenerate method bodies with complete method map for proper call resolution
                    for (int i = 0; i < functionList.Count; i++)
                    {
                        var function = functionList[i];
                        
                        Console.WriteLine($"DEBUG: Regenerating body for method '{function.Name}' with complete method map");
                        
                        // Generate method body with complete method map for proper call resolution
                        var methodBody = GenerateMethodBody(function, metadataBuilder, writeLineMethodRef, methodMap);
                        
                        // Add method body to the stream at the expected offset
                        methodBodyStream.WriteBytes(methodBody.ToArray());
                        
                        Console.WriteLine($"DEBUG: Added final method body for '{function.Name}' to stream");
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
                        MetadataTokens.FieldDefinitionHandle(1),
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
    private BlobBuilder GenerateMethodBody(il_ast.MethodDefinition ilMethod, MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, Dictionary<string, MethodDefinitionHandle>? methodMap = null)
    {
        var ilInstructions = new BlobBuilder();
        var il = new InstructionEncoder(ilInstructions);
        
        // Use AstToIlTransformationVisitor to get instruction sequences for each statement
        var transformer = new AstToIlTransformationVisitor();
        
        // Track if we've emitted a return instruction and collect local variables
        bool hasReturnInstruction = false;
        var localVariables = new HashSet<string>();
        
        // Generate instructions from the method's body statements
        if (ilMethod.Impl.Body.Statements.Any())
        {
            foreach (var statement in ilMethod.Impl.Body.Statements)
            {
                var instructionSequence = transformer.GenerateStatement(statement);
                
                // Collect local variable information and check for return instructions
                foreach (var instruction in instructionSequence.Instructions)
                {
                    if (instruction is il_ast.ReturnInstruction)
                    {
                        hasReturnInstruction = true;
                    }
                    else if (instruction is il_ast.LoadInstruction loadInst && 
                            loadInst.Opcode.ToLowerInvariant() == "ldloc" && 
                            loadInst.Value is string loadVar)
                    {
                        localVariables.Add(loadVar);
                    }
                    else if (instruction is il_ast.StoreInstruction storeInst && 
                            storeInst.Opcode.ToLowerInvariant() == "stloc" && 
                            storeInst.Target is string storeVar)
                    {
                        localVariables.Add(storeVar);
                    }
                }
                
                EmitInstructionSequence(il, instructionSequence, metadataBuilder, writeLineMethodRef, methodMap);
            }
        }
        else
        {
            // If no statements, add a simple return for the method to be valid
            // For int return type, load a constant first
            if (ilMethod.Signature.ReturnTypeSignature.Name == "Int32")
            {
                il.LoadConstantI4(42); // Default return value
            }
        }
        
        // Only add ret instruction if one wasn't already emitted
        if (!hasReturnInstruction)
        {
            il.OpCode(ILOpCode.Ret);
        }
        
        // Create local variable signature if needed
        EntityHandle localVarSigToken = default;
        if (localVariables.Any())
        {
            localVarSigToken = CreateLocalVariableSignature(metadataBuilder, localVariables);
        }
        
        // Create the method body blob with proper header format
        var methodBody = new BlobBuilder();
        
        // Write method body header (simplified approach - always use tiny format if possible)
        var codeSize = ilInstructions.Count;
        var hasLocals = localVariables.Any();
        
        if (codeSize < 64 && !hasLocals)
        {
            // Tiny format: first byte contains 2-bit format + 6-bit size
            methodBody.WriteByte((byte)(0x02 | (codeSize << 2))); // Format: CorILMethod_TinyFormat
        }
        else
        {
            // Fat format header (12 bytes) - more carefully constructed
            ushort flags = 0x3003; // CorILMethod_FatFormat | CorILMethod_InitLocals | CorILMethod_MoreSects (if needed)
            if (!hasLocals) flags = 0x3002; // No init locals if no locals
            
            methodBody.WriteUInt16(flags);        // Flags and format (16-bit)
            methodBody.WriteUInt16(8);            // Max stack (16-bit) - conservative estimate
            methodBody.WriteUInt32((uint)codeSize); // Code size (32-bit)
            methodBody.WriteUInt32(hasLocals ? (uint)MetadataTokens.GetToken(localVarSigToken) : 0);  // Local var sig token
        }
        
        // Write the actual IL instructions
        methodBody.WriteBytes(ilInstructions.ToArray());
        
        return methodBody;
    }

    /// <summary>
    /// Create a local variable signature for the method
    /// </summary>
    private EntityHandle CreateLocalVariableSignature(MetadataBuilder metadataBuilder, HashSet<string> localVariables)
    {
        var localsSignature = new BlobBuilder();
        localsSignature.WriteByte(0x07); // LOCAL_SIG
        localsSignature.WriteCompressedInteger(localVariables.Count); // Number of locals
        
        // For simplicity, all locals are Int32 for now
        foreach (var localVar in localVariables)
        {
            localsSignature.WriteByte((byte)SignatureTypeCode.Int32);
        }
        
        return metadataBuilder.AddStandaloneSignature(metadataBuilder.GetOrAddBlob(localsSignature));
    }

    /// <summary>
    /// Emit instruction sequence using InstructionEncoder
    /// </summary>
    private void EmitInstructionSequence(InstructionEncoder il, il_ast.InstructionSequence sequence, 
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, Dictionary<string, MethodDefinitionHandle>? methodMap = null)
    {
        // Build a local variable name to index mapping
        var localVarNames = new List<string>();
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
        
        foreach (var instruction in sequence.Instructions)
        {
            EmitInstruction(il, instruction, metadataBuilder, writeLineMethodRef, localVarNames, methodMap);
        }
    }

    /// <summary>
    /// Emit a single instruction using InstructionEncoder
    /// </summary>
    private void EmitInstruction(InstructionEncoder il, il_ast.CilInstruction instruction, 
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, List<string>? localVarNames = null, Dictionary<string, MethodDefinitionHandle>? methodMap = null)
    {
        switch (instruction)
        {
            case il_ast.LoadInstruction loadInst:
                EmitLoadInstruction(il, loadInst, metadataBuilder, localVarNames);
                break;
                
            case il_ast.StoreInstruction storeInst:
                EmitStoreInstruction(il, storeInst, localVarNames);
                break;
                
            case il_ast.ArithmeticInstruction arithInst:
                EmitArithmeticInstruction(il, arithInst);
                break;
                
            case il_ast.CallInstruction callInst:
                EmitCallInstruction(il, callInst, metadataBuilder, writeLineMethodRef, methodMap);
                break;
                
            case il_ast.BranchInstruction branchInst:
                EmitBranchInstruction(il, branchInst);
                break;
                
            case il_ast.ReturnInstruction:
                il.OpCode(ILOpCode.Ret);
                break;
                
            case il_ast.LabelInstruction labelInst:
                // Labels are handled by the branching infrastructure
                // For now, skip explicit label emission
                break;
        }
    }

    /// <summary>
    /// Emit load instruction
    /// </summary>
    private void EmitLoadInstruction(InstructionEncoder il, il_ast.LoadInstruction loadInst, MetadataBuilder metadataBuilder, List<string> localVarNames = null)
    {
        switch (loadInst.Opcode.ToLowerInvariant())
        {
            case "ldc.i4":
                if (loadInst.Value is int intValue)
                {
                    il.LoadConstantI4(intValue);
                }
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
                    }
                    else
                    {
                        // Fallback to index 0 if variable not found
                        il.LoadLocal(0);
                    }
                }
                else
                {
                    // Fallback for non-string value or no local var names
                    il.LoadLocal(0);
                }
                break;
        }
    }

    /// <summary>
    /// Emit store instruction
    /// </summary>
    private void EmitStoreInstruction(InstructionEncoder il, il_ast.StoreInstruction storeInst, List<string> localVarNames = null)
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
                    }
                    else
                    {
                        // Fallback to index 0 if variable not found
                        il.StoreLocal(0);
                    }
                }
                else
                {
                    // Fallback for non-string value or no local var names
                    il.StoreLocal(0);
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
        }
    }

    /// <summary>
    /// Emit call instruction
    /// </summary>
    private void EmitCallInstruction(InstructionEncoder il, il_ast.CallInstruction callInst, 
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef, Dictionary<string, MethodDefinitionHandle>? methodMap = null)
    {
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
        
        Console.WriteLine($"DEBUG: Trying to resolve method call: '{callInst.MethodSignature}' -> '{methodName}'");
        
        // Try to resolve internal method calls using the method map
        if (methodMap != null && methodMap.TryGetValue(methodName, out var methodHandle))
        {
            Console.WriteLine($"DEBUG: Found method '{methodName}' in method map");
            il.Call(methodHandle);
            return;
        }
        
        // Also try to handle subclause method calls by checking for the pattern
        // This handles cases where the IL calls subclause methods directly
        if (methodName.Contains("_subclause") && methodMap != null)
        {
            if (methodMap.TryGetValue(methodName, out var subclaueueHandle))
            {
                Console.WriteLine($"DEBUG: Found subclause method '{methodName}' in method map");
                il.Call(subclaueueHandle);
                return;
            }
        }
        
        // For unresolved method calls, try to find any method that starts with the same base name
        if (methodMap != null)
        {
            Console.WriteLine($"DEBUG: Method map contents:");
            foreach (var kvp in methodMap)
            {
                Console.WriteLine($"  - {kvp.Key}");
            }
            
            var baseName = methodName.Split('_')[0]; // Get base name before any suffix
            var matchingMethod = methodMap.FirstOrDefault(kvp => kvp.Key.StartsWith(baseName));
            if (!matchingMethod.Equals(default(KeyValuePair<string, MethodDefinitionHandle>)))
            {
                Console.WriteLine($"DEBUG: Found matching method by base name '{baseName}': '{matchingMethod.Key}'");
                il.Call(matchingMethod.Value);
                return;
            }
        }
        
        // If still unresolved, emit a warning (this should be rare now)
        Console.WriteLine($"WARNING: Skipping unresolved method call: {callInst.MethodSignature}");
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
    /// Emit branch instruction
    /// </summary>
    private void EmitBranchInstruction(InstructionEncoder il, il_ast.BranchInstruction branchInst)
    {
        // Branch instruction handling is complex and requires label management
        // For initial implementation, skip complex branching
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
}