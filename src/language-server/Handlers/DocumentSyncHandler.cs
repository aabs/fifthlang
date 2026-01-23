using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Fifth.LanguageServer.Parsing;
using ServerCapabilities = OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Fifth.LanguageServer.Handlers;

public sealed class DocumentSyncHandler : TextDocumentSyncHandlerBase
{
    private readonly DocumentService _documents;
    private readonly ILanguageServerFacade _server;
    private readonly ILogger<DocumentSyncHandler> _logger;

    public DocumentSyncHandler(DocumentService documents, ILanguageServerFacade server, ILogger<DocumentSyncHandler> logger)
    {
        _documents = documents;
        _server = server;
        _logger = logger;
    }

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new(uri, "fifth");

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("lsp.didOpen uri={Uri}", request.TextDocument.Uri);
        Publish(_documents.Open(request.TextDocument.Uri.ToUri(), request.TextDocument.Text));
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var text = request.ContentChanges.LastOrDefault()?.Text;
        if (text is not null)
        {
            _logger.LogInformation("lsp.didChange uri={Uri}", request.TextDocument.Uri);
            Publish(_documents.Update(request.TextDocument.Uri.ToUri(), text));
        }
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("lsp.didClose uri={Uri}", request.TextDocument.Uri);
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
            _logger.LogInformation("lsp.didSave uri={Uri}", request.TextDocument.Uri);
            Publish(_documents.Update(request.TextDocument.Uri.ToUri(), request.Text));
        }
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            Change = ServerCapabilities.TextDocumentSyncKind.Full,
            Save = new ServerCapabilities.SaveOptions { IncludeText = true },
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth")
        };

    private void Publish(ParsedDocument document)
    {
        var diagnostics = document.Diagnostics.Select(d => new Diagnostic
        {
            Range = new LspRange(new Position(d.Line, d.Column), new Position(d.Line, d.Column + 1)),
            Message = d.Message,
            Severity = DiagnosticSeverity.Error,
            Source = "fifth"
        }).ToList();

        foreach (var semantic in document.SemanticDiagnostics)
        {
            var (line, column) = NormalizePosition(semantic.Line, semantic.Column);
            diagnostics.Add(new Diagnostic
            {
                Range = new LspRange(new Position(line, column), new Position(line, column + 1)),
                Message = semantic.Message,
                Severity = semantic.Level switch
                {
                    compiler.DiagnosticLevel.Warning => DiagnosticSeverity.Warning,
                    compiler.DiagnosticLevel.Info => DiagnosticSeverity.Information,
                    _ => DiagnosticSeverity.Error
                },
                Source = "fifth"
            });
        }

        _server.TextDocument?.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = DocumentUri.From(document.Uri),
            Diagnostics = new Container<Diagnostic>(diagnostics)
        });
    }

    private static (int line, int column) NormalizePosition(int? line, int? column)
    {
        var normalizedLine = line.HasValue && line.Value > 0 ? line.Value - 1 : 0;
        var normalizedColumn = column.HasValue && column.Value > 0 ? column.Value - 1 : 0;
        return (normalizedLine, normalizedColumn);
    }
}
