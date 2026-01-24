using System.CommandLine;
using compiler;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
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

        // Define output type option
        var outputTypeOption = new Option<string>(
            name: "--output-type",
            description: "Output type: Exe or Library")
        {
            IsRequired = false
        };
        outputTypeOption.SetDefaultValue("Exe");

        // Define reference option
        var referenceOption = new Option<string[]>(
            name: "--reference",
            description: "Assembly reference path")
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true
        };

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
            outputTypeOption,
            referenceOption,
            argsOption,
            keepTempOption,
            diagnosticsOption
        };

        var exitCode = 0;

        rootCommand.SetHandler(async context =>
        {
            var command = context.ParseResult.GetValueForOption(commandOption) ?? "build";
            var source = context.ParseResult.GetValueForOption(sourceOption) ?? Array.Empty<string>();
            var sourceManifest = context.ParseResult.GetValueForOption(sourceManifestOption);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var outputType = context.ParseResult.GetValueForOption(outputTypeOption) ?? "Exe";
            var reference = context.ParseResult.GetValueForOption(referenceOption) ?? Array.Empty<string>();
            var args = context.ParseResult.GetValueForOption(argsOption) ?? Array.Empty<string>();
            var keepTemp = context.ParseResult.GetValueForOption(keepTempOption);
            var diagnostics = context.ParseResult.GetValueForOption(diagnosticsOption);

            var compilerCommand = ParseCommand(command);
            var resolvedSourceFiles = new List<string>();
            var sourceDirectories = new List<string>();

            foreach (var sourceEntry in source)
            {
                if (Directory.Exists(sourceEntry))
                {
                    sourceDirectories.Add(sourceEntry);
                    continue;
                }

                resolvedSourceFiles.Add(sourceEntry);
            }

            foreach (var directory in sourceDirectories)
            {
                resolvedSourceFiles.AddRange(Directory.GetFiles(directory, "*.5th", SearchOption.TopDirectoryOnly));
            }

            var primarySource = resolvedSourceFiles.FirstOrDefault()
                ?? sourceDirectories.FirstOrDefault()
                ?? string.Empty;

            var options = new CompilerOptions(
                Command: compilerCommand,
                Source: primarySource,
                Output: output ?? "",
                OutputType: outputType,
                Args: args,
                KeepTemp: keepTemp,
                Diagnostics: diagnostics,
                SourceFiles: resolvedSourceFiles,
                SourceManifest: sourceManifest,
                References: reference);

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

            exitCode = result.ExitCode;
        });

        await rootCommand.InvokeAsync(args);
        return exitCode;
    }

    private static CompilerCommand ParseCommand(string command)
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
}
