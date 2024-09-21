using System.Diagnostics;
using Fifth.Metamodel.AST2;

namespace ast_generated;

public abstract class BaseBuilder<T, TModel> : IBuilder<TModel>
    where T : IBuilder<TModel>, new()
{
    public TModel Model { get; set; }

    [DebuggerStepThrough]
    public static T Create(TModel model)
    {
        var builder = new T();
        builder.Model = model;
        return builder;
    }

    public static T Create()
    {
        return new T();
    }
    public TModel New()
    {
        return Model;
    }

    public static string Join<TItem>(IEnumerable<TItem> items, string sep, Func<TItem, string> renderer)
    {
        var renderedItems = from i in items
                            select renderer(i);
        return String.Join(sep, items);
    }
}
