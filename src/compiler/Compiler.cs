using System.Diagnostics;
using System.Runtime.InteropServices;
using ast;
using code_generator;

namespace compiler;

/// <summary>
/// Main compiler class that orchestrates the entire compilation process
/// </summary>
public class Compiler
{
    private readonly IProcessRunner _processRunner;
    private readonly ILCodeGenerator _ilCodeGenerator;
    private readonly PEEmitter _peEmitter;

    public Compiler() : this(new ProcessRunner())
    {
    }

    public Compiler(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
        _ilCodeGenerator = new ILCodeGenerator();
        _peEmitter = new PEEmitter();
    }

    /// <summary>
    /// Perform compilation with the given options
    /// </summary>
    /// <param name="options">Compilation options</param>
    /// <returns>Compilation result</returns>
    public async Task<CompilationResult> CompileAsync(CompilerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var validationError = options.Validate();
        if (validationError != null)
        {
            return CompilationResult.Failed(1, validationError);
        }

        var stopwatch = Stopwatch.StartNew();
        var diagnostics = new List<Diagnostic>();

        try
        {
            return options.Command switch
            {
                CompilerCommand.Build => await BuildAsync(options, diagnostics),
                CompilerCommand.Run => await RunAsync(options, diagnostics),
                CompilerCommand.Lint => await LintAsync(options, diagnostics),
                CompilerCommand.Help => ShowHelp(),
                _ => CompilationResult.Failed(1, $"Unknown command: {options.Command}")
            };
        }
        finally
        {
            stopwatch.Stop();
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Total compilation time: {stopwatch.ElapsedMilliseconds}ms"));
            }
        }
    }

    private async Task<CompilationResult> BuildAsync(CompilerOptions options, List<Diagnostic> diagnostics)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Phase 1: Parse
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting parse phase"));
            }

            var parseResult = ParsePhase(options, diagnostics);
            if (parseResult.ast == null)
            {
                return CompilationResult.Failed(2, diagnostics);
            }

            // Phase 2: Transform
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting transform phase"));
            }

            var transformedAst = TransformPhase(parseResult.ast, diagnostics);
            if (transformedAst == null)
            {
                return CompilationResult.Failed(3, diagnostics);
            }

            // Phase 3: Code Generation - Select backend
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Starting code generation phase using {options.Backend} backend"));
            }

            // Phase 4: Assembly
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting assembly phase"));
            }

            var (assemblyResult, assemblyPath) = options.Backend switch
            {
                CompilerBackend.Roslyn => await RoslynEmissionPhase(transformedAst, options, diagnostics),
                CompilerBackend.Legacy => await DirectPEEmissionPhase(transformedAst, options, diagnostics),
                _ => await DirectPEEmissionPhase(transformedAst, options, diagnostics)
            };

            if (!assemblyResult)
            {
                return CompilationResult.Failed(4, diagnostics);
            }

            stopwatch.Stop();

            // Determine ILPath for result - not available with direct PE emission
            string? ilPathForResult = null;

            return CompilationResult.Successful(
                diagnostics,
                outputPath: assemblyPath,
                ilPath: ilPathForResult,
                elapsed: stopwatch.Elapsed);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Build failed: {ex.Message}"));
            return CompilationResult.Failed(1, diagnostics);
        }
    }

    private async Task<CompilationResult> RunAsync(CompilerOptions options, List<Diagnostic> diagnostics)
    {
        // First build the program
        var buildResult = await BuildAsync(options, diagnostics);
        if (!buildResult.Success)
        {
            return buildResult;
        }

        // Then run it
        try
        {
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting execution phase"));
            }

            var runResult = await RunPhase(buildResult.OutputPath!, options, diagnostics);

            // Map non-zero exit code to 5, but preserve original for diagnostics
            var exitCode = runResult.exitCode == 0 ? 0 : 5;
            if (runResult.exitCode != 0)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Program exited with code: {runResult.exitCode}"));
            }

            return new CompilationResult(
                runResult.exitCode == 0,
                exitCode,
                diagnostics,
                buildResult.OutputPath,
                buildResult.ILPath,
                buildResult.ElapsedTime);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Execution failed: {ex.Message}"));
            return CompilationResult.Failed(5, diagnostics);
        }
    }

    private async Task<CompilationResult> LintAsync(CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            // Parse phase
            var parseResult = ParsePhase(options, diagnostics);
            if (parseResult.ast == null)
            {
                return CompilationResult.Failed(2, diagnostics);
            }

            // Transform phase (semantic analysis)
            var transformedAst = TransformPhase(parseResult.ast, diagnostics);
            if (transformedAst == null)
            {
                return CompilationResult.Failed(3, diagnostics);
            }

            // Successful lint
            return CompilationResult.Successful(diagnostics);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Lint failed: {ex.Message}"));
            return CompilationResult.Failed(1, diagnostics);
        }
    }

    private CompilationResult ShowHelp()
    {
        var helpText = @"
Fifth Language Compiler (fifthc)

Usage: fifthc [command] [options]

Commands:
  build (default)  Parse, transform, generate IL, and assemble to executable
  run              Same as build, then execute the produced binary
  lint             Parse and apply transformations only, report issues
  help             Show this help message

Options:
  --source <path>      Source file or directory path (required for build/run/lint)
  --output <path>      Output executable path (required for build/run)
  --args <args>        Arguments to pass to program when running
  --keep-temp          Keep temporary IL files
  --diagnostics        Enable diagnostic output

Examples:
  fifthc --source hello.5th --output hello.exe
  fifthc --command run --source hello.5th --output hello.exe --args ""arg1 arg2""
  fifthc --command lint --source src/
";

        Console.WriteLine(helpText);
        return CompilationResult.Successful();
    }

    private (AstThing? ast, int sourceCount) ParsePhase(CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            if (File.Exists(options.Source))
            {
                // Single file
                var ast = FifthParserManager.ParseFile(options.Source);
                return (ast, 1);
            }
            else if (Directory.Exists(options.Source))
            {
                // Directory - find all .5th files
                var files = Directory.GetFiles(options.Source, "*.5th", SearchOption.AllDirectories)
                    .OrderBy(f => f) // Deterministic order
                    .ToArray();

                if (files.Length == 0)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, "No .5th files found in directory", options.Source));
                    return (null, 0);
                }

                // For now, parse the first file (multiple file support can be added later)
                var ast = FifthParserManager.ParseFile(files[0]);
                return (ast, files.Length);
            }
            else
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Source path not found: {options.Source}"));
                return (null, 0);
            }
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Parse error: {ex.Message}", options.Source));
            return (null, 0);
        }
    }

    private AstThing? TransformPhase(AstThing ast, List<Diagnostic> diagnostics)
    {
        try
        {
            var transformed = FifthParserManager.ApplyLanguageAnalysisPhases(ast, diagnostics);

            // If any error-level diagnostics were produced during language analysis (e.g., guard validation), fail transform
            if (diagnostics.Any(d => d.Level == DiagnosticLevel.Error))
            {
                return null;
            }

            return transformed;
        }
        catch (ast_model.CompilationException cex)
        {
            // Surface compilation diagnostics from language phases without extra prefixing
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, cex.Message));
            return null;
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Transform error: {ex.Message}"));
            return null;
        }
    }



    private async Task GenerateRuntimeConfigAsync(string executablePath, List<Diagnostic> diagnostics)
    {
        try
        {
            var executableName = Path.GetFileNameWithoutExtension(executablePath);
            var runtimeConfigPath = Path.Combine(Path.GetDirectoryName(executablePath) ?? "", $"{executableName}.runtimeconfig.json");

            var runtimeConfig = new
            {
                runtimeOptions = new
                {
                    tfm = "net8.0",
                    framework = new
                    {
                        name = "Microsoft.NETCore.App",
                        version = "8.0.0"
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(runtimeConfig, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(runtimeConfigPath, json);

            if (diagnostics.Any(d => d.Level == DiagnosticLevel.Info && d.Message.Contains("Diagnostics mode")))
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Generated runtime config: {runtimeConfigPath}"));
            }
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to generate runtime config: {ex.Message}"));
        }
    }

    private async Task<(int exitCode, string stdout, string stderr)> RunPhase(string executablePath, CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            var arguments = string.Join(" ", options.Args ?? Array.Empty<string>());
            var result = await _processRunner.RunAsync(executablePath, arguments);

            // Stream stdout/stderr (for now, just capture them)
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                Console.Write(result.StandardOutput);
            }

            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                Console.Error.Write(result.StandardError);
            }

            return (result.ExitCode, result.StandardOutput, result.StandardError);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Execution error: {ex.Message}"));
            return (1, "", ex.Message);
        }
    }


    private async Task<(bool success, string? outputPath)> RoslynEmissionPhase(AstThing transformedAst, CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            // Cast to AssemblyDef as expected by the Roslyn translator
            if (transformedAst is not AssemblyDef assemblyDef)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Expected AssemblyDef but got {transformedAst.GetType().Name}"));
                return (false, null);
            }

            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Using Roslyn backend for code generation"));
            }

            // Create the Roslyn translator
            var translator = new LoweredAstToRoslynTranslator();
            
            // Translate the AST to C# sources
            var translationResult = translator.Translate(assemblyDef);
            
            // Check for translation diagnostics
            if (translationResult.Diagnostics != null && translationResult.Diagnostics.Any())
            {
                foreach (var diag in translationResult.Diagnostics)
                {
                    diagnostics.Add(diag);
                }
                
                // If there are any errors, fail the compilation
                if (translationResult.Diagnostics.Any(d => d.Level == DiagnosticLevel.Error))
                {
                    return (false, null);
                }
            }

            // For now, write the generated C# sources to disk for inspection
            if (options.Diagnostics && translationResult.Sources.Any())
            {
                try
                {
                    var debugDir = Path.Combine(Directory.GetCurrentDirectory(), "build_debug_roslyn");
                    Directory.CreateDirectory(debugDir);
                    
                    for (int i = 0; i < translationResult.Sources.Count; i++)
                    {
                        var csPath = Path.Combine(debugDir, $"generated_{i}.cs");
                        await File.WriteAllTextAsync(csPath, translationResult.Sources[i]);
                        diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Generated C# source written to: {csPath}"));
                    }
                }
                catch (System.Exception ex)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to write C# sources for diagnostics: {ex.Message}"));
                }
            }

            // TODO: Compile the C# sources using Roslyn and emit the assembly
            // For now, we'll return an error indicating this is not yet fully implemented
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, "Roslyn backend: Full compilation pipeline not yet implemented. Translation completed successfully, but assembly emission is pending."));
            return (false, null);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Roslyn emission error: {ex.Message}"));
            return (false, null);
        }
    }

    private async Task<(bool success, string? outputPath)> DirectPEEmissionPhase(AstThing transformedAst, CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            // Cast to AssemblyDef as expected by ILCodeGenerator
            if (transformedAst is not AssemblyDef assemblyDef)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Expected AssemblyDef but got {transformedAst.GetType().Name}"));
                return (false, null);
            }

            // Transform AST to IL metamodel
            var ilAssembly = _ilCodeGenerator.TransformToILMetamodel(assemblyDef);

            // When diagnostics are enabled, write the generated IL to disk for inspection
            if (options.Diagnostics)
            {
                try
                {
                    var debugDir = Path.Combine(Directory.GetCurrentDirectory(), "build_debug_il");
                    Directory.CreateDirectory(debugDir);
                    var ilGen = new code_generator.ILCodeGenerator(new code_generator.ILCodeGeneratorConfiguration { OutputDirectory = debugDir });
                    var ilPath = ilGen.GenerateCode(assemblyDef);
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Generated IL written to: {ilPath}"));
                }
                catch (System.Exception ex)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to write IL file for diagnostics: {ex.Message}"));
                }
            }

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(options.Output);
            if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Using direct PE emission"));
            }

            // Emit PE assembly directly
            var success = _peEmitter.EmitAssembly(ilAssembly, options.Output);

            if (!success)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, "Direct PE emission failed"));
                return (false, null);
            }

            // Generate runtime configuration file for framework-dependent execution
            await GenerateRuntimeConfigAsync(options.Output, diagnostics);

            // Set execute permission on Unix-like systems
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    // Use chmod to set execute permission
                    var chmodResult = await _processRunner.RunAsync("chmod", $"+x \"{options.Output}\"");
                    if (!chmodResult.Success && options.Diagnostics)
                    {
                        diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to set execute permission on {options.Output}"));
                    }
                }
                catch (System.Exception ex)
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to set execute permission: {ex.Message}"));
                }
            }

            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Successfully generated PE assembly: {options.Output}"));
            }

            return (true, options.Output);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Direct PE emission error: {ex.Message}"));
            return (false, null);
        }
    }


}