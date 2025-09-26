using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TUnit;

public class PerfAssertions
{
    [Test]
    public async Task GuardValidationPerformance_ShouldNotRegressBeyondThreshold()
    {
        var repoRoot = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(repoRoot);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "fifthlang.sln")))
        {
            dir = dir.Parent;
        }
        if (dir == null)
        {
            dir = new DirectoryInfo(repoRoot);
        }

        var resultsDir = Path.Combine(dir.FullName, "BenchmarkDotNet.Artifacts", "results");
        var baseline = Path.Combine(dir.FullName, "test", "perf", "baselines", "guard_validation_baseline.json");

        if (!Directory.Exists(resultsDir))
        {
            Console.WriteLine("No benchmark artifacts found under BenchmarkDotNet.Artifacts/results — skipping performance assertion.");
            return;
        }

        await Assert.That(File.Exists(baseline)).IsTrue();

        var psi = new ProcessStartInfo()
        {
            FileName = "python3",
            Arguments = $"scripts/perf/compare_benchmarks.py --baseline \"{baseline}\" --threshold 5",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = dir.FullName
        };

        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        Console.WriteLine(stdout);
        if (!string.IsNullOrWhiteSpace(stderr))
            Console.WriteLine(stderr);

        await Assert.That(proc.ExitCode).IsEqualTo(0);
    }
}
