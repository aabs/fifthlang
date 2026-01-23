using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Fifth.LanguageServer.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Fifth.LanguageServer.Handlers;

public sealed class HoverHandler : IHoverHandler
{
    private readonly DocumentService _documents;
    private readonly DocumentStore _store;
    private readonly SymbolService _symbols;
    private readonly ILogger<HoverHandler> _logger;

    public HoverHandler(DocumentService documents, DocumentStore store, SymbolService symbols, ILogger<HoverHandler> logger)
    {
        _documents = documents;
        _store = store;
        _symbols = symbols;
        _logger = logger;
    }

    public HoverRegistrationOptions GetRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth")
        };

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("lsp.hover uri={Uri} line={Line} col={Col}", request.TextDocument.Uri, request.Position.Line, request.Position.Character);
        if (!_documents.TryGetParsed(request.TextDocument.Uri.ToUri(), out var doc))
            return Task.FromResult<Hover?>(null);

        var word = _symbols.GetWordAtPosition(doc.Text, request.Position);
        if (word is null)
        {
            return Task.FromResult<Hover?>(null);
        }

        var workspaceRoot = SymbolService.ResolveWorkspaceRoot(request.TextDocument.Uri.ToUri());
        var index = _symbols.BuildWorkspaceIndex(workspaceRoot, _store.Snapshot(), _documents.SnapshotParsed());
        var definition = _symbols.FindDefinition(index, word.Value.word, request.TextDocument.Uri.ToUri());
        if (definition is null)
        {
            return Task.FromResult<Hover?>(null);
        }

        var resolved = definition.FirstOrDefault();
        if (resolved is null)
        {
            return Task.FromResult<Hover?>(null);
        }

        var location = resolved.Location;
        var locationUri = location?.Uri.ToUri();
        var fileName = locationUri?.IsFile == true ? Path.GetFileName(locationUri.LocalPath) : null;
        var lines = new List<string>();

        var symbolEntry = index.Definitions.TryGetValue(word.Value.word, out var entries)
            ? entries.FirstOrDefault(e => e.Uri == locationUri) ?? entries.FirstOrDefault()
            : null;

        if (symbolEntry?.Signature is not null)
        {
            lines.Add(symbolEntry.Signature);
        }
        else
        {
            lines.Add(word.Value.word);
        }

        if (symbolEntry is not null)
        {
            lines.Add($"Kind: {symbolEntry.Kind}");
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            lines.Add($"Defined in: {fileName}");
        }

        var contents = new MarkedStringsOrMarkupContent(new MarkupContent
        {
            Kind = MarkupKind.PlainText,
            Value = string.Join("\n", lines)
        });

        return Task.FromResult<Hover?>(new Hover
        {
            Contents = contents,
            Range = word.Value.range
        });
    }

    public void SetCapability(HoverCapability capability)
    {
    }
}
