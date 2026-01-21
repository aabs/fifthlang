using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Fifth.LanguageServer.Handlers;

public sealed class HoverHandler : IHoverHandler
{
    public HoverRegistrationOptions GetRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth")
        };

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        // Placeholder hover; real implementation will use parser + symbol lookup
        return Task.FromResult<Hover?>(new Hover
        {
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind = MarkupKind.PlainText,
                Value = "Fifth language server hover not yet implemented."
            }),
            Range = new Range(request.Position, request.Position)
        });
    }

    public void SetCapability(HoverCapability capability)
    {
    }
}
