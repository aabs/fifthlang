// Generated IL for assembly: 
// Generated at: 2025-09-29 09:58:48 UTC

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
    .locals init (class [System.Runtime]System.Object V_0)
    ldstr "DEBUG: START"
    call void [System.Console]System.Console::WriteLine(string)
    ldstr "TRIPLE_COUNT:"
    call void [System.Console]System.Console::WriteLine(string)
    ldloc V_0
    ldloc V_0
    call class [System.Runtime]System.Object [Fifth.System]Fifth.System.KG::Assert(class VDS.RDF.Triple)
    call class [System.Runtime]System.Object [Fifth.System]Fifth.System.KG::Assert(class VDS.RDF.Triple)
    call class [dotNetRdf]VDS.RDF.IGraph [Fifth.System]Fifth.System.KG::Merge(class [dotNetRdf]VDS.RDF.IGraph, class [dotNetRdf]VDS.RDF.IGraph)
    call int32 [Fifth.System]Fifth.System.KG::CountTriples(class [dotNetRdf]VDS.RDF.IGraph)
    box int32
    call void [System.Console]System.Console::WriteLine(class [System.Runtime]System.Object)
    ldc.i4 0
    ret
}

