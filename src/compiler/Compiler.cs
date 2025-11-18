using System.Diagnostics;
using System.Runtime.InteropServices;
using ast;

namespace compiler;

/// <summary>
/// Main compiler class that orchestrates the entire compilation process
/// </summary>
public class Compiler
{
    private readonly IProcessRunner _processRunner;

    public Compiler() : this(new ProcessRunner())
    {
    }

    public Compiler(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
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

            // Phase 3: Code Generation using Roslyn backend
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting code generation phase using Roslyn backend"));
            }

            // Phase 4: Assembly
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Starting assembly phase"));
            }

            var (assemblyResult, assemblyPath) = await RoslynEmissionPhase(transformedAst, options, diagnostics);

            if (!assemblyResult)
            {
                return CompilationResult.Failed(4, diagnostics);
            }

            stopwatch.Stop();

            return CompilationResult.Successful(
                diagnostics,
                outputPath: assemblyPath,
                ilPath: null,
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
  build (default)  Parse, transform, and compile to executable
  run              Same as build, then execute the produced binary
  lint             Parse and apply transformations only, report issues
  help             Show this help message

Options:
  --source <path>      Source file or directory path (required for build/run/lint)
  --output <path>      Output executable path (required for build/run)
  --args <args>        Arguments to pass to program when running
  --keep-temp          Keep temporary files
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

    private async Task CopyRuntimeDependenciesAsync(string outputPath, List<Diagnostic> diagnostics)
    {
        diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"CopyRuntimeDependenciesAsync called with output: {outputPath}"));

        try
        {
            var outputDir = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                // If output is just a filename, use current directory
                outputDir = Directory.GetCurrentDirectory();
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Using current directory for dependencies: {outputDir}"));
            }
            else
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Output directory: {outputDir}"));
            }

            // Copy Fifth.System.dll
            try
            {
                var fifthSystemAsm = typeof(Fifth.System.KG).Assembly.Location;
                if (!string.IsNullOrWhiteSpace(fifthSystemAsm) && File.Exists(fifthSystemAsm))
                {
                    var destPath = Path.Combine(outputDir, Path.GetFileName(fifthSystemAsm));
                    if (!File.Exists(destPath) || File.GetLastWriteTimeUtc(fifthSystemAsm) > File.GetLastWriteTimeUtc(destPath))
                    {
                        await Task.Run(() => File.Copy(fifthSystemAsm, destPath, overwrite: true));
                        diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Copied Fifth.System.dll to output directory"));
                    }
                }
                else
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Fifth.System assembly not found at: {fifthSystemAsm}"));
                }
            }
            catch (System.Exception ex)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to copy Fifth.System.dll: {ex.Message}"));
            }

            // Copy VDS.RDF.dll and its dependencies
            try
            {
                var dotNetRdfAsm = typeof(VDS.RDF.IGraph).Assembly.Location;
                if (!string.IsNullOrWhiteSpace(dotNetRdfAsm) && File.Exists(dotNetRdfAsm))
                {
                    var destPath = Path.Combine(outputDir, Path.GetFileName(dotNetRdfAsm));
                    if (!File.Exists(destPath) || File.GetLastWriteTimeUtc(dotNetRdfAsm) > File.GetLastWriteTimeUtc(destPath))
                    {
                        await Task.Run(() => File.Copy(dotNetRdfAsm, destPath, overwrite: true));
                        diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Copied VDS.RDF.dll to output directory"));
                    }
                }
                else
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"VDS.RDF assembly not found at: {dotNetRdfAsm}"));
                }
            }
            catch (System.Exception ex)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to copy VDS.RDF.dll: {ex.Message}"));
            }
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to copy runtime dependencies: {ex.Message}"));
        }
    }

    private async Task<(int exitCode, string stdout, string stderr)> RunPhase(string executablePath, CompilerOptions options, List<Diagnostic> diagnostics)
    {
        try
        {
            // If the executable is a .dll, run it with dotnet
            string command;
            string arguments;

            if (executablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                command = "dotnet";
                var userArgs = string.Join(" ", options.Args ?? Array.Empty<string>());
                arguments = string.IsNullOrWhiteSpace(userArgs) ? $"\"{executablePath}\"" : $"\"{executablePath}\" {userArgs}";
            }
            else
            {
                command = executablePath;
                arguments = string.Join(" ", options.Args ?? Array.Empty<string>());
            }

            var result = await _processRunner.RunAsync(command, arguments);

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
                var debugDir = Path.Combine(Directory.GetCurrentDirectory(), "build_debug_roslyn");

                for (int i = 0; i < translationResult.Sources.Count; i++)
                {
                    await WriteSourceFileWithRetryAsync(debugDir, i, translationResult.Sources[i], diagnostics);
                }
            }

            // Compile the C# sources using Roslyn
            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, "Compiling generated C# sources with Roslyn"));
            }

            // Parse the C# sources into syntax trees
            var syntaxTrees = new List<Microsoft.CodeAnalysis.SyntaxTree>();
            for (int i = 0; i < translationResult.Sources.Count; i++)
            {
                var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
                    translationResult.Sources[i],
                    path: $"generated_{i}.cs",
                    encoding: System.Text.Encoding.UTF8);
                syntaxTrees.Add(syntaxTree);
            }

            // Get required references using Trusted Platform Assemblies for robustness
            var references = new List<Microsoft.CodeAnalysis.MetadataReference>();
            try
            {
                var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
                if (!string.IsNullOrWhiteSpace(tpa))
                {
                    var paths = tpa.Split(Path.PathSeparator);
                    foreach (var p in paths)
                    {
                        try
                        {
                            // Only include core framework assemblies to keep compile fast
                            var fileName = Path.GetFileName(p);
                            if (fileName != null && (fileName.StartsWith("System.") || fileName.StartsWith("Microsoft.") || fileName == "netstandard.dll" || fileName == "mscorlib.dll"))
                            {
                                references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(p));
                            }
                        }
                        catch { /* ignore bad refs */ }
                    }
                }
            }
            catch { /* ignore */ }

            // Always add essential references
            references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));

            // Add references to Fifth.System and dotNetRDF via loaded assembly locations
            try
            {
                var fifthSystemAsm = typeof(Fifth.System.KG).Assembly.Location;
                if (!string.IsNullOrWhiteSpace(fifthSystemAsm) && File.Exists(fifthSystemAsm))
                {
                    references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(fifthSystemAsm));
                }
            }
            catch { /* ignore if assembly not available */ }

            try
            {
                var dotNetRdfAsm = typeof(VDS.RDF.IGraph).Assembly.Location;
                if (!string.IsNullOrWhiteSpace(dotNetRdfAsm) && File.Exists(dotNetRdfAsm))
                {
                    references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(dotNetRdfAsm));
                }
            }
            catch { /* ignore if assembly not available */ }

            // Include local project/runtime dependencies if present (Fifth.System, VDS.RDF)
            void TryAddRef(string path)
            {
                try { if (File.Exists(path)) references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(path)); } catch { }
            }
            var baseDir = Path.GetDirectoryName(options.Output) ?? Directory.GetCurrentDirectory();
            // Typical location next to emitted exe
            TryAddRef(Path.Combine(baseDir, "Fifth.System.dll"));
            // Common dotNetRDF locations: next to exe or in NuGet cache not resolved here; try sibling bin
            TryAddRef(Path.Combine(baseDir, "VDS.RDF.dll"));

            // Create the compilation - emit as DLL for proper .NET Core format
            var assemblyName = Path.GetFileNameWithoutExtension(options.Output);
            var outputPath = Path.ChangeExtension(options.Output, ".dll");
            var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                    Microsoft.CodeAnalysis.OutputKind.ConsoleApplication,
                    optimizationLevel: Microsoft.CodeAnalysis.OptimizationLevel.Debug,
                    platform: Microsoft.CodeAnalysis.Platform.AnyCpu));

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Emit the assembly as .dll
            using var peStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var pdbStream = new FileStream(Path.ChangeExtension(outputPath, ".pdb"), FileMode.Create, FileAccess.Write);

            var emitOptions = new Microsoft.CodeAnalysis.Emit.EmitOptions(
                debugInformationFormat: Microsoft.CodeAnalysis.Emit.DebugInformationFormat.PortablePdb);

            var emitResult = compilation.Emit(peStream, pdbStream, options: emitOptions);

            if (!emitResult.Success)
            {
                // Report compilation errors
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, "Roslyn compilation failed with errors:"));
                foreach (var diagnostic in emitResult.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
                {
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, diagnostic.ToString()));
                }
                return (false, null);
            }

            if (options.Diagnostics)
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Successfully compiled assembly: {outputPath}"));
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Generated PDB: {Path.ChangeExtension(outputPath, ".pdb")}"));
            }

            // Generate runtime configuration file for framework-dependent execution
            await GenerateRuntimeConfigAsync(outputPath, diagnostics);

            // Copy runtime dependencies to output directory
            await CopyRuntimeDependenciesAsync(outputPath, diagnostics);

            // No need to set execute permission on DLLs - they're executed via dotnet runtime

            return (true, outputPath);
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, $"Roslyn emission error: {ex.Message}"));
            return (false, null);
        }
    }

    /// <summary>
    /// Atomically write a generated C# source file with retry logic to handle transient file-lock issues.
    /// Uses unique filenames to avoid cross-process collisions on Windows CI.
    /// </summary>
    private async Task WriteSourceFileWithRetryAsync(string directory, int sourceIndex, string sourceContent, List<Diagnostic> diagnostics)
    {
        const int maxAttempts = 3;
        const int retryDelayMs = 50;

        // Create unique filename to avoid cross-process collisions
        var pid = Environment.ProcessId;
        var ticks = DateTime.UtcNow.Ticks;
        var guid = Guid.NewGuid().ToString("N")[..8]; // Use range operator for modern C#
        var fileName = $"generated_{sourceIndex}_{pid}_{ticks}_{guid}.cs";
        var finalPath = Path.Combine(directory, fileName);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(directory);

                // Write to temporary file first for atomic operation
                var tempGuid = Guid.NewGuid().ToString("N")[..8];
                var tempFileName = $".tmp_{tempGuid}";
                var tempPath = Path.Combine(directory, tempFileName);

                await File.WriteAllTextAsync(tempPath, sourceContent);

                // Atomic move to final location - don't overwrite since filename should be unique
                File.Move(tempPath, finalPath, overwrite: false);

                // Success - log informational message
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Generated C# source written to: {finalPath}"));
                return;
            }
            catch (IOException ioEx) when (attempt < maxAttempts)
            {
                // Transient file-lock issue - retry with backoff
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info,
                    $"Retrying write to {fileName} (attempt {attempt}/{maxAttempts}): {ioEx.Message}"));
                await Task.Delay(retryDelayMs * attempt);
            }
            catch (System.Exception ex)
            {
                // Non-transient error or final attempt failed - try fallback location
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info,
                    $"Failed to write to {directory}: {ex.Message}. Attempting fallback location."));

                try
                {
                    // Fallback to system temp directory
                    var fallbackPath = Path.Combine(Path.GetTempPath(), fileName);
                    await File.WriteAllTextAsync(fallbackPath, sourceContent);
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Info,
                        $"Generated C# source written to fallback location: {fallbackPath}"));
                    return;
                }
                catch (System.Exception fallbackEx)
                {
                    // Even fallback failed - just log it and continue
                    diagnostics.Add(new Diagnostic(DiagnosticLevel.Info,
                        $"Unable to write C# source for diagnostics: {fallbackEx.Message}"));
                    return;
                }
            }
        }

        // All retry attempts exhausted
        diagnostics.Add(new Diagnostic(DiagnosticLevel.Info,
            $"Unable to write C# source after {maxAttempts} attempts. Continuing compilation."));
    }

}