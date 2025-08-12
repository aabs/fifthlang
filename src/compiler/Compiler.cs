using System.Diagnostics;
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

    public Compiler() : this(new ProcessRunner())
    {
    }

    public Compiler(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
        _ilCodeGenerator = new ILCodeGenerator();
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

            // Phase 3: IL Generation
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting IL generation phase"));
            }

            var ilPath = IlGenPhase(transformedAst, options, diagnostics);
            if (ilPath == null)
            {
                return CompilationResult.Failed(3, diagnostics);
            }

            // Phase 4: Assembly
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting assembly phase"));
            }

            var assemblyResult = await AssemblePhase(ilPath, options, diagnostics);
            if (!assemblyResult.success)
            {
                return CompilationResult.Failed(4, diagnostics);
            }

            // Cleanup temporary files unless requested to keep them
            if (!options.KeepTemp)
            {
                try
                {
                    File.Delete(ilPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            stopwatch.Stop();
            return CompilationResult.Successful(
                outputPath: assemblyResult.outputPath,
                ilPath: options.KeepTemp ? ilPath : null,
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
            return CompilationResult.Successful();
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
            return FifthParserManager.ApplyLanguageAnalysisPhases(ast);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Transform error: {ex.Message}"));
            return null;
        }
    }

    private string? IlGenPhase(AstThing ast, CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            var pid = Environment.ProcessId;
            var outputName = Path.GetFileNameWithoutExtension(options.Output);
            var tempIlPath = Path.Combine(Path.GetDirectoryName(options.Output) ?? ".", $"{outputName}.tmp.{pid}.il");

            // Generate IL using existing ILCodeGenerator, but write to our temp file
            var ilCode = _ilCodeGenerator.GenerateCode(ast);
            
            // The ILCodeGenerator.GenerateCode returns a path, but we want to control the temp naming
            var generatedIlCode = File.ReadAllText(ilCode);
            File.WriteAllText(tempIlPath, generatedIlCode);
            
            // Clean up the original file if it's different from our temp file
            if (ilCode != tempIlPath)
            {
                try
                {
                    File.Delete(ilCode);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            return tempIlPath;
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"IL generation error: {ex.Message}"));
            return null;
        }
    }

    private async Task<(bool success, string? outputPath)> AssemblePhase(string ilPath, CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            // Locate ilasm
            var ilasmPath = ILAsmLocator.FindILAsm();
            if (ilasmPath == null)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, ILAsmLocator.GetILAsmNotFoundMessage()));
                return (false, null);
            }

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(options.Output);
            if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Run ilasm
            var arguments = $"\"{ilPath}\" /output=\"{options.Output}\"";
            var result = await _processRunner.RunAsync(ilasmPath, arguments);

            if (!result.Success)
            {
                // Show first 40 lines of stderr
                var errorLines = result.StandardError.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var errorToShow = string.Join('\n', errorLines.Take(40));
                if (errorLines.Length > 40)
                {
                    errorToShow += $"\n... ({errorLines.Length - 40} more lines)";
                }

                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"IL assembly failed:\n{errorToShow}"));
                return (false, null);
            }

            return (true, options.Output);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Assembly error: {ex.Message}"));
            return (false, null);
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
}