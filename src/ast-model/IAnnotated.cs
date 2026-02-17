using ast_model;

namespace ast;

public interface IAnnotated
{
    [Ignore]
    object this[string index] { get; set; }

    bool HasAnnotation(string key);

    bool TryGetAnnotation<T>(string name, out T result) where T : class;
}
