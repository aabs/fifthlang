using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System.Linq;

namespace Fifth.LanguageServer.Handlers;

public sealed class DocumentSyncHandler : TextDocumentSyncHandlerBase
{
    private readonly DocumentStore _store;

    public DocumentSyncHandler(DocumentStore store)
    {
        _store = store;
    }

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new(uri, "fifth");

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        _store.Open(request.TextDocument.Uri.ToUri(), request.TextDocument.Text);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var text = request.ContentChanges.LastOrDefault()?.Text;
        if (text is not null)
        {
            _store.Update(request.TextDocument.Uri.ToUri(), text);
        }
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _store.Close(request.TextDocument.Uri.ToUri());
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (request.TextDocument is not null && request.Text is not null)
        {
            _store.Update(request.TextDocument.Uri.ToUri(), request.Text);
        }
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions { IncludeText = true },
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth")
        };
}
