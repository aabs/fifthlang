using System.CommandLine;
using compiler;


// Define command option
var commandOption = new Option<string>(
    name: "--command",
    description: "The command to execute: build (default), run, lint, help")
{
    IsRequired = false
};
commandOption.SetDefaultValue("build");

// Define source option
var sourceOption = new Option<string[]>(
    name: "--source",
    description: "Source file or directory path")
{
    IsRequired = false,
    AllowMultipleArgumentsPerToken = true
};

// Define source manifest option
var sourceManifestOption = new Option<string>(
    name: "--source-manifest",
    description: "Path to a manifest file listing .5th source modules")
{
    IsRequired = false
};

// Define output option  
var outputOption = new Option<string>(
    name: "--output",
    description: "Output executable path");

// Define args option
var argsOption = new Option<string[]>(
    name: "--args",
    description: "Arguments to pass to the program when running")
{
    IsRequired = false,
    AllowMultipleArgumentsPerToken = true
};

// Define keep-temp option
var keepTempOption = new Option<bool>(
    name: "--keep-temp",
    description: "Keep temporary files")
{
    IsRequired = false
};

// Define diagnostics option
var diagnosticsOption = new Option<bool>(
    name: "--diagnostics",
    description: "Enable diagnostic output")
{
    IsRequired = false
};

var rootCommand = new RootCommand("Fifth Language Compiler (fifthc)")
{
    commandOption,
    sourceOption,
    sourceManifestOption,
    outputOption,
    argsOption,
    keepTempOption,
    diagnosticsOption
};

rootCommand.SetHandler(async (command, source, sourceManifest, output, args, keepTemp, diagnostics) =>
{
    var compilerCommand = ParseCommand(command);
    var sourceFiles = source ?? Array.Empty<string>();
    var primarySource = sourceFiles.FirstOrDefault() ?? string.Empty;

    var options = new CompilerOptions(
        Command: compilerCommand,
        Source: primarySource,
        Output: output ?? "",
        Args: args ?? Array.Empty<string>(),
        KeepTemp: keepTemp,
        Diagnostics: diagnostics,
        SourceFiles: sourceFiles,
        SourceManifest: sourceManifest);

    var compiler = new Compiler();
    var result = await compiler.CompileAsync(options);

    // Output diagnostics
    foreach (var diagnostic in result.Diagnostics)
    {
        var level = diagnostic.Level switch
        {
            DiagnosticLevel.Error => "ERROR",
            DiagnosticLevel.Warning => "WARNING",
            DiagnosticLevel.Info => "INFO",
            _ => "UNKNOWN"
        };

        var message = diagnostic.Source != null
            ? $"{level}: {diagnostic.Message} ({diagnostic.Source})"
            : $"{level}: {diagnostic.Message}";

        if (diagnostic.Level == DiagnosticLevel.Error)
        {
            Console.Error.WriteLine(message);
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    Environment.Exit(result.ExitCode);
}, commandOption, sourceOption, sourceManifestOption, outputOption, argsOption, keepTempOption, diagnosticsOption);

return await rootCommand.InvokeAsync(args);

static CompilerCommand ParseCommand(string command)
{
    return command.ToLowerInvariant() switch
    {
        "build" => CompilerCommand.Build,
        "run" => CompilerCommand.Run,
        "lint" => CompilerCommand.Lint,
        "help" => CompilerCommand.Help,
        _ => CompilerCommand.Build // Default to build
    };
}
