namespace ast_model.Symbols;

public interface IHierarchicalMap<TKey, TValue> where TKey : notnull
{
    HierarchicalMap<TKey, TValue> Parent { get; set; }

    HierarchicalMap<TKey, TValue> CreateChild();

    bool TryResolve(TKey v, out TValue result);
}
