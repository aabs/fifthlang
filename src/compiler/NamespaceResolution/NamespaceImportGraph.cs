namespace compiler.NamespaceResolution;

public sealed class NamespaceImportGraph
{
    private readonly Dictionary<string, HashSet<string>> _edges = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IReadOnlyList<string>> _traversalCache = new(StringComparer.Ordinal);

    public void AddImport(string fromNamespace, string toNamespace)
    {
        if (string.IsNullOrWhiteSpace(fromNamespace) || string.IsNullOrWhiteSpace(toNamespace))
        {
            return;
        }

        if (!_edges.TryGetValue(fromNamespace, out var targets))
        {
            targets = new HashSet<string>(StringComparer.Ordinal);
            _edges[fromNamespace] = targets;
        }

        if (targets.Add(toNamespace))
        {
            _traversalCache.Remove(fromNamespace);
        }
    }

    public IReadOnlyList<string> TraverseImports(string rootNamespace)
    {
        if (string.IsNullOrWhiteSpace(rootNamespace))
        {
            return Array.Empty<string>();
        }

        if (_traversalCache.TryGetValue(rootNamespace, out var cached))
        {
            return cached;
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();
        Traverse(rootNamespace, visited, result);

        _traversalCache[rootNamespace] = result;
        return result;
    }

    private void Traverse(string current, HashSet<string> visited, List<string> result)
    {
        if (!_edges.TryGetValue(current, out var targets))
        {
            return;
        }

        foreach (var target in targets)
        {
            if (!visited.Add(target))
            {
                continue;
            }

            result.Add(target);
            Traverse(target, visited, result);
        }
    }
}
