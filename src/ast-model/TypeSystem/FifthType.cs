using Dunet;

namespace ast_model.TypeSystem;

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
