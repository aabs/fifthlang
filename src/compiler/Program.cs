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
var sourceOption = new Option<string>(
    name: "--source", 
    description: "Source file or directory path");

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
    description: "Keep temporary IL files")
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

// Define backend option
var backendOption = new Option<string>(
    name: "--backend",
    description: "Backend to use for code generation: legacy (default), roslyn")
{
    IsRequired = false
};
backendOption.SetDefaultValue("legacy");

var rootCommand = new RootCommand("Fifth Language Compiler (fifthc)")
{
    commandOption,
    sourceOption,
    outputOption,
    argsOption,
    keepTempOption,
    diagnosticsOption,
    backendOption
};

rootCommand.SetHandler(async (command, source, output, args, keepTemp, diagnostics, backend) =>
{
    var compilerCommand = ParseCommand(command);
    var compilerBackend = ParseBackend(backend);
    
    var options = new CompilerOptions(
        Command: compilerCommand,
        Source: source ?? "",
        Output: output ?? "",
        Args: args ?? Array.Empty<string>(),
        KeepTemp: keepTemp,
        Diagnostics: diagnostics,
        Backend: compilerBackend);

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
}, commandOption, sourceOption, outputOption, argsOption, keepTempOption, diagnosticsOption, backendOption);

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

static CompilerBackend ParseBackend(string backend)
{
    return backend.ToLowerInvariant() switch
    {
        "roslyn" => CompilerBackend.Roslyn,
        "legacy" => CompilerBackend.Legacy,
        _ => CompilerBackend.Legacy // Default to legacy
    };
}
