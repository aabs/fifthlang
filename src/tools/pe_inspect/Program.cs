using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata.Ecma335;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Usage: pe_inspect <assembly-path>");
    return 1;
}

var path = args[0];
using var stream = File.OpenRead(path);
using var peReader = new PEReader(stream);
var mdReader = peReader.GetMetadataReader();

Console.WriteLine($"MemberRef count: {mdReader.MemberReferences.Count}");
int i = 1;
foreach (var handle in mdReader.MemberReferences)
{
    var mr = mdReader.GetMemberReference(handle);
    var parent = mr.Parent;
    string parentStr;
    int parentRow = -1;
    if (parent.Kind == HandleKind.TypeReference)
    {
        var tr = mdReader.GetTypeReference((TypeReferenceHandle)parent);
        parentStr = mdReader.GetString(tr.Name);
        parentRow = MetadataTokens.GetRowNumber((TypeReferenceHandle)parent);
    }
    else if (parent.Kind == HandleKind.TypeDefinition)
    {
        parentStr = mdReader.GetString(mdReader.GetTypeDefinition((TypeDefinitionHandle)parent).Name);
    }
    else
    {
        parentStr = parent.Kind.ToString();
    }
    var name = mdReader.GetString(mr.Name);
    var sigBlob = mdReader.GetBlobReader(mr.Signature);
    var bytes = sigBlob.ReadBytes(sigBlob.Length).ToArray();
    if (parentRow > 0)
        Console.WriteLine($"MemberRef row={i} parent={parentStr} (TypeRef row={parentRow}) name={name} sigBytes={BitConverter.ToString(bytes)}");
    else
        Console.WriteLine($"MemberRef row={i} parent={parentStr} name={name} sigBytes={BitConverter.ToString(bytes)}");
    // Decode signature briefly to locate any class type coded indexes
    try
    {
        var br = mdReader.GetBlobReader(mr.Signature);
        // calling convention
        var cc = br.ReadByte();
        var pcount = br.ReadByte();
        // return type - skip
        _ = br.ReadByte();
        for (int pi = 0; pi < pcount; pi++)
        {
            var t = br.ReadByte();
            if (t == 0x12) // ELEMENT_TYPE_CLASS
            {
                var coded = br.ReadCompressedInteger();
                var row = coded >> 2;
                var tag = coded & 0x3;
                Console.WriteLine($"  -> param {pi}: ELEMENT_TYPE_CLASS coded={coded} row={row} tag={tag}");
            }
            else
            {
                // skip other simple types
            }
        }
    }
    catch { }
    i++;
}

Console.WriteLine("\nTypeRef table:");
var trIdx = 1;
foreach (var trh in mdReader.TypeReferences)
{
    var tr = mdReader.GetTypeReference(trh);
    var ns = mdReader.GetString(tr.Namespace);
    var nm = mdReader.GetString(tr.Name);
    var rs = tr.ResolutionScope.Kind switch
    {
        HandleKind.AssemblyReference => mdReader.GetString(mdReader.GetAssemblyReference((AssemblyReferenceHandle)tr.ResolutionScope).Name),
        HandleKind.ModuleReference => mdReader.GetString(mdReader.GetModuleReference((ModuleReferenceHandle)tr.ResolutionScope).Name),
        _ => tr.ResolutionScope.Kind.ToString()
    };
    Console.WriteLine($"TypeRef row={trIdx} name={ns}.{nm} scope={rs}");
    trIdx++;
}

Console.WriteLine("\nAssemblyRef table:");
var arIdx = 1;
foreach (var arh in mdReader.AssemblyReferences)
{
    var ar = mdReader.GetAssemblyReference(arh);
    var name = mdReader.GetString(ar.Name);
    var ver = ar.Version;
    Console.WriteLine($"AssemblyRef row={arIdx} name={name} version={ver}");
    arIdx++;
}

// Inspect all method bodies for call tokens
Console.WriteLine("\nMethod bodies (call tokens):");
foreach (var methodHandle in mdReader.MethodDefinitions)
{
    var method = mdReader.GetMethodDefinition(methodHandle);
    var name = mdReader.GetString(method.Name);
    var body = peReader.GetMethodBody(method.RelativeVirtualAddress);
    if (body == null) continue;
    var ilBytes = body.GetILBytes() ?? Array.Empty<byte>();
    Console.WriteLine($"Method: {name} IL size={ilBytes.Length}");
    // Print raw IL bytes (hex) for quick inspection
    Console.WriteLine("  IL bytes: " + BitConverter.ToString(ilBytes));
    for (int idx = 0; idx < ilBytes.Length; idx++)
    {
        var b = ilBytes[idx];
        if (b == 0x28) // call
        {
            if (idx + 4 < ilBytes.Length)
            {
                var token = BitConverter.ToUInt32(ilBytes, idx + 1);
                Console.WriteLine($"  IL_{idx:X4}: call token=0x{token:X8}");
            }
        }
    }
}

return 0;