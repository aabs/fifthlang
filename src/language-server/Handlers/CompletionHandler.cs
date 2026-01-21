using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Fifth.LanguageServer.Handlers;

public sealed class CompletionHandler : ICompletionHandler
{
    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth"),
            ResolveProvider = false,
            TriggerCharacters = new Container<string>(".")
        };

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        // Basic keyword completion placeholder
        var items = new List<CompletionItem>
        {
            new CompletionItem { Label = "class", Kind = CompletionItemKind.Keyword },
            new CompletionItem { Label = "fun", Kind = CompletionItemKind.Keyword },
            new CompletionItem { Label = "let", Kind = CompletionItemKind.Keyword },
        };

        return Task.FromResult(new CompletionList(items, isIncomplete: false));
    }

    public Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    public void SetCapability(CompletionCapability capability)
    {
    }
}
