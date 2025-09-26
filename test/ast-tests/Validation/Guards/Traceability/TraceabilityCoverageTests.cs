using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TUnit;

namespace Validation.Guards.Traceability
{
    public class TraceabilityCoverageTests
    {
        [Test]
        public async Task TraceabilityJson_ShouldContainAllFRandACKeys_AndReferenceExistingTests()
        {
            var repoRoot = Directory.GetCurrentDirectory();
            // Walk up to repository root heuristically
            var dir = new DirectoryInfo(repoRoot);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "fifthlang.sln")))
            {
                dir = dir.Parent;
            }
            if (dir == null)
                throw new InvalidOperationException("Cannot locate repository root (fifthlang.sln).");

            var tracePath = Path.Combine(dir.FullName, "specs", "002-guard-clause-overload-completeness", "traceability.json");
            await Assert.That(File.Exists(tracePath)).IsTrue();

            using var fs = File.OpenRead(tracePath);
            var doc = JsonDocument.Parse(fs);
            var root = doc.RootElement;

            // Validate FR-001..FR-070
            var frRoot = root.GetProperty("featureRequirements");
            int frTotal = 70;
            int frMappedCount = 0;
            for (int i = 1; i <= frTotal; ++i)
            {
                var key = $"FR-{i:D3}";
                await Assert.That(frRoot.TryGetProperty(key, out var entry)).IsTrue();
                // Ensure 'tests' property exists and is an array
                await Assert.That(entry.TryGetProperty("tests", out var testsProp) && testsProp.ValueKind == JsonValueKind.Array).IsTrue();

                bool anyMapped = false;
                foreach (var t in testsProp.EnumerateArray())
                {
                    var testName = t.GetString();
                    await Assert.That(string.IsNullOrWhiteSpace(testName)).IsFalse();
                    bool found = SearchForTestInRepo(dir.FullName, testName!);
                    await Assert.That(found).IsTrue();
                    anyMapped = true;
                }

                if (anyMapped) frMappedCount++;
            }

            // Validate AC-001..AC-038
            var acRoot = root.GetProperty("acceptanceCriteria");
            int acTotal = 38;
            int acMappedCount = 0;
            for (int i = 1; i <= acTotal; ++i)
            {
                var key = $"AC-{i:D3}";
                await Assert.That(acRoot.TryGetProperty(key, out var entry)).IsTrue();
                await Assert.That(entry.TryGetProperty("tests", out var testsProp) && testsProp.ValueKind == JsonValueKind.Array).IsTrue();

                bool anyMapped = false;
                foreach (var t in testsProp.EnumerateArray())
                {
                    var testName = t.GetString();
                    await Assert.That(string.IsNullOrWhiteSpace(testName)).IsFalse();
                    bool found = SearchForTestInRepo(dir.FullName, testName!);
                    await Assert.That(found).IsTrue();
                    anyMapped = true;
                }

                if (anyMapped) acMappedCount++;
            }

            // Enforce coverage threshold (75%) for FR and AC separately
            int frRequired = (int)Math.Ceiling(0.75 * frTotal);
            int acRequired = (int)Math.Ceiling(0.75 * acTotal);
            await Assert.That(frMappedCount).IsGreaterThanOrEqualTo(frRequired);
            await Assert.That(acMappedCount).IsGreaterThanOrEqualTo(acRequired);
        }

        static bool SearchForTestInRepo(string repoRoot, string testName)
        {
            // Search by file name pattern first
            var testFiles = Directory.GetFiles(Path.Combine(repoRoot, "test"), "*.cs", SearchOption.AllDirectories);
            foreach (var f in testFiles)
            {
                var filename = Path.GetFileNameWithoutExtension(f);
                if (filename.Equals(testName, StringComparison.OrdinalIgnoreCase) || filename.Contains(testName, StringComparison.OrdinalIgnoreCase))
                    return true;
                // fallback: quick content scan for class declaration
                try
                {
                    var content = File.ReadAllText(f);
                    if (content.Contains($"class {testName}") || content.Contains($"record {testName}"))
                        return true;
                }
                catch { }
            }
            return false;
        }
    }
}
