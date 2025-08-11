using System.CommandLine;


var folderOption = new Option<DirectoryInfo?>(
    name: "--folder",
    description: "The absolute path of the folder to generate code into.");

var rootCommand = new RootCommand("Generate the AST Support Code");
rootCommand.AddOption(folderOption);

rootCommand.SetHandler((folder) => 
    { 
        DoSomething(folder!).Wait(); 
    },
    folderOption);

return await rootCommand.InvokeAsync(args);

static async Task DoSomething(DirectoryInfo folder)
{
}
