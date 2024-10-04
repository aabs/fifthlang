// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using ast_generator;

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
    await WriteSourceToFile(Path.Combine(folder.FullName, "builders.generated.cs"), new AstBuilderGenerator().TransformText());
    await WriteSourceToFile(Path.Combine(folder.FullName, "mutator-visitors.generated.cs"), new AstMutatorVisitors().TransformText());
    await WriteSourceToFile(Path.Combine(folder.FullName, "visitors.generated.cs"), new AstVisitors().TransformText());
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
