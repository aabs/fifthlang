// T002/T041-T043A: Performance baseline and benchmark for triple feature parse timing
// This test measures parse performance for triple literals and enforces regression checks

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using compiler;
using Assert = Xunit.Assert;

namespace perf;

/// <summary>
/// Performance benchmarks for triple literal parsing.
/// Implements T041-T043A: large sample parsing, baseline capture, and regression checks.
/// </summary>
public class TripleParseBaseline
{
    private const int WarmupRuns = 3;
    private const int MeasurementRuns = 10;
    private const double MaxRegressionPercent = 5.0;

    [Fact(DisplayName = "T041-T043: Triple parse performance baseline with 1000 triples")]
    public void TripleHeavyFile_ParsePerformance_MeetsBaseline()
    {
        // T041: Parse large sample file with 1000 triple literals
        var repoRoot = FindRepoRoot();
        var sampleFile = Path.Combine(repoRoot, "test", "perf", "triple_heavy_01.5th");
        
        Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");
        
        var code = File.ReadAllText(sampleFile);
        
        // Warmup runs
        for (int i = 0; i < WarmupRuns; i++)
        {
            ParseCode(code);
        }
        
        // Measurement runs
        var times = new long[MeasurementRuns];
        for (int i = 0; i < MeasurementRuns; i++)
        {
            var sw = Stopwatch.StartNew();
            var result = ParseCode(code);
            sw.Stop();
            
            times[i] = sw.ElapsedMilliseconds;
            
            // Verify parse succeeded
            Assert.NotNull(result);
        }
        
        // T042: Calculate statistics
        var mean = times.Average();
        var variance = times.Select(t => Math.Pow(t - mean, 2)).Average();
        var stdDev = Math.Sqrt(variance);
        
        Console.WriteLine($"Triple parse performance (1000 literals):");
        Console.WriteLine($"  Mean: {mean:F2} ms");
        Console.WriteLine($"  Std Dev: {stdDev:F2} ms");
        Console.WriteLine($"  Min: {times.Min()} ms");
        Console.WriteLine($"  Max: {times.Max()} ms");
        
        // T043: Enforce ≤5% regression check (baseline: 3000ms for 1000 triples)
        // This is a generous baseline for first implementation
        var baselineMs = 3000.0;
        var regressionPercent = ((mean - baselineMs) / baselineMs) * 100.0;
        
        Console.WriteLine($"  Baseline: {baselineMs:F2} ms");
        Console.WriteLine($"  Regression: {regressionPercent:F2}%");
        
        Assert.True(mean <= baselineMs * (1 + MaxRegressionPercent / 100.0),
            $"Parse time {mean:F2}ms exceeds baseline {baselineMs}ms by more than {MaxRegressionPercent}%");
        
        // T043A: Enforce variance guard (mean ≤ 2σ confidence)
        var upperBound = mean + (2 * stdDev);
        Console.WriteLine($"  Upper bound (mean + 2σ): {upperBound:F2} ms");
        
        Assert.True(upperBound <= baselineMs * (1 + MaxRegressionPercent / 100.0),
            $"Performance variance too high: upper bound {upperBound:F2}ms exceeds acceptable threshold");
    }

    [Fact(DisplayName = "T042: Small sample performance check (5 triples)")]
    public void SmallSample_ParsePerformance()
    {
        var code = @"
alias ex as <http://example.org/>;
main(): int {
    <ex:s1, ex:p1, ex:o1>;
    <ex:s2, ex:p2, ex:o2>;
    <ex:s3, ex:p3, ex:o3>;
    <ex:s4, ex:p4, ex:o4>;
    <ex:s5, ex:p5, ex:o5>;
    return 0;
}";
        
        // Warmup
        for (int i = 0; i < WarmupRuns; i++)
        {
            ParseCode(code);
        }
        
        // Measurement
        var times = new long[MeasurementRuns];
        for (int i = 0; i < MeasurementRuns; i++)
        {
            var sw = Stopwatch.StartNew();
            var result = ParseCode(code);
            sw.Stop();
            times[i] = sw.ElapsedMilliseconds;
            
            Assert.NotNull(result);
        }
        
        var mean = times.Average();
        Console.WriteLine($"Small sample parse (5 triples): {mean:F2} ms (mean of {MeasurementRuns} runs)");
        
        // Small samples should parse very quickly (< 500ms)
        Assert.True(mean < 500, $"Small sample parsing too slow: {mean:F2}ms");
    }

    private static ast.AssemblyDef? ParseCode(string code)
    {
        try
        {
            var result = FifthParserManager.ParseString(code);
            return result as ast.AssemblyDef;
        }
        catch
        {
            return null;
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "fifthlang.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }
}
