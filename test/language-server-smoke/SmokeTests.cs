using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
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

    [Fact]
    public void DocumentService_UpdatesDiagnosticsOnChange()
    {
        var service = new Fifth.LanguageServer.DocumentService(
            new Fifth.LanguageServer.DocumentStore(),
            new Fifth.LanguageServer.Parsing.ParsingService());

        var uri = new Uri("file:///test.5th");
        var initial = service.Open(uri, "not valid !!!");

        initial.Diagnostics.Should().NotBeEmpty();

        var updated = service.Update(uri, "main(): int { return 0; }");
        updated.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task DefinitionHandler_ResolvesUnopenedWorkspaceFile()
    {
        var temp = Directory.CreateTempSubdirectory("fifth-lsp-");
        var workspaceRoot = temp.FullName;

        var defsPath = Path.Combine(workspaceRoot, "defs.5th");
        var mainPath = Path.Combine(workspaceRoot, "main.5th");

        File.WriteAllText(defsPath, "myprint(int x): int { return x; }");
        var mainText = "main(): int { return myprint(5); }";
        File.WriteAllText(mainPath, mainText);

        Environment.SetEnvironmentVariable("FIFTH_WORKSPACE_ROOT", workspaceRoot);

        try
        {
            var store = new Fifth.LanguageServer.DocumentStore();
            var parsing = new Fifth.LanguageServer.Parsing.ParsingService();
            var documents = new Fifth.LanguageServer.DocumentService(store, parsing);
            var symbols = new Fifth.LanguageServer.SymbolService();
            var handler = new Fifth.LanguageServer.Handlers.DefinitionHandler(documents, store, symbols, NullLogger<Fifth.LanguageServer.Handlers.DefinitionHandler>.Instance);
            var hoverHandler = new Fifth.LanguageServer.Handlers.HoverHandler(documents, store, symbols, NullLogger<Fifth.LanguageServer.Handlers.HoverHandler>.Instance);

            var mainUri = new Uri(mainPath);
            documents.Open(mainUri, mainText);

            var position = GetPosition(mainText, "myprint");
            var result = await handler.Handle(new DefinitionParams
            {
                TextDocument = new TextDocumentIdentifier(DocumentUri.From(mainUri)),
                Position = position
            }, CancellationToken.None);

            result.Should().NotBeNull();
            var resolved = result!.FirstOrDefault();
            resolved.Should().NotBeNull();
            var location = resolved!.Location;
            location.Should().NotBeNull();
            location!.Uri.ToUri().LocalPath.Should().Be(defsPath);

            var hover = await hoverHandler.Handle(new HoverParams
            {
                TextDocument = new TextDocumentIdentifier(DocumentUri.From(mainUri)),
                Position = position
            }, CancellationToken.None);

            hover.Should().NotBeNull();
            hover!.Contents.Should().NotBeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("FIFTH_WORKSPACE_ROOT", null);
            temp.Delete(true);
        }
    }

    [Fact]
    public async Task CompletionHandler_ReturnsSymbolsAndKeywords()
    {
        var temp = Directory.CreateTempSubdirectory("fifth-lsp-");
        var workspaceRoot = temp.FullName;

        try
        {
            Environment.SetEnvironmentVariable("FIFTH_WORKSPACE_ROOT", workspaceRoot);

            var store = new Fifth.LanguageServer.DocumentStore();
            var parsing = new Fifth.LanguageServer.Parsing.ParsingService();
            var documents = new Fifth.LanguageServer.DocumentService(store, parsing);
            var symbols = new Fifth.LanguageServer.SymbolService();
            var handler = new Fifth.LanguageServer.Handlers.CompletionHandler(documents, store, symbols, NullLogger<Fifth.LanguageServer.Handlers.CompletionHandler>.Instance);

            var text = "myprint(int x): int { return x; }\nmain(): int { return myp; }";
            var filePath = Path.Combine(workspaceRoot, "completion.5th");
            File.WriteAllText(filePath, text);
            var uri = new Uri(filePath);
            documents.Open(uri, text);

            var position = GetPositionForIndex(text, text.LastIndexOf("myp", StringComparison.Ordinal) + 2);
            var result = await handler.Handle(new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier(DocumentUri.From(uri)),
                Position = position
            }, CancellationToken.None);

            result.Items.Should().Contain(item => item.Label == "myprint");
            result.Items.Should().Contain(item => item.Detail != null && item.Detail.Contains("myprint(", StringComparison.Ordinal));
        }
        finally
        {
            Environment.SetEnvironmentVariable("FIFTH_WORKSPACE_ROOT", null);
            temp.Delete(true);
        }
    }

    private static Position GetPosition(string text, string word)
    {
        var index = text.IndexOf(word, StringComparison.Ordinal);
        if (index < 0)
        {
            return new Position(0, 0);
        }

        var line = 0;
        var column = 0;
        for (var i = 0; i < index; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 0;
            }
            else
            {
                column++;
            }
        }

        return new Position(line, column);
    }

    private static Position GetPositionForIndex(string text, int index)
    {
        var line = 0;
        var column = 0;
        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 0;
            }
            else
            {
                column++;
            }
        }

        return new Position(line, column);
    }
}
