using ast_model.TypeSystem;
using System.Collections.Concurrent;

namespace compiler.TypeSystem;

/// <summary>
/// Cache for generic type instantiations to avoid redundant type construction.
/// Implements NFR-001: Limited to 10,000 entries with LRU eviction.
/// Task T029: GenericTypeCache with 10,000 entry limit and LRU eviction
/// </summary>
public class GenericTypeCache
{
    private const int MaxCacheSize = 10_000;
    
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly LinkedList<string> _lruList = new();
    private readonly object _lruLock = new();

    private class CacheEntry
    {
        public FifthType Type { get; set; }
        public LinkedListNode<string> LruNode { get; set; }
        public DateTime LastAccessed { get; set; }

        public CacheEntry(FifthType type, LinkedListNode<string> lruNode)
        {
            Type = type;
            LruNode = lruNode;
            LastAccessed = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets or creates a generic type instantiation.
    /// </summary>
    /// <param name="genericTypeDefinition">The generic type definition (e.g., "Stack")</param>
    /// <param name="typeArguments">The type arguments (e.g., [int])</param>
    /// <returns>The instantiated generic type</returns>
    public FifthType GetOrCreate(TypeName genericTypeDefinition, List<FifthType> typeArguments)
    {
        var key = CreateCacheKey(genericTypeDefinition, typeArguments);

        if (_cache.TryGetValue(key, out var entry))
        {
            // Update LRU tracking
            lock (_lruLock)
            {
                _lruList.Remove(entry.LruNode);
                _lruList.AddFirst(entry.LruNode);
                entry.LastAccessed = DateTime.UtcNow;
            }
            return entry.Type;
        }

        // Create new instantiation
        var instantiatedType = new FifthType.TGenericInstance(genericTypeDefinition, typeArguments)
        {
            Name = TypeName.From($"{genericTypeDefinition.Value}<{string.Join(", ", typeArguments.Select(t => t.Name.Value))}>")
        };

        // Add to cache with LRU tracking
        lock (_lruLock)
        {
            // Evict if cache is full
            if (_cache.Count >= MaxCacheSize)
            {
                EvictLeastRecentlyUsed();
            }

            var lruNode = _lruList.AddFirst(key);
            var cacheEntry = new CacheEntry(instantiatedType, lruNode);
            _cache[key] = cacheEntry;
        }

        return instantiatedType;
    }

    /// <summary>
    /// Creates a unique cache key for a generic type instantiation.
    /// </summary>
    private string CreateCacheKey(TypeName genericTypeDefinition, List<FifthType> typeArguments)
    {
        // Create a stable key based on type name and argument types
        var typeArgNames = string.Join(",", typeArguments.Select(t => GetTypeKey(t)));
        return $"{genericTypeDefinition.Value}<{typeArgNames}>";
    }

    /// <summary>
    /// Gets a stable key for a FifthType, handling nested generics recursively.
    /// </summary>
    private string GetTypeKey(FifthType type)
    {
        return type.Match(
            unknownType => "?",
            tVoidType => "void",
            tDotnetType => tDotnetType.TheType.FullName ?? tDotnetType.TheType.Name,
            tType => type.Name.Value,
            tFunc => $"({GetTypeKey(tFunc.InputType)})->{GetTypeKey(tFunc.OutputType)}",
            tArrayOf => $"{GetTypeKey(tArrayOf.ElementType)}[]",
            tListOf => $"[{GetTypeKey(tListOf.ElementType)}]",
            tGenericParameter => $"T:{tGenericParameter.ParameterName.Value}",
            tGenericInstance => 
            {
                var args = string.Join(",", tGenericInstance.TypeArguments.Select(GetTypeKey));
                return $"{tGenericInstance.GenericTypeDefinition.Value}<{args}>";
            }
        );
    }

    /// <summary>
    /// Evicts the least recently used entry from the cache.
    /// </summary>
    private void EvictLeastRecentlyUsed()
    {
        if (_lruList.Last != null)
        {
            var keyToRemove = _lruList.Last.Value;
            _lruList.RemoveLast();
            _cache.TryRemove(keyToRemove, out _);
        }
    }

    /// <summary>
    /// Gets the current cache statistics.
    /// </summary>
    public (int Count, int Capacity) GetStatistics()
    {
        return (_cache.Count, MaxCacheSize);
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    public void Clear()
    {
        lock (_lruLock)
        {
            _cache.Clear();
            _lruList.Clear();
        }
    }
}
