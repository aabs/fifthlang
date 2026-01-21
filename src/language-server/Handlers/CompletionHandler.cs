using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Fifth.LanguageServer.Handlers;

public sealed class CompletionHandler : ICompletionHandler
{
    private static readonly string[] Keywords = ["class", "fun", "let", "return", "match"];

    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth"),
            ResolveProvider = false,
            TriggerCharacters = new Container<string>(".", ":", " ")
        };

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var items = Keywords.Select(kw => new CompletionItem { Label = kw, Kind = CompletionItemKind.Keyword }).ToList();
        return Task.FromResult(new CompletionList(items, isIncomplete: false));
    }

    public Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken) =>
        Task.FromResult(request);

    public void SetCapability(CompletionCapability capability)
    {
    }
}
