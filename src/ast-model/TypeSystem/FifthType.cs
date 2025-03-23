using Dunet;

namespace ast_model.TypeSystem;

/// <summary>
/// The core representation of a type in fifth, for the purposes of type checking. Every <see
/// cref="Expression"/>, and <see cref="UserDefinedType"/> instance must have a FifthType.
/// </summary>
/// <seealso cref="IEquatable&lt;FifthType&gt;"/>
//[Ignore]
//public record struct FifthType : IType
//{
//    public required TypeId Id { get; init; }
//    public required Symbol Symbol { get; init; }
//    public required FifthType[] ParentTypes { get; init; }
//    public required FifthType[] TypeArguments { get; init; }
//    public required bool IsArray { get; init; }

//    /// <summary>The default type to be assigned to all things that have no type (such as statements)</summary>
//    public static FifthType VoidType = new FifthType
//    {
//        Symbol = new() { Kind = SymbolKind.VoidSymbol, Name = null },
//        IsArray = false,
//        ParentTypes = [],
//        TypeArguments = [],
//        Id = TypeId.From(ushort.MaxValue)
//    };
//}

[ValueObject<uint>]
public partial struct TypeId;

[ValueObject<string>]
[Instance("unknown", "", "For cases where a typename is mandatory but the type is unknown")]
[Instance("anonymous", "", "For anonymous types")]
public partial struct TypeName;

[Union, Ignore]
public partial record FifthType
{
    public required TypeName Name { get; init; }

    /// <summary>
    /// When the type is unknown (before type checking and inference), but a FifthType is mandatory
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ast_model.TypeSystem.FifthType.UnknownType&gt;"/>
    partial record UnknownType();
    /// <summary>
    /// When the type is known to be nothing
    /// </summary>
    /// <seealso cref="ast_model.TypeSystem.FifthType"/>
    /// <seealso cref="System.IEquatable&lt;ast_model.TypeSystem.FifthType&gt;"/>
    /// <seealso cref="System.IEquatable&lt;ast_model.TypeSystem.FifthType.TVoidType&gt;"/>
    partial record TVoidType();
    /// <summary>
    /// Type of some reference to a .NET type
    /// </summary>
    partial record TDotnetType(System.Type TheType);
    /// <summary>
    /// A User Defined Type (just a plain old class)
    /// </summary>
    partial record TType();

    /// <summary>
    /// Type of a function from some sequence of types to some type
    /// </summary>
    partial record TFunc(FifthType InputType, FifthType OutputType);

    /// <summary>
    /// type of an array of things
    /// </summary>
    partial record TArrayOf(FifthType ElementType);

    /// <summary>
    /// type of a list of things
    /// </summary>
    partial record TListOf(FifthType ElementType);
}

[Union]
public partial record Maybe<T>
{
    partial record Some(T Value);
    partial record None();
}
