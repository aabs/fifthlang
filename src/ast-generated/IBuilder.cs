namespace Fifth.Metamodel.AST2;

using System.IO;

public interface IBuilder<TModel>
{
    TModel Model { get; set; }

    TModel New();
}
