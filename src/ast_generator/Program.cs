// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using ast_generator;
using ast_model;

var folderOption = new Option<DirectoryInfo?>(
    name: "--folder",
    description: "The absolute path of the folder to generate code into.");

var rootCommand = new RootCommand("Generate the AST Support Code");
rootCommand.AddOption(folderOption);

rootCommand.SetHandler((folder) => 
    { 
        GenerateSource(folder!).Wait(); 
    },
    folderOption);

return await rootCommand.InvokeAsync(args);

static async Task GenerateSource(DirectoryInfo folder)
{
    var astTypeProvider = new TypeProvider<ast.AstThing>();
    await WriteSourceToFile(Path.Combine(folder.FullName, "builders.generated.cs"), await new RazorLightBuilderGenerator(astTypeProvider).TransformTextAsync());
    await WriteSourceToFile(Path.Combine(folder.FullName, "visitors.generated.cs"), await new RazorLightVisitorGenerator(astTypeProvider).TransformTextAsync());
    await WriteSourceToFile(Path.Combine(folder.FullName, "rewriter.generated.cs"), await new RazorLightRewriterGenerator(astTypeProvider).TransformTextAsync());
    await WriteSourceToFile(Path.Combine(folder.FullName, "typeinference.generated.cs"), await new RazorLightTypeCheckerGenerator(astTypeProvider).TransformTextAsync());
    var ilAstTypeProvider = new TypeProvider<il_ast.AstThing>();
    await WriteSourceToFile(Path.Combine(folder.FullName, "il.builders.generated.cs"), await new RazorLightBuilderGenerator(ilAstTypeProvider).TransformTextAsync());
    await WriteSourceToFile(Path.Combine(folder.FullName, "il.visitors.generated.cs"), await new RazorLightVisitorGenerator(ilAstTypeProvider).TransformTextAsync());
    await WriteSourceToFile(Path.Combine(folder.FullName, "il.rewriter.generated.cs"), await new RazorLightRewriterGenerator(ilAstTypeProvider).TransformTextAsync());
}

static async Task WriteSourceToFile(string path, string source)
{
    using var sr = new StringReader(source);
    await using StreamWriter sw = File.CreateText(path);
    while (sr.ReadLine() is { } line)
    {
        sw.WriteLine(line);
        sw.Flush(); //force line to be written to disk
    }
}
