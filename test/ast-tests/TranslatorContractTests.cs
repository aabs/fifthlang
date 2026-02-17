using System.Collections.Generic;
using compiler;
using FluentAssertions;
using Xunit;

namespace ast_tests;

public class TranslatorContractTests
{
    [Fact]
    public void Translate_MinimalModule_Should_Return_TranslationResult_With_Sources_And_Mapping()
    {
        var types = new List<LoweredType> { new LoweredType("T") };
        var methods = new List<LoweredMethod> { new LoweredMethod("node1", "M", "source.5th", 1, 1) };
        var module = new LoweredAstModule("poc", types, methods, new[] { "source.5th" });

        var translator = new LoweredAstToRoslynTranslator();
        var result = translator.Translate(module);

        // Contract assertions (fail initially for TDD):
        result.Sources.Should().NotBeEmpty("Translator must emit at least one generated C# source for the POC");
        result.Mapping.Entries.Should().Contain(e => e.NodeId == "node1", "Translator must provide a mapping entry for the POC node");
    }
}
