using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
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
            var errorMsg = $"Transform error: {ex.Message}\nStack trace:\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMsg += $"\n\nInner exception: {ex.InnerException.Message}\nInner stack trace:\n{ex.InnerException.StackTrace}";
            }
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, errorMsg));
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

            var packageLibDir = GetPackageLibDirectory();
            if (Directory.Exists(packageLibDir))
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Copying dependencies from package lib directory: {packageLibDir}"));
                var filesCopied = 0;
                await Task.Run(() =>
                {
                    CopyDirectory(packageLibDir, outputDir, fileName =>
                    {
                        var lower = fileName.ToLowerInvariant();
                        return lower == "compiler" || lower == "compiler.exe" || lower == "compiler.dll";
                    }, ref filesCopied);
                });
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Copied {filesCopied} runtime dependency files"));
                return;
            }

            diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Package lib directory not found at {packageLibDir}; falling back to assembly locations"));

            // Fallback path for developer builds where lib directory may not exist yet
            await TryCopyAssemblyAsync(typeof(Fifth.System.KG).Assembly, outputDir, "Fifth.System.dll", diagnostics);
            await TryCopyAssemblyAsync(typeof(VDS.RDF.IGraph).Assembly, outputDir, "dotNetRdf.dll", diagnostics);
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

            // Get required references using multiple strategies to support self-contained bundles
            var references = new List<Microsoft.CodeAnalysis.MetadataReference>();
            var referencePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referenceAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddReferenceIfExists(string? candidatePath)
            {
                if (string.IsNullOrWhiteSpace(candidatePath))
                {
                    return;
                }

                try
                {
                    var normalized = Path.GetFullPath(candidatePath);
                    if (!File.Exists(normalized) || !referencePaths.Add(normalized))
                    {
                        return;
                    }

                    if (!string.Equals(Path.GetExtension(normalized), ".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        referencePaths.Remove(normalized);
                        return;
                    }

                    var simpleName = Path.GetFileNameWithoutExtension(normalized);
                    if (!string.IsNullOrWhiteSpace(simpleName) && !referenceAssemblyNames.Add(simpleName))
                    {
                        referencePaths.Remove(normalized);
                        return;
                    }

                    references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(normalized));
                }
                catch
                {
                    // Ignore invalid reference entries; fall back to runtime-provided metadata if necessary
                }
            }

            void AddReferenceFromAssembly(Assembly? assembly)
            {
                if (assembly == null || assembly.IsDynamic)
                {
                    return;
                }

                var identity = assembly.GetName().Name ?? assembly.FullName;
                if (identity != null && !referenceAssemblyNames.Add(identity))
                {
                    return;
                }

                var reference = CreateMetadataReferenceFromAssembly(assembly);
                if (reference != null)
                {
                    references.Add(reference);
                }
            }

            try
            {
                var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
                if (!string.IsNullOrWhiteSpace(tpa))
                {
                    foreach (var path in tpa.Split(Path.PathSeparator))
                    {
                        AddReferenceIfExists(path);
                    }
                }
            }
            catch
            {
                // Best-effort only – the runtime may not expose TPA data in all environments
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AddReferenceFromAssembly(assembly);
            }

            try
            {
                AddReferenceFromAssembly(Assembly.Load("netstandard"));
            }
            catch
            {
                // Ignore – not all runtimes expose netstandard facade
            }

            var packageLibDir = GetPackageLibDirectory();
            if (Directory.Exists(packageLibDir))
            {
                foreach (var dllPath in Directory.EnumerateFiles(packageLibDir, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    AddReferenceIfExists(dllPath);
                }
            }

            var baseDir = Path.GetDirectoryName(options.Output) ?? Directory.GetCurrentDirectory();
            AddReferenceIfExists(Path.Combine(baseDir, "Fifth.System.dll"));
            AddReferenceIfExists(Path.Combine(baseDir, "dotNetRdf.dll"));
            AddReferenceIfExists(Path.Combine(baseDir, "dotNetRdf.Client.dll"));
            AddReferenceIfExists(Path.Combine(baseDir, "VDS.Common.dll"));

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

    private static string GetPackageLibDirectory()
    {
        var baseDir = AppContext.BaseDirectory;
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            baseDir = Directory.GetCurrentDirectory();
        }

        return Path.GetFullPath(Path.Combine(baseDir, "..", "lib"));
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, Func<string, bool>? skipFilePredicate, ref int filesCopied)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var filePath in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(filePath);
            if (skipFilePredicate != null && skipFilePredicate(fileName))
            {
                continue;
            }

            var destPath = Path.Combine(destinationDir, fileName);
            File.Copy(filePath, destPath, overwrite: true);
            filesCopied++;
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubdir = Path.Combine(destinationDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubdir, skipFilePredicate, ref filesCopied);
        }
    }

    private static async Task TryCopyAssemblyAsync(Assembly assembly, string outputDir, string friendlyName, List<Diagnostic> diagnostics)
    {
        try
        {
            var assemblyPath = assembly.Location;
            if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath))
            {
                var destination = Path.Combine(outputDir, Path.GetFileName(assemblyPath));
                await Task.Run(() => File.Copy(assemblyPath, destination, overwrite: true));
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Info, $"Copied {friendlyName} to output directory"));
            }
            else
            {
                diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"{friendlyName} assembly not found at: {assemblyPath}"));
            }
        }
        catch (System.Exception ex)
        {
            diagnostics.Add(new Diagnostic(DiagnosticLevel.Warning, $"Failed to copy {friendlyName}: {ex.Message}"));
        }
    }

    private static Microsoft.CodeAnalysis.MetadataReference? CreateMetadataReferenceFromAssembly(Assembly assembly)
    {
        var location = assembly.Location;
        if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
        {
            return Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(location);
        }

        try
        {
            unsafe
            {
                if (assembly.TryGetRawMetadata(out byte* metadata, out int length))
                {
                    var moduleMetadata = ModuleMetadata.CreateFromMetadata((IntPtr)metadata, length);
                    var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);
                    var display = assembly.GetName().Name ?? assembly.FullName ?? Guid.NewGuid().ToString();
                    return assemblyMetadata.GetReference(display: display);
                }
            }
        }
        catch
        {
            // Some runtime assemblies (notably generated ones) do not expose raw metadata
        }

        return null;
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