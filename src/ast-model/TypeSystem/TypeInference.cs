using BaseType = ast_model.TypeSystem.FifthType;
using Type = ast_model.TypeSystem.FifthType;
using Arrow = ast_model.TypeSystem.FifthType.TFunc;

namespace ast_model.TypeSystem.Inference;

/// <summary>
/// Represents the type system interface.
/// </summary>
public interface ITypeSystem
{
    /// <summary>
    /// Gets the list of function arrows in the type system.
    /// </summary>
    List<Arrow> Arrows { get; }

    /// <summary>
    /// Gets the list of types in the type system.
    /// </summary>
    List<Type> Types { get; }

    /// <summary>
    /// Infers the result type based on the input types and operator.
    /// </summary>
    /// <param name="inputTypes">The input types.</param>
    /// <param name="@operator">The operator.</param>
    /// <returns>The inferred result type.</returns>
    BaseType InferResultType(BaseType[] inputTypes, string @operator);

    /// <summary>
    /// Adds a function to the type system.
    /// </summary>
    /// <param name="inputTypes">The input types.</param>
    /// <param name="outputType">The output type.</param>
    /// <param name="op">The operator.</param>
    /// <returns>The updated type system.</returns>
    TypeSystem WithFunction(BaseType[] inputTypes, BaseType outputType, string op);

    /// <summary>
    /// Adds an operation to the type system.
    /// </summary>
    /// <param name="tInLHS">The left-hand side input type.</param>
    /// <param name="tInRHS">The right-hand side input type.</param>
    /// <param name="tOut">The output type.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The updated type system.</returns>
    TypeSystem WithOperation(BaseType tInLHS, BaseType tInRHS, BaseType tOut, string operation);

    /// <summary>
    /// Adds a type to the type system.
    /// </summary>
    /// <param name="t">The type to add.</param>
    /// <returns>The updated type system.</returns>
    TypeSystem WithType(Type t);
}

/// <summary>
/// Represents the type system.
/// </summary>
public class TypeSystem : ITypeSystem
{
    /// <summary>
    /// Represents the void type.
    /// </summary>
    public static readonly BaseType Void = new FifthType.TVoidType() { Name = TypeName.From("void") };

    /// <summary>
    /// Gets the list of function arrows in the type system.
    /// </summary>
    public List<Arrow> Arrows { get; } = [];

    /// <summary>
    /// Gets the list of types in the type system.
    /// </summary>
    public List<Type> Types { get; } = [];

    /// <summary>
    /// Creates a new function arrow.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="tin">The input type.</param>
    /// <param name="tout">The output type.</param>
    /// <returns>The created function arrow.</returns>
    public static Arrow newArrow(string op, BaseType tin, BaseType tout)
        => new([tin], tout) { Name = TypeName.From(op) };

    public static Arrow newArrow(string op, BaseType[] tins, BaseType tout)
        => new(tins.ToList(), tout) { Name = TypeName.From(op) };

    /// <summary>
    /// Builds a function arrow based on the input types, output type, and operator.
    /// </summary>
    /// <param name="inputTypes">The input types.</param>
    /// <param name="outputType">The output type.</param>
    /// <param name="op">The operator.</param>
    /// <returns>The built function arrow.</returns>
    public Arrow Build(BaseType[] inputTypes, BaseType outputType, string op)
    {
        if (inputTypes.Length == 0)
        {
            return newArrow(op, [Void], outputType);
        }

        return newArrow(op, inputTypes, outputType);
    }

    /// <summary>
    /// Infers the result type based on the input types and operator.
    /// </summary>
    /// <param name="inputTypes">The input types.</param>
    /// <param name="@operator">The operator.</param>
    /// <returns>The inferred result type.</returns>
    public BaseType InferResultType(BaseType[] inputTypes, string @operator)
    {
        return InferResultType(inputTypes, @operator, Arrows);
    }

    /// <summary>
    /// Adds a function to the type system.
    /// </summary>
    /// <param name="inputTypes">The input types.</param>
    /// <param name="outputType">The output type.</param>
    /// <param name="op">The operator.</param>
    /// <returns>The updated type system.</returns>
    public TypeSystem WithFunction(BaseType[] inputTypes, BaseType outputType, string op)
    {
        Arrows.Add(Build(inputTypes, outputType, op));
        return this;
    }

    /// <summary>
    /// Adds an operation to the type system.
    /// </summary>
    /// <param name="tInLHS">The left-hand side input type.</param>
    /// <param name="tInRHS">The right-hand side input type.</param>
    /// <param name="tOut">The output type.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The updated type system.</returns>
    public TypeSystem WithOperation(BaseType tInLHS, BaseType tInRHS, BaseType tOut, string operation)
    {
        Arrows.Add(newArrow(operation, [tInLHS, tInRHS], tOut));
        return this;
    }

    /// <summary>
    /// Adds a type to the type system.
    /// </summary>
    /// <param name="t">The type to add.</param>
    /// <returns>The updated type system.</returns>
    public TypeSystem WithType(Type t)
    {
        Types.Add(t);
        return this;
    }

    /// <summary>
    /// Infers the result type based on the input types, operator, and a list of arrows.
    /// </summary>
    /// <param name="inputTypes">The input types.</param>
    /// <param name="@operator">The operator.</param>
    /// <param name="arrows">The list of arrows.</param>
    /// <returns>The inferred result type.</returns>
    private BaseType InferResultType(BaseType[] inputTypes, string @operator, List<Arrow> arrows)
    {
        if (inputTypes.Length == 0)
        {
            return InferResultType([Void], @operator, arrows);
        }

        var matches = from a in arrows
                      where @operator == a.Name
                      && a.InputTypes.SequenceEqual(inputTypes)
                      select a;

        return matches.Select(a => a.OutputType).FirstOrDefault();
    }
}

