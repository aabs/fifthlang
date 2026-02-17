using Dunet;

namespace ast_model.TypeSystem;

[Union]
public partial record Maybe<T>
{
    partial record Some(T Value);
    partial record None();
}
