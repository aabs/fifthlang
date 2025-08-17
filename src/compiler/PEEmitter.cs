using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using il_ast;
using code_generator;

namespace compiler;

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
                metadataBuilder.GetOrAddString(ilAssembly.Name),
                metadataBuilder.GetOrAddGuid(Guid.NewGuid()),
                default,
                default);

            // Add assembly definition
            var assemblyHandle = metadataBuilder.AddAssembly(
                metadataBuilder.GetOrAddString(ilAssembly.Name),
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
                Console.WriteLine("ERROR: No methods found in IL metamodel, creating empty Program class");
                
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
                Console.WriteLine($"DEBUG: Found {ilAssembly.PrimeModule.Functions.Count} methods in IL metamodel");
                
                // Find the main method in the IL metamodel
                var mainMethod = ilAssembly.PrimeModule.Functions.FirstOrDefault(f => f.Header.IsEntrypoint);
                if (mainMethod == null)
                {
                    mainMethod = ilAssembly.PrimeModule.Functions.FirstOrDefault(f => f.Name == "main");
                }
                
                if (mainMethod != null)
                {
                    Console.WriteLine($"DEBUG: Found main method '{mainMethod.Name}', entry point: {mainMethod.Header.IsEntrypoint}");
                    Console.WriteLine($"DEBUG: Return type: {mainMethod.Signature.ReturnTypeSignature.Namespace}.{mainMethod.Signature.ReturnTypeSignature.Name}");
                    Console.WriteLine($"DEBUG: Body statements count: {mainMethod.Impl.Body.Statements.Count}");
                    
                    // Add Program type
                    var programTypeHandle = metadataBuilder.AddTypeDefinition(
                        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                        default,
                        metadataBuilder.GetOrAddString("Program"),
                        objectTypeRef,
                        MetadataTokens.FieldDefinitionHandle(1),
                        MetadataTokens.MethodDefinitionHandle(1));

                    // Generate method body from IL metamodel and add to method body stream
                    var mainMethodBody = GenerateMethodBody(mainMethod, metadataBuilder, writeLineMethodRef);
                    var methodBodyOffset = methodBodyStream.Count;
                    
                    // Add method body to the stream
                    methodBodyStream.WriteBytes(mainMethodBody.ToArray());
                    
                    // Create method signature from IL metamodel
                    var mainMethodSignatureBlob = new BlobBuilder();
                    mainMethodSignatureBlob.WriteByte(0x00); // calling convention
                    mainMethodSignatureBlob.WriteByte(0x00); // parameter count
                    
                    // Set return type based on IL metamodel
                    var returnTypeCode = GetSignatureTypeCode(mainMethod.Signature.ReturnTypeSignature);
                    mainMethodSignatureBlob.WriteByte((byte)returnTypeCode);
                    
                    Console.WriteLine($"DEBUG: Method signature return type code: {returnTypeCode}");
                    Console.WriteLine($"DEBUG: Method body size: {mainMethodBody.Count} bytes");
                    Console.WriteLine($"DEBUG: Method body offset: {methodBodyOffset}");
                    
                    var mainMethodHandle = metadataBuilder.AddMethodDefinition(
                        MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                        MethodImplAttributes.IL,
                        metadataBuilder.GetOrAddString("Main"),
                        metadataBuilder.GetOrAddBlob(mainMethodSignatureBlob),
                        methodBodyOffset, // RVA will be calculated by PE builder
                        default); // parameterList

                    Console.WriteLine($"DEBUG: Method definition added with body RVA");
                }
                else
                {
                    Console.WriteLine("ERROR: No main method found, creating empty Program class");
                    
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
            
            // Build PE with method bodies
            var peBuilder = new ManagedPEBuilder(
                new PEHeaderBuilder(imageCharacteristics: Characteristics.ExecutableImage),
                new MetadataRootBuilder(metadataBuilder),
                methodBodyStream);

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
    private BlobBuilder GenerateMethodBody(il_ast.MethodDefinition ilMethod, MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef)
    {
        var methodBody = new BlobBuilder();
        var il = new InstructionEncoder(methodBody);
        
        // Use AstToIlTransformationVisitor to get instruction sequences for each statement
        var transformer = new code_generator.AstToIlTransformationVisitor();
        
        // Generate instructions from the method's body statements
        if (ilMethod.Impl.Body.Statements.Any())
        {
            foreach (var statement in ilMethod.Impl.Body.Statements)
            {
                var instructionSequence = transformer.GenerateStatement(statement);
                EmitInstructionSequence(il, instructionSequence, metadataBuilder, writeLineMethodRef);
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
        
        // Always end with ret instruction
        il.OpCode(ILOpCode.Ret);
        
        return methodBody;
    }

    /// <summary>
    /// Emit instruction sequence using InstructionEncoder
    /// </summary>
    private void EmitInstructionSequence(InstructionEncoder il, il_ast.InstructionSequence sequence, 
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef)
    {
        foreach (var instruction in sequence.Instructions)
        {
            EmitInstruction(il, instruction, metadataBuilder, writeLineMethodRef);
        }
    }

    /// <summary>
    /// Emit a single instruction using InstructionEncoder
    /// </summary>
    private void EmitInstruction(InstructionEncoder il, il_ast.CilInstruction instruction, 
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef)
    {
        switch (instruction)
        {
            case il_ast.LoadInstruction loadInst:
                EmitLoadInstruction(il, loadInst, metadataBuilder);
                break;
                
            case il_ast.StoreInstruction storeInst:
                EmitStoreInstruction(il, storeInst);
                break;
                
            case il_ast.ArithmeticInstruction arithInst:
                EmitArithmeticInstruction(il, arithInst);
                break;
                
            case il_ast.CallInstruction callInst:
                EmitCallInstruction(il, callInst, metadataBuilder, writeLineMethodRef);
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
    private void EmitLoadInstruction(InstructionEncoder il, il_ast.LoadInstruction loadInst, MetadataBuilder metadataBuilder)
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
                // For simplicity, assume local 0 for now
                il.LoadLocal(0);
                break;
        }
    }

    /// <summary>
    /// Emit store instruction
    /// </summary>
    private void EmitStoreInstruction(InstructionEncoder il, il_ast.StoreInstruction storeInst)
    {
        switch (storeInst.Opcode.ToLowerInvariant())
        {
            case "stloc":
                // For simplicity, assume local 0 for now
                il.StoreLocal(0);
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
        MetadataBuilder metadataBuilder, EntityHandle writeLineMethodRef)
    {
        // For now, redirect print calls to Console.WriteLine
        if (callInst.MethodSignature?.Contains("Console") == true || 
            callInst.MethodSignature?.Contains("WriteLine") == true ||
            callInst.MethodSignature?.Contains("print") == true)
        {
            il.Call(writeLineMethodRef);
        }
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