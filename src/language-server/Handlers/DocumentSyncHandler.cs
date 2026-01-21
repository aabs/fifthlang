using System.Linq;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Fifth.LanguageServer.Parsing;

namespace Fifth.LanguageServer.Handlers;

public sealed class DocumentSyncHandler : TextDocumentSyncHandlerBase
{
    private readonly DocumentService _documents;
    private readonly ILanguageServerFacade _server;

    public DocumentSyncHandler(DocumentService documents, ILanguageServerFacade server)
    {
        _documents = documents;
        _server = server;
    }

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new(uri, "fifth");

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        Publish(_documents.Open(request.TextDocument.Uri.ToUri(), request.TextDocument.Text));
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var text = request.ContentChanges.LastOrDefault()?.Text;
        if (text is not null)
        {
            Publish(_documents.Update(request.TextDocument.Uri.ToUri(), text));
        }
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _documents.Close(request.TextDocument.Uri.ToUri());
        _server.TextDocument?.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = request.TextDocument.Uri,
            Diagnostics = new Container<Diagnostic>()
        });
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (request.TextDocument is not null && request.Text is not null)
        {
            Publish(_documents.Update(request.TextDocument.Uri.ToUri(), request.Text));
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

    private void Publish(ParsedDocument document)
    {
        var diagnostics = document.Diagnostics.Select(d => new Diagnostic
        {
            Range = new Range(new Position(d.Line, d.Column), new Position(d.Line, d.Column + 1)),
            Message = d.Message,
            Severity = DiagnosticSeverity.Error,
            Source = "fifth"
        }).ToList();

        _server.TextDocument?.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = DocumentUri.From(document.Uri),
            Diagnostics = new Container<Diagnostic>(diagnostics)
        });
    }
}
