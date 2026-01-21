using Fifth.LanguageServer.Parsing;
using FluentAssertions;
using Xunit;

namespace LanguageServerSmoke;

public class ParsingServiceTests
{
    [Fact]
    public void Parse_InvalidText_ProducesDiagnostics()
    {
        var service = new ParsingService();
        var result = service.Parse(new Uri("file:///test.5th"), "not valid !!!");

        result.Diagnostics.Should().NotBeEmpty();
        result.Ast.Should().BeNull();
    }
}
