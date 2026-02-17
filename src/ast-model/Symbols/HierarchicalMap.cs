namespace ast_model.Symbols;

public class HierarchicalMap<TKey, TValue> : Dictionary<TKey, TValue>, IHierarchicalMap<TKey, TValue> where TKey : notnull
{
    public HierarchicalMap<TKey, TValue> Parent { get; set; }

    public HierarchicalMap<TKey, TValue> CreateChild()
    {
        return new HierarchicalMap<TKey, TValue> { Parent = this };
    }

    public bool TryResolve(TKey v, out TValue result)
    {
        if (TryGetValue(v, out result))
        {
            return true;
        }
        return Parent?.TryResolve(v, out result) ?? false;
    }
}
