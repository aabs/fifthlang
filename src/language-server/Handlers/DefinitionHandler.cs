using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Microsoft.Extensions.Logging;

namespace Fifth.LanguageServer.Handlers;

public sealed class DefinitionHandler : IDefinitionHandler
{
    private readonly DocumentService _documents;
    private readonly DocumentStore _store;
    private readonly SymbolService _symbols;
    private readonly ILogger<DefinitionHandler> _logger;

    public DefinitionHandler(DocumentService documents, DocumentStore store, SymbolService symbols, ILogger<DefinitionHandler> logger)
    {
        _documents = documents;
        _store = store;
        _symbols = symbols;
        _logger = logger;
    }

    public DefinitionRegistrationOptions GetRegistrationOptions(DefinitionCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth")
        };

    public Task<LocationOrLocationLinks?> Handle(DefinitionParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("lsp.definition uri={Uri} line={Line} col={Col}", request.TextDocument.Uri, request.Position.Line, request.Position.Character);
        if (!_documents.TryGetParsed(request.TextDocument.Uri.ToUri(), out var doc))
        {
            return HandlerResults.EmptyDefinitionAsync();
        }

        var word = _symbols.GetWordAtPosition(doc.Text, request.Position);
        if (word is null)
        {
            return HandlerResults.EmptyDefinitionAsync();
        }

        var workspaceRoot = SymbolService.ResolveWorkspaceRoot(request.TextDocument.Uri.ToUri());
        var index = _symbols.BuildWorkspaceIndex(workspaceRoot, _store.Snapshot(), _documents.SnapshotParsed());
        var location = _symbols.FindDefinition(index, word.Value.word, request.TextDocument.Uri.ToUri());

        return location is null
            ? HandlerResults.EmptyDefinitionAsync()
            : Task.FromResult<LocationOrLocationLinks?>(location);
    }

    public void SetCapability(DefinitionCapability capability)
    {
    }
}
