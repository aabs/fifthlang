namespace compiler.NamespaceResolution;

/// <summary>
/// Directed graph capturing namespace import relationships.
/// Detects cycles and provides memoized traversal to prevent infinite recursion during symbol resolution.
/// </summary>
public class NamespaceImportGraph
{
    /// <summary>
    /// Map from namespace name to the set of namespaces it imports
    /// </summary>
    private readonly Dictionary<string, HashSet<string>> _edges = new();
    
    /// <summary>
    /// Set of namespaces that have been visited during the current traversal.
    /// Used to short-circuit cyclic imports.
    /// </summary>
    private readonly HashSet<string> _visited = new();
    
    /// <summary>
    /// Add an edge representing that fromNamespace imports toNamespace
    /// </summary>
    public void AddEdge(string fromNamespace, string toNamespace)
    {
        if (!_edges.ContainsKey(fromNamespace))
        {
            _edges[fromNamespace] = new HashSet<string>();
        }
        _edges[fromNamespace].Add(toNamespace);
    }
    
    /// <summary>
    /// Get all namespaces directly imported by the given namespace
    /// </summary>
    public IEnumerable<string> GetDirectImports(string namespaceName)
    {
        return _edges.TryGetValue(namespaceName, out var imports) ? imports : Enumerable.Empty<string>();
    }
    
    /// <summary>
    /// Traverse the import graph starting from a namespace, collecting all transitively imported namespaces.
    /// Short-circuits when encountering cycles to prevent infinite recursion.
    /// Returns namespaces in dependency order (dependencies before dependents).
    /// </summary>
    public List<string> TraverseImports(string startNamespace)
    {
        _visited.Clear();
        var result = new List<string>();
        TraverseRecursive(startNamespace, result);
        return result;
    }
    
    private void TraverseRecursive(string namespaceName, List<string> result)
    {
        // Short-circuit if already visited (handles cycles and memoization)
        if (_visited.Contains(namespaceName))
        {
            return;
        }
        
        _visited.Add(namespaceName);
        
        // Recursively traverse imports first (depth-first)
        if (_edges.TryGetValue(namespaceName, out var imports))
        {
            foreach (var importedNamespace in imports)
            {
                TraverseRecursive(importedNamespace, result);
            }
        }
        
        // Add this namespace after its imports (dependency order)
        if (!result.Contains(namespaceName))
        {
            result.Add(namespaceName);
        }
    }
    
    /// <summary>
    /// Detect if there is a cycle involving the given namespace.
    /// Returns true if the namespace is part of a cycle.
    /// </summary>
    public bool HasCycle(string namespaceName)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        return HasCycleRecursive(namespaceName, visited, recursionStack);
    }
    
    private bool HasCycleRecursive(string current, HashSet<string> visited, HashSet<string> recursionStack)
    {
        if (recursionStack.Contains(current))
        {
            return true;  // Found a cycle
        }
        
        if (visited.Contains(current))
        {
            return false;  // Already fully explored this path
        }
        
        visited.Add(current);
        recursionStack.Add(current);
        
        if (_edges.TryGetValue(current, out var imports))
        {
            foreach (var importedNamespace in imports)
            {
                if (HasCycleRecursive(importedNamespace, visited, recursionStack))
                {
                    return true;
                }
            }
        }
        
        recursionStack.Remove(current);
        return false;
    }
}
