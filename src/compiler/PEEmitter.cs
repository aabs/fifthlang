using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using il_ast;

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
            // Create a simple Hello World executable for now
            // This is a minimal implementation to get the basic infrastructure working
            var metadataBuilder = new MetadataBuilder();
            var blobBuilder = new BlobBuilder();
            
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

            // Add Program type
            var programTypeHandle = metadataBuilder.AddTypeDefinition(
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                default,
                metadataBuilder.GetOrAddString("Program"),
                objectTypeRef,
                MetadataTokens.FieldDefinitionHandle(1),
                MetadataTokens.MethodDefinitionHandle(1));

            // Add Main method
            var mainMethodBody = new BlobBuilder();
            var il = new InstructionEncoder(mainMethodBody);
            
            // ldstr "Hello from Fifth!"
            il.LoadString(metadataBuilder.GetOrAddUserString("Hello from Fifth!"));
            // call void [System.Console]System.Console::WriteLine(string)
            il.Call(writeLineMethodRef);
            // ret
            il.OpCode(ILOpCode.Ret);

            // Add Main method
            var mainMethodSignatureBlob = new BlobBuilder();
            mainMethodSignatureBlob.WriteByte(0x00); // calling convention
            mainMethodSignatureBlob.WriteByte(0x00); // parameter count
            mainMethodSignatureBlob.WriteByte((byte)SignatureTypeCode.Void); // return type
            
            var mainMethodHandle = metadataBuilder.AddMethodDefinition(
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                MethodImplAttributes.IL,
                metadataBuilder.GetOrAddString("Main"),
                metadataBuilder.GetOrAddBlob(mainMethodSignatureBlob),
                -1,
                default); // parameterList

            // Add the method body using a different approach
            var methodBodyBlob = metadataBuilder.GetOrAddBlob(mainMethodBody.ToArray());
            
            // Build PE
            var peBuilder = new ManagedPEBuilder(
                new PEHeaderBuilder(imageCharacteristics: Characteristics.ExecutableImage),
                new MetadataRootBuilder(metadataBuilder),
                new BlobBuilder());

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
}