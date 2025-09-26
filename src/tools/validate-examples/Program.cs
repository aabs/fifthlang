using System;
using System.IO;
using System.Linq;
using compiler;

class Program
{
    static int Main(string[] args)
    {
        var includeNegatives = args.Any(a => a == "--include-negatives" || a == "--include-invalid");

        // Determine root search paths
        var repoRoot = FindRepoRoot();
        var searchPaths = new[]
        {
            Path.Combine(repoRoot, "docs"),
            Path.Combine(repoRoot, "specs"),
            Path.Combine(repoRoot, "src", "parser", "grammar", "test_samples"),
            Path.Combine(repoRoot, "test")
        };

        var allFiles = searchPaths
            .Where(Directory.Exists)
            .SelectMany(p => Directory.EnumerateFiles(p, "*.5th", SearchOption.AllDirectories))
            .Distinct()
            .ToList();

        var files = allFiles
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
                        !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .Where(p => includeNegatives || !IsNegativeTest(p))
            .ToList();

        var skipped = allFiles.Except(files).ToList();

        if (!files.Any())
        {
            Console.WriteLine("No .5th sample files found to validate.");
            return 0;
        }

        Console.WriteLine($"Found {files.Count} .5th files to validate. Skipped {skipped.Count} files (intentional negatives or build artifacts).");

        if (skipped.Any())
        {
            Console.WriteLine("Skipped files summary (first 20):");
            foreach (var s in skipped.Take(20)) Console.WriteLine($"  SKIP: {s}");
        }

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

    static bool IsNegativeTest(string path)
    {
        // Path-based heuristics
        var lower = path.ToLowerInvariant();
        if (lower.Contains(Path.DirectorySeparatorChar + "invalid" + Path.DirectorySeparatorChar)) return true;
        var fileName = Path.GetFileName(lower);
        if (fileName.StartsWith("invalid")) return true;

        // File-content heuristics: look for common negative-test markers in the first N lines
        try
        {
            using var sr = new StreamReader(path);
            for (int i = 0; i < 8; i++)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                var l = line.ToLowerInvariant();
                if (l.Contains("should not parse") || l.Contains("should fail") || l.Contains("invalid test") || l.Contains("intent: invalid") || l.Contains("expect parse error") || l.Contains("negative test")) return true;
            }
        }
        catch
        {
            // If we can't read the file for some reason, avoid treating it as negative to prevent false positives
        }

        return false;
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
