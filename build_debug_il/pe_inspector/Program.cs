using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Loader;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: pe_inspector <generated-exe-path> <fifth.system.dll-path>");
            return 2;
        }

        var exePath = args[0];
        var fifthPath = args[1];

        if (!File.Exists(exePath))
        {
            Console.Error.WriteLine($"Generated exe not found: {exePath}");
            return 2;
        }
        if (!File.Exists(fifthPath))
        {
            Console.Error.WriteLine($"Fifth.System.dll not found: {fifthPath}");
            return 2;
        }

        Console.WriteLine($"Inspecting generated exe: {exePath}");
        Console.WriteLine($"Inspecting Fifth.System: {fifthPath}");
        Console.WriteLine();

        using (var fs = File.OpenRead(exePath))
        using (var peReader = new PEReader(fs))
        {
            if (!peReader.HasMetadata)
            {
                Console.WriteLine("PE has no metadata");
                return 1;
            }
            var reader = peReader.GetMetadataReader();

            Console.WriteLine("TypeRefs:");
            foreach (var tr in reader.TypeReferences)
            {
                var tref = reader.GetTypeReference(tr);
                var ns = reader.GetString(tref.Namespace);
                var name = reader.GetString(tref.Name);
                var scope = tref.ResolutionScope;
                string scopeStr = scope.Kind.ToString();
                if (scope.Kind == HandleKind.AssemblyReference)
                {
                    var aref = reader.GetAssemblyReference((AssemblyReferenceHandle)scope);
                    scopeStr = reader.GetString(aref.Name);
                }
                Console.WriteLine($"  TypeRef: {ns}.{name}  (scope: {scopeStr})");
            }

            Console.WriteLine();
            Console.WriteLine("MemberRefs (subset related to Fifth.System):");

            foreach (var mrh in reader.MemberReferences)
            {
                var mr = reader.GetMemberReference(mrh);
                var name = reader.GetString(mr.Name);
                string parentStr = mr.Parent.Kind.ToString();
                if (mr.Parent.Kind == HandleKind.TypeReference)
                {
                    var parentTypeRef = reader.GetTypeReference((TypeReferenceHandle)mr.Parent);
                    parentStr = reader.GetString(parentTypeRef.Namespace) + "." + reader.GetString(parentTypeRef.Name);
                }
                else if (mr.Parent.Kind == HandleKind.TypeDefinition)
                {
                    var td = reader.GetTypeDefinition((TypeDefinitionHandle)mr.Parent);
                    parentStr = reader.GetString(td.Namespace) + "." + reader.GetString(td.Name);
                }
                else if (mr.Parent.Kind == HandleKind.MethodDefinition)
                {
                    parentStr = "MethodDef:" + ((MethodDefinitionHandle)mr.Parent).ToString();
                }

                // Only print Fifth.System-related MemberRefs (common case)
                if (!parentStr.Contains("Fifth.System") && !name.Contains("Create") && !name.Contains("Count") && !name.Contains("Assert"))
                    continue;

                var sigBlob = mr.Signature;
                var blobBytes = reader.GetBlobBytes(sigBlob);
                var hex = BitConverter.ToString(blobBytes.Take(12).ToArray()).Replace('-', ' ');
                Console.WriteLine($"  MemberRef: {parentStr}::{name}  blobLen={blobBytes.Length}  blobPrefix={hex}");
            }

            Console.WriteLine();
            Console.WriteLine("MethodDefs in generated assembly (top-level methods):");
            foreach (var mh in reader.MethodDefinitions)
            {
                var md = reader.GetMethodDefinition(mh);
                var name = reader.GetString(md.Name);
                if (name == "main" || name == "Main")
                {
                    var sigBlob = md.Signature;
                    var blobBytes = reader.GetBlobBytes(sigBlob);
                    var hex = BitConverter.ToString(blobBytes.Take(12).ToArray()).Replace('-', ' ');
                    Console.WriteLine($"  MethodDef: {name}  blobLen={blobBytes.Length}  blobPrefix={hex}");
                }
            }
        }

        // Load metadata for all referenced assemblies found next to the generated exe (build_debug_il), index methods per type
        Console.WriteLine();
        Console.WriteLine("Loading referenced assemblies from build_debug_il to resolve external method signatures:");
        var exeDir = Path.GetDirectoryName(exePath) ?? Directory.GetCurrentDirectory();
        var assemblyMetadata = new Dictionary<string, MetadataReader>(StringComparer.OrdinalIgnoreCase);
        var assemblyTypeMethodIndex = new Dictionary<(string assemblyName, string typeFullName), Dictionary<string, (string returnType, List<string> paramTypes, byte[] raw)>>();

        // Helper to load an assembly metadata reader by assembly simple name if the file exists in exeDir
        void TryLoadAssemblyMetadata(string asmName)
        {
            if (string.IsNullOrEmpty(asmName)) return;
            if (assemblyMetadata.ContainsKey(asmName)) return;
            var probe = Path.Combine(exeDir, asmName + ".dll");
            if (!File.Exists(probe)) return;
            try
            {
                using var fsx = File.OpenRead(probe);
                var pe = new PEReader(fsx);
                if (!pe.HasMetadata) return;
                var rdr = pe.GetMetadataReader();
                // store reader; we must copy metadata for later use (can't keep PEReader open after disposing fsx)
                // Instead, re-open when needed; but for simplicity, store path and reopen later
                assemblyMetadata[asmName] = rdr;
            }
            catch { /* ignore load failures */ }
        }

        // Scan TypeRefs and AssemblyRefs in exe to find assemblies to probe
        using (var fsr = File.OpenRead(exePath))
        using (var per = new PEReader(fsr))
        {
            var rdr = per.GetMetadataReader();
            foreach (var aref in rdr.AssemblyReferences)
            {
                var aname = rdr.GetString(rdr.GetAssemblyReference(aref).Name);
                TryLoadAssemblyMetadata(aname);
            }
            foreach (var tr in rdr.TypeReferences)
            {
                var tref = rdr.GetTypeReference(tr);
                var scope = tref.ResolutionScope;
                if (scope.Kind == HandleKind.AssemblyReference)
                {
                    var aref = rdr.GetAssemblyReference((AssemblyReferenceHandle)scope);
                    var aname = rdr.GetString(aref.Name);
                    TryLoadAssemblyMetadata(aname);
                }
            }
        }

        // Build per-type method signature index for loaded assemblies
        foreach (var kv in assemblyMetadata.ToList())
        {
            var asmName = kv.Key;
            var readerA = kv.Value;
            // For each TypeDef in assembly, index methods
            foreach (var tdh in readerA.TypeDefinitions)
            {
                var td = readerA.GetTypeDefinition(tdh);
                var ns = readerA.GetString(td.Namespace);
                var name = readerA.GetString(td.Name);
                var full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
                var methods = new Dictionary<string, (string returnType, List<string> paramTypes, byte[] raw)>(StringComparer.Ordinal);
                foreach (var mh in td.GetMethods())
                {
                    var md = readerA.GetMethodDefinition(mh);
                    var mname = readerA.GetString(md.Name);
                    var raw = readerA.GetBlobBytes(md.Signature);
                    var decoded = DecodeMethodSignature(readerA, md.Signature);
                    methods[mname] = (decoded.returnType, decoded.paramTypes, raw);
                }
                assemblyTypeMethodIndex[(asmName, full)] = methods;
                Console.WriteLine($" Loaded {asmName}::{full} ({methods.Count} methods)");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Comparing MemberRefs (generated exe) to Fifth.System.KG MethodDefs:");
        using (var fs3 = File.OpenRead(exePath))
        using (var peReader3 = new PEReader(fs3))
        {
            var reader3 = peReader3.GetMetadataReader();
            foreach (var mrh in reader3.MemberReferences)
            {
                var mr = reader3.GetMemberReference(mrh);
                var name = reader3.GetString(mr.Name);
                string parentStr = mr.Parent.Kind.ToString();
                if (mr.Parent.Kind == HandleKind.TypeReference)
                {
                    var parentTypeRef = reader3.GetTypeReference((TypeReferenceHandle)mr.Parent);
                    parentStr = reader3.GetString(parentTypeRef.Namespace) + "." + reader3.GetString(parentTypeRef.Name);
                }
                if (!parentStr.Contains("Fifth.System")) continue;

                var blobBytes = reader3.GetBlobBytes(mr.Signature);
                var decodedMem = DecodeMethodSignature(reader3, mr.Signature);
                Console.Write($" MemberRef: {parentStr}::{name}  ret={decodedMem.returnType} params=[{string.Join(",", decodedMem.paramTypes)}] blobLen={blobBytes.Length}");
                // Determine parent assembly and type (if available) to find the MethodDef signature to compare
                string parentAssembly = "";
                string parentTypeFullName = parentStr;
                if (mr.Parent.Kind == HandleKind.TypeReference)
                {
                    var parentTypeRef = reader3.GetTypeReference((TypeReferenceHandle)mr.Parent);
                    var scope = parentTypeRef.ResolutionScope;
                    if (scope.Kind == HandleKind.AssemblyReference)
                    {
                        var aref = reader3.GetAssemblyReference((AssemblyReferenceHandle)scope);
                        parentAssembly = reader3.GetString(aref.Name);
                    }
                    var ns = reader3.GetString(parentTypeRef.Namespace);
                    var tn = reader3.GetString(parentTypeRef.Name);
                    parentTypeFullName = string.IsNullOrEmpty(ns) ? tn : ns + "." + tn;
                }

                if (!string.IsNullOrEmpty(parentAssembly) && assemblyTypeMethodIndex.TryGetValue((parentAssembly, parentTypeFullName), out var methodMap))
                {
                    if (methodMap.TryGetValue(name, out var defEntry))
                    {
                        Console.Write($"   vs {parentAssembly}::{parentTypeFullName}.{name} ret={defEntry.returnType} params=[{string.Join(",", defEntry.paramTypes)}] blobLen={defEntry.raw.Length}");
                        if (decodedMem.returnType != defEntry.returnType || decodedMem.paramTypes.Count != defEntry.paramTypes.Count || !decodedMem.paramTypes.SequenceEqual(defEntry.paramTypes))
                        {
                            Console.Write("   => SIGNATURE MISMATCH");
                        }
                    }
                    else
                    {
                        Console.Write($"   vs {parentAssembly}::{parentTypeFullName}.{name} => METHOD NOT FOUND");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(parentAssembly))
                    {
                        Console.Write($"   => Target assembly metadata not loaded: {parentAssembly}");
                    }
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine();
        return 0;
    }

    // Helper: decode a method signature blob into a return-type string and list of parameter-type strings
    static (string returnType, List<string> paramTypes) DecodeMethodSignature(MetadataReader reader, BlobHandle sigBlob)
    {
        var br = reader.GetBlobReader(sigBlob);
        // Calling convention (byte)
        byte calling;
        byte paramCount;
        try
        {
            calling = br.ReadByte();
            paramCount = br.ReadByte();
        }
        catch
        {
            return ("<invalid>", new List<string>());
        }

        string DecodeType(MetadataReader r, ref BlobReader b)
        {
            try
            {
                var typeByte = b.ReadByte();
                switch (typeByte)
                {
                    case 0x01: return "System.Void"; // ELEMENT_TYPE_VOID
                    case 0x02: return "System.Boolean"; // ELEMENT_TYPE_BOOLEAN
                    case 0x03: return "System.Char"; // ELEMENT_TYPE_CHAR
                    case 0x04: return "System.SByte"; // ELEMENT_TYPE_I1
                    case 0x05: return "System.Byte"; // ELEMENT_TYPE_U1
                    case 0x06: return "System.Int16"; // ELEMENT_TYPE_I2
                    case 0x07: return "System.UInt16"; // ELEMENT_TYPE_U2
                    case 0x08: return "System.Int32"; // ELEMENT_TYPE_I4
                    case 0x09: return "System.UInt32"; // ELEMENT_TYPE_U4
                    case 0x0a: return "System.Int64"; // ELEMENT_TYPE_I8
                    case 0x0b: return "System.UInt64"; // ELEMENT_TYPE_U8
                    case 0x0c: return "System.Single"; // ELEMENT_TYPE_R4
                    case 0x0d: return "System.Double"; // ELEMENT_TYPE_R8
                    case 0x0e: return "System.String"; // ELEMENT_TYPE_STRING
                    case 0x12: // ELEMENT_TYPE_CLASS
                        {
                            var coded = b.ReadCompressedInteger();
                            var tag = coded & 0x3;
                            var index = coded >> 2;
                            if (tag == 1) // TypeRef
                            {
                                var trh = MetadataTokens.TypeReferenceHandle(index);
                                var tr = r.GetTypeReference(trh);
                                var tns = r.GetString(tr.Namespace);
                                var tname = r.GetString(tr.Name);
                                // Resolve assembly if possible
                                var scope = tr.ResolutionScope;
                                string scopeName = scope.Kind.ToString();
                                if (scope.Kind == HandleKind.AssemblyReference)
                                {
                                    var ar = r.GetAssemblyReference((AssemblyReferenceHandle)scope);
                                    scopeName = r.GetString(ar.Name);
                                }
                                return string.IsNullOrEmpty(tns) ? $"{tname}@{scopeName}" : $"{tns}.{tname}@{scopeName}";
                            }
                            else if (tag == 0) // TypeDef
                            {
                                var tdh = MetadataTokens.TypeDefinitionHandle(index);
                                var td = r.GetTypeDefinition(tdh);
                                var tns = r.GetString(td.Namespace);
                                var tname = r.GetString(td.Name);
                                return string.IsNullOrEmpty(tns) ? tname : $"{tns}.{tname}";
                            }
                            else
                            {
                                return $"TypeSpec({coded})";
                            }
                        }
                }
            }
            catch
            {
            }
            return "<invalid>";
        }

        var retType = DecodeType(reader, ref br);
        var paramList = new List<string>();
        for (int i = 0; i < paramCount; i++)
        {
            var p = DecodeType(reader, ref br);
            paramList.Add(p);
        }
        return (retType, paramList);
    }
}
