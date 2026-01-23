using System.Linq;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Fifth.LanguageServer.Handlers;

public sealed class CompletionHandler : ICompletionHandler
{
    private static readonly string[] Keywords = ["class", "fun", "let", "return", "match"];

    private readonly DocumentService _documents;
    private readonly DocumentStore _store;
    private readonly SymbolService _symbols;
    private readonly ILogger<CompletionHandler> _logger;

    public CompletionHandler(DocumentService documents, DocumentStore store, SymbolService symbols, ILogger<CompletionHandler> logger)
    {
        _documents = documents;
        _store = store;
        _symbols = symbols;
        _logger = logger;
    }

    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("fifth"),
            ResolveProvider = false,
            TriggerCharacters = new Container<string>(".", ":", " ")
        };

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("lsp.completion uri={Uri} line={Line} col={Col}", request.TextDocument.Uri, request.Position.Line, request.Position.Character);
        var items = new List<CompletionItem>();
        var prefix = string.Empty;

        if (_documents.TryGetParsed(request.TextDocument.Uri.ToUri(), out var doc))
        {
            var word = _symbols.GetWordAtPosition(doc.Text, request.Position);
            if (word is not null)
            {
                prefix = word.Value.word;
            }

            var requestUri = request.TextDocument.Uri.ToUri();
            var workspaceRoot = SymbolService.ResolveWorkspaceRoot(requestUri);
            var index = _symbols.BuildWorkspaceIndex(workspaceRoot, _store.Snapshot(), _documents.SnapshotParsed());

            var allDefinitions = index.Definitions.Values.SelectMany(d => d).ToList();
            var scopedDefinitions = allDefinitions.Where(d => d.Uri == requestUri).ToList();
            if (scopedDefinitions.Count == 0)
            {
                scopedDefinitions = allDefinitions;
            }

            foreach (var definition in scopedDefinitions)
            {
                if (!string.IsNullOrEmpty(prefix) && !definition.Name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                items.Add(new CompletionItem
                {
                    Label = definition.Name,
                    Kind = MapKind(definition.Kind),
                    Detail = definition.Signature ?? definition.QualifiedName ?? definition.Name
                });
            }
        }

        foreach (var keyword in Keywords)
        {
            if (!string.IsNullOrEmpty(prefix) && !keyword.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            items.Add(new CompletionItem { Label = keyword, Kind = CompletionItemKind.Keyword });
        }

        var distinct = items
            .GroupBy(i => i.Label, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        return Task.FromResult(new CompletionList(distinct, isIncomplete: false));
    }

#pragma warning disable VSTHRD200
    public Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken) =>
        Task.FromResult(request);
#pragma warning restore VSTHRD200

    public void SetCapability(CompletionCapability capability)
    {
    }

    private static CompletionItemKind MapKind(ast.SymbolKind kind) => kind switch
    {
        ast.SymbolKind.FunctionDef => CompletionItemKind.Function,
        ast.SymbolKind.MethodDef => CompletionItemKind.Method,
        ast.SymbolKind.ClassDef => CompletionItemKind.Class,
        ast.SymbolKind.StructDef => CompletionItemKind.Struct,
        ast.SymbolKind.PropertyDef => CompletionItemKind.Property,
        ast.SymbolKind.FieldDef => CompletionItemKind.Field,
        ast.SymbolKind.ParamDef => CompletionItemKind.Variable,
        ast.SymbolKind.VarDeclStatement => CompletionItemKind.Variable,
        ast.SymbolKind.TypeDef => CompletionItemKind.Class,
        ast.SymbolKind.TypeParameter => CompletionItemKind.TypeParameter,
        _ => CompletionItemKind.Text
    };
}
