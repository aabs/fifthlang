// Here you could define global logic that would affect all tests

// Assembly-level attributes for code coverage
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace fifth_runtime_tests;

// xUnit uses ICollectionFixture for shared setup/teardown across tests
// For now, we'll keep the console output logic but convert to xUnit patterns
// Individual tests can implement IAsyncLifetime if they need per-test setup/teardown
public class GlobalHooks
{
    static GlobalHooks()
    {
        Console.WriteLine(@"Or you can define methods that do stuff before...");
    }
}
