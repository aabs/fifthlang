using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using compiler;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using TUnit;

namespace runtime_integration_tests;

public class RoslynPdbVerificationTests
{
    [Test]
    public void Translator_Should_Emit_MappingEntry_For_Known_Node_In_POC()
    {
        // Construct a minimal lowered module referencing the POC sample
        var methods = new List<LoweredMethod>
        {
            new LoweredMethod("node1", "main", "roslyn-poc-simple.5th", 1, 1)
        };
        var module = new LoweredAstModule("roslyn-poc", new List<LoweredType>(), methods, new[] { "roslyn-poc-simple.5th" });

        var translator = new LoweredAstToRoslynTranslator();
        var result = translator.Translate(module);

        // Failing expectation (POC-driven): translator must produce at least one mapping entry
        // that maps the lowered node 'node1' back to a generated C# source location.
        result.Mapping.Entries.Should().NotBeEmpty("POC translator should populate mapping entries for sample nodes");
        result.Mapping.Entries.Any(e => e.NodeId == "node1").Should().BeTrue("Expected mapping row for NodeId 'node1'");
    }

    [Test]
    public void EmitPortablePdb_HasDocumentsAndMethodDebugInfo()
    {
        var source = "using System; public class Program { public static void Main() { Console.WriteLine(1); } }";
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "Program.cs", encoding: Encoding.UTF8);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "RoslynPdbTest",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);
        var result = compilation.Emit(peStream, pdbStream, options: emitOptions);

        result.Success.Should().BeTrue("Roslyn should successfully emit the compilation and PDB for the sample source");

        pdbStream.Seek(0, SeekOrigin.Begin);
        using var provider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
        var reader = provider.GetMetadataReader();

        reader.Documents.Count.Should().BeGreaterThan(0, "PDB should contain at least one Document entry");
        reader.MethodDebugInformation.Count.Should().BeGreaterThan(0, "PDB should contain method debug information entries");
    }
}
