namespace Fifth.LanguageServer;

using Fifth.LanguageServer.Parsing;

public sealed class DocumentService
{
    private readonly DocumentStore _store;
    private readonly ParsingService _parsingService;

    private readonly Dictionary<Uri, ParsedDocument> _parsedDocuments = new();

    public DocumentService(DocumentStore store, ParsingService parsingService)
    {
        _store = store;
        _parsingService = parsingService;
    }

    public ParsedDocument Open(Uri uri, string text) => Update(uri, text);

    public ParsedDocument Update(Uri uri, string text)
    {
        _store.Update(uri, text);
        var parsed = _parsingService.Parse(uri, text);
        _parsedDocuments[uri] = parsed;
        return parsed;
    }

    public void Close(Uri uri)
    {
        _store.Close(uri);
        _parsedDocuments.Remove(uri);
    }

    public bool TryGetParsed(Uri uri, out ParsedDocument document) => _parsedDocuments.TryGetValue(uri, out document!);
}
