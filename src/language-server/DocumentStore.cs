using System.Collections.Concurrent;

namespace Fifth.LanguageServer;

public sealed class DocumentStore
{
    private readonly ConcurrentDictionary<Uri, string> _documents = new();

    public void Open(Uri uri, string text) => _documents[uri] = text;

    public void Close(Uri uri) => _documents.TryRemove(uri, out _);

    public void Update(Uri uri, string text) => _documents[uri] = text;

    public bool TryGet(Uri uri, out string text) => _documents.TryGetValue(uri, out text!);
}
