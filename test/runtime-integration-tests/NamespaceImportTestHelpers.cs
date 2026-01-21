using compiler;

namespace runtime_integration_tests;

internal static class NamespaceImportTestHelpers
{
    public static async Task<string> WriteSourceAsync(string directory, string fileName, string source)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        await File.WriteAllTextAsync(path, source);
        return path;
    }

    public static async Task<CompilationResult> CompileAsync(string sourcePath, string outputFile, bool diagnostics = true)
    {
        var compiler = new Compiler();
        var options = new CompilerOptions(
            Command: CompilerCommand.Build,
            Source: sourcePath,
            Output: outputFile,
            Diagnostics: diagnostics);

        return await compiler.CompileAsync(options);
    }
}
