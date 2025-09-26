using System;
using System.IO;
using System.Linq;
using compiler;

class Program
{
    static int Main(string[] args)
    {
        // Determine root search paths
        var repoRoot = FindRepoRoot();
        var searchPaths = new[]
        {
            Path.Combine(repoRoot, "docs"),
            Path.Combine(repoRoot, "specs"),
            Path.Combine(repoRoot, "src", "parser", "grammar", "test_samples"),
            Path.Combine(repoRoot, "test")
        };

        var files = searchPaths
            .Where(Directory.Exists)
            .SelectMany(p => Directory.EnumerateFiles(p, "*.5th", SearchOption.AllDirectories))
            .Distinct()
            .ToList();

        if (!files.Any())
        {
            Console.WriteLine("No .5th sample files found to validate.");
            return 0;
        }

        Console.WriteLine($"Found {files.Count} .5th files to validate.");

        int failures = 0;
        foreach (var f in files.OrderBy(x => x))
        {
            Console.WriteLine($"Parsing: {f}");
            try
            {
                FifthParserManager.ParseFileSyntaxOnly(f);
            }
            catch (Exception ex)
            {
                failures++;
                Console.Error.WriteLine($"PARSE ERROR: {f}: {ex.Message}");
            }
        }

        if (failures > 0)
        {
            Console.Error.WriteLine($"Validation failed: {failures} file(s) failed to parse.");
            return 2;
        }

        Console.WriteLine("All .5th samples parsed successfully.");
        return 0;
    }

    static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "fifthlang.sln"))) return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }
        return Directory.GetCurrentDirectory();
    }
}
