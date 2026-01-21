using FluentAssertions;
using Xunit;

namespace LanguageServerSmoke;

public class SmokeTests
{
    [Fact]
    public void LanguageServer_AssemblyLoads()
    {
        var type = typeof(Fifth.LanguageServer.Program);
        type.Should().NotBeNull();
    }
}
