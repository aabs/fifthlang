// Generated IL for assembly: 
// Generated at: 2025-09-29 11:28:21 UTC

.assembly extern System.Runtime
{
    .publickeytoken = (b03f5f7f11d50a3a)
    .ver 8:0:0:0
}

.assembly extern System.Console
{
    .publickeytoken = (b03f5f7f11d50a3a)
    .ver 8:0:0:0
}

.assembly extern Fifth.System
{
    .publickeytoken = ()
    .ver 1:0:0:0
}

.assembly extern dotNetRDF
{
    .publickeytoken = ()
    .ver 3:4:0:0
}

.assembly FifthProgram
{
    .ver 1:0:0:0
}

.module main.dll

.class public auto ansi beforefieldinit Fifth.Generated.Resource
{
    .field public string uri
    .field public int32 value
}

.method public int32 main() cil managed
{
    .entrypoint
    .maxstack 8
    ldstr "DEBUG: START"
    call void [System.Console]System.Console::WriteLine(string)
    ldstr "TRIPLE_COUNT:"
    call void [System.Console]System.Console::WriteLine(string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/s1"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/p"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldc.i4 42
    call class [dotNetRdf]VDS.RDF.ILiteralNode [Fifth.System]Fifth.System.KG::CreateLiteral(class [dotNetRdf]VDS.RDF.IGraph, int32)
    call class [dotNetRdf]VDS.RDF.Triple [Fifth.System]Fifth.System.KG::CreateTriple(class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::Assert(class [dotNetRdf]VDS.RDF.IGraph, class [dotNetRdf]VDS.RDF.Triple)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/s2"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/p"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldc.i4 43
    call class [dotNetRdf]VDS.RDF.ILiteralNode [Fifth.System]Fifth.System.KG::CreateLiteral(class [dotNetRdf]VDS.RDF.IGraph, int32)
    call class [dotNetRdf]VDS.RDF.Triple [Fifth.System]Fifth.System.KG::CreateTriple(class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::Assert(class [dotNetRdf]VDS.RDF.IGraph, class [dotNetRdf]VDS.RDF.Triple)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/s1"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/p"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldc.i4 42
    call class [dotNetRdf]VDS.RDF.ILiteralNode [Fifth.System]Fifth.System.KG::CreateLiteral(class [dotNetRdf]VDS.RDF.IGraph, int32)
    call class [dotNetRdf]VDS.RDF.Triple [Fifth.System]Fifth.System.KG::CreateTriple(class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::Assert(class [dotNetRdf]VDS.RDF.IGraph, class [dotNetRdf]VDS.RDF.Triple)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/s2"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldstr "http://example.org/p"
    call class [dotNetRdf]VDS.RDF.IUriNode [Fifth.System]Fifth.System.KG::CreateUri(class [dotNetRdf]VDS.RDF.IGraph, string)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::CreateGraph()
    ldc.i4 43
    call class [dotNetRdf]VDS.RDF.ILiteralNode [Fifth.System]Fifth.System.KG::CreateLiteral(class [dotNetRdf]VDS.RDF.IGraph, int32)
    call class [dotNetRdf]VDS.RDF.Triple [Fifth.System]Fifth.System.KG::CreateTriple(class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode, class [dotNetRdf]VDS.RDF.INode)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::Assert(class [dotNetRdf]VDS.RDF.IGraph, class [dotNetRdf]VDS.RDF.Triple)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::Merge(class [dotNetRdf]VDS.RDF.IGraph, class [dotNetRdf]VDS.RDF.IGraph)
    call int32 [Fifth.System]Fifth.System.KG::CountTriples(class [dotNetRdf]VDS.RDF.IGraph)
    call void [System.Console]System.Console::WriteLine(int32)
    ldc.i4 0
    ret
}

