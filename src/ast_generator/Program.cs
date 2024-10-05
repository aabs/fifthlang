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
    var astTypeProvider = new TypeProvider() { NamespaceScope = "ast" };
    await WriteSourceToFile(Path.Combine(folder.FullName, "builders.generated.cs"), new AstBuilderGenerator(astTypeProvider).TransformText());
    await WriteSourceToFile(Path.Combine(folder.FullName, "visitors.generated.cs"), new AstVisitors(astTypeProvider).TransformText());
    await WriteSourceToFile(Path.Combine(folder.FullName, "typeinference.generated.cs"), new AstTypeCheckerGenerator(astTypeProvider).TransformText());
    var ilAstTypeProvider = new TypeProvider() { NamespaceScope = "il_ast" };
    await WriteSourceToFile(Path.Combine(folder.FullName, "il.builders.generated.cs"), new AstBuilderGenerator(ilAstTypeProvider).TransformText());
    await WriteSourceToFile(Path.Combine(folder.FullName, "il.visitors.generated.cs"), new AstVisitors(ilAstTypeProvider).TransformText());
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
