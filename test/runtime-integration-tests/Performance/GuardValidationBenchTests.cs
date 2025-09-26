using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;
using TUnit.Core;
using compiler;

namespace runtime_integration_tests.Performance;

public class GuardValidationBenchTests : RuntimeTestBase
{
    private readonly string _specScenarios = Path.Combine(FindRepoRoot(), "specs", "002-guard-clause-overload-completeness", "perf-scenarios.json");

    private static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "fifthlang.sln"))) return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }
        return Directory.GetCurrentDirectory();
    }

    private string GenerateScenarioSource(int groupCount, int overloadsPerGroup, int avgAtoms)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("main() => 0;");
        for (int g = 0; g < groupCount; g++)
        {
            var funcName = $"bench_{g}";
            for (int o = 0; o < overloadsPerGroup; o++)
            {
                // create a simple analyzable constraint using integer comparisons
                int val = (o % 100) - 50; // some range
                sb.AppendLine($"{funcName}(int x | x == {val}) {{ return 0; }}");
            }
        }
        return sb.ToString();
    }

    [Test]
    public async Task BenchPoolingImpact_ShouldReportTimesWhenEnabled()
    {
        var benchEnabled = (Environment.GetEnvironmentVariable("FIFTH_GUARD_VALIDATION_BENCH") ?? string.Empty) == "1";
        if (!benchEnabled)
        {
            Console.WriteLine("BENCH: Skipping micro-bench since FIFTH_GUARD_VALIDATION_BENCH is not set.");
            return;
        }

        var json = File.ReadAllText(_specScenarios);
        using var doc = JsonDocument.Parse(json);
        var scenarios = doc.RootElement.GetProperty("scenarios");

        foreach (var scenario in scenarios.EnumerateArray())
        {
            var id = scenario.GetProperty("id").GetString();
            var groupCount = scenario.GetProperty("groupCount").GetInt32();
            var overloadsPerGroup = scenario.GetProperty("overloadsPerGroup").GetInt32();
            var avgAtoms = scenario.GetProperty("avgAtoms").GetInt32();

            Console.WriteLine($"BENCH: Running scenario {id} groups={groupCount} overloads={overloadsPerGroup} atoms={avgAtoms}");

            var source = GenerateScenarioSource(groupCount, overloadsPerGroup, avgAtoms);
            var srcPath = Path.Combine(TempDirectory, $"bench_{id}.5th");
            await File.WriteAllTextAsync(srcPath, source);
            GeneratedFiles.Add(srcPath);

            // Warmup
            Environment.SetEnvironmentVariable("FIFTH_GUARD_VALIDATION_POOL", "0");
            var compiler = new Compiler();
            var options = new CompilerOptions(CompilerCommand.Build, srcPath, Path.Combine(TempDirectory, $"bench_{id}_nopool.exe"), Diagnostics: true);
            var warm = await compiler.CompileAsync(options);

            int runs = 3;
            var noPoolTimes = new List<long>();
            var poolTimes = new List<long>();

            for (int r = 0; r < runs; r++)
            {
                Environment.SetEnvironmentVariable("FIFTH_GUARD_VALIDATION_POOL", "0");
                compiler = new Compiler();
                var sw = Stopwatch.StartNew();
                var res = await compiler.CompileAsync(options);
                sw.Stop();
                noPoolTimes.Add(sw.ElapsedMilliseconds);

                Environment.SetEnvironmentVariable("FIFTH_GUARD_VALIDATION_POOL", "1");
                compiler = new Compiler();
                var options2 = new CompilerOptions(CompilerCommand.Build, srcPath, Path.Combine(TempDirectory, $"bench_{id}_pool.exe"), Diagnostics: true);
                sw = Stopwatch.StartNew();
                var res2 = await compiler.CompileAsync(options2);
                sw.Stop();
                poolTimes.Add(sw.ElapsedMilliseconds);
            }

            long medianNoPool = noPoolTimes.OrderBy(x => x).ElementAt(runs / 2);
            long medianPool = poolTimes.OrderBy(x => x).ElementAt(runs / 2);

            Console.WriteLine($"BENCH: Scenario {id} medianNoPool={medianNoPool}ms medianPool={medianPool}ms");

            var assertPerf = (Environment.GetEnvironmentVariable("FIFTH_GUARD_VALIDATION_PERF_ASSERT") ?? string.Empty) == "1";
            if (assertPerf)
            {
                // Enforce that pooling should not make things worse by >5%
                var allowed = (double)medianNoPool * 1.05;
                medianPool.Should().BeLessOrEqualTo((long)Math.Ceiling(allowed), "Pooling should not increase median compile time by >5% in this environment");
            }
        }
    }
}
