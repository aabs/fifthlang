using Fifth.LanguageServer.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Fifth.LanguageServer.Handlers;

public sealed class HoverHandler : IHoverHandler
{
    private readonly DocumentService _documents;

    public HoverHandler(DocumentService documents)
    {
        _documents = documents;
    }

    public HoverRegistrationOptions GetRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth")
        };

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        if (!_documents.TryGetParsed(request.TextDocument.Uri.ToUri(), out var doc))
            return Task.FromResult<Hover?>(null);

        // Placeholder hover: echo the current line
        var lines = doc.Text.Split('\n');
        if (request.Position.Line < 0 || request.Position.Line >= lines.Length)
            return Task.FromResult<Hover?>(null);

        var line = lines[request.Position.Line];
        var contents = new MarkedStringsOrMarkupContent(new MarkupContent
        {
            Kind = MarkupKind.PlainText,
            Value = line.Trim()
        });

        return Task.FromResult<Hover?>(new Hover
        {
            Contents = contents,
            Range = new Range(
                new Position(request.Position.Line, 0),
                new Position(request.Position.Line, line.Length))
        });
    }

    public void SetCapability(HoverCapability capability)
    {
    }
}
