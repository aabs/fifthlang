// ReSharper disable UnusedMember.Global

/// <summary>The ast namespace is the namespace where items that constitute part of the main abstract syntax tree (AST) are defined</summary>
/// <remarks>
///   <para>
/// Some Definitions/Clarifications:
/// </para>
///   <list type="number">
///     <item>
///       <strong>Parameter</strong>: A parameter is a variable in the declaration of a function or method.</item>
///     <item>
///       <strong>Argument</strong>: An argument is the actual value that is passed to the function when it is called.</item>
///   </list>
/// </remarks>
namespace ast;

using ast_model;
using ast_model.Symbols;
using ast_model.TypeSystem;
using Vogen;
using static ast_model.TypeSystem.Maybe<string>;

//using static ast_model.TypeSystem.Maybe<string>;
#region Core Abstractions


/// <summary>
/// Constraints on what can be done with the thing so adorned.
/// </summary>
public enum AccessConstraint
{
    None,
    ReadOnly,
    WriteOnly,
    ReadWrite
}

public enum CollectionType
{
    SingleInstance,
    Array,
    List
}

public enum Operator : ushort
{
    // Unary Operators
    ArithmeticNegative, // as opposed to Subtract, which is different

    LogicalNot, // bitwise complement...

    // ArithmeticOperator
    ArithmeticAdd,

    ArithmeticSubtract,
    ArithmeticMultiply,
    ArithmeticDivide,
    ArithmeticRem,
    ArithmeticMod,
    ArithmeticPow,

    // LogicalOperators
    BitwiseAnd,

    BitwiseOr,
    LogicalAnd,
    LogicalOr,
    LogicalNand,
    LogicalNor,
    LogicalXor,
    TernaryCondition, // ?:

    // RelationalOperators
    Equal,

    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,

    BitwiseLeftShift,
    BitwiseRightShift,

    StringConcatenate,
    Concatenate,
}

public enum OperatorPosition
{
    Prefix, Infix, Postfix
}

public enum SymbolKind
{
    VoidSymbol, // as in no symbol known
    Assembly,
    AssemblyRef,
    AssertionStatement,
    AssignmentStatement,
    Atom,
    BinaryExp,
    BlockStatement,
    CastExp,
    CatchClause,
    ClassDef,
    EmptyStatement,
    Expression,
    ExpStatement,
    FieldDef,
    ForeachStatement,
    ForStatement,
    FuncCallExp,
    FunctionDef,
    Graph,
    GraphNamespaceAlias,
    GuardStatement,
    IfElseStatement,
    InferenceRule,
    InferenceRuleDef,
    KnowledgeManagementStatement,
    LambdaDef,
    List,
    LiteralExp,
    MemberAccessExp,
    MemberDef,
    MemberRef,
    MethodDef,
    ObjectInstantiationExp,
    ParamDef,
    ParamDestructureDef,
    PropertyBindingDef,
    PropertyDef,
    RetractionStatement,
    ReturnStatement,
    StructDef,
    ThrowExp,
    ThrowStatement,
    Triple,
    TryStatement,
    TypeDef,
    TypeRef,
    TypeParameter,  // Added for generic type parameters
    TriGLiteralExpression,
    SparqlLiteralExpression,
    VariableBinding,
    Interpolation,
    UnaryExp,
    VarDeclStatement,
    VarRef,
    VarRefExp,
    WhileStatement,
    WithScopeStatement
}

/// <summary>
///     Visibility of a member.
/// </summary>
public enum Visibility
{
    /// <summary>
    ///     Not visible outside of this assembly.
    /// </summary>
    Internal,

    /// <summary>
    ///     Visible outside of this assembly.
    /// </summary>
    Public,

    /// <summary>
    ///     Visible only within this type.
    /// </summary>
    Private,

    /// <summary>
    ///     Visible only within this type and subtypes.
    /// </summary>
    Protected,

    /// <summary>
    ///     Visible only within the declaring type.
    /// </summary>
    ProtectedInternal,
}

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For things like in-memory assemblies etc")]
public partial struct AssemblyName;

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For anonymous members")]
public partial struct MemberName;

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For anonymous namespaces")]
public partial struct NamespaceName;

[Ignore, ValueObject<string>]
public partial struct TypeParameterName;

[Ignore, ValueObject<string>]
public partial struct OperatorId;

[Ignore, ValueObject<ushort>]
[Instance("byte", 0, "byte has lowest seniority")]
[Instance("short", 1)]
[Instance("integer", 2)]
[Instance("long", 3)]
[Instance("float", 4)]
[Instance("double", 5)]
[Instance("decimal", 6, "decimal has highest seniority")]
public partial struct TypeCoertionSeniority;

public record struct Symbol(string Name, SymbolKind Kind);

public record struct SourceLocationMetadata(int Column, string Filename, int Line, string OriginalText);

public abstract record AstThing : AnnotatedThing, IAstThing
{
    public virtual void Accept(IVisitor visitor)
    {
        visitor.OnEnter(this);
        visitor.OnLeave(this);
    }
    public FifthType Type { get; set; }
    public IAstThing? Parent { get; set; }
    public SourceLocationMetadata? Location { get; set; }
}

public abstract record ScopeAstThing : AstThing, IScope
{
    private ISymbolTable _symbolTable;

    [IgnoreDuringVisit]
    public IScope EnclosingScope { get; init; }
    [IgnoreDuringVisit]
    public ISymbolTable SymbolTable
    {
        get => _symbolTable ??= new SymbolTable();
        init => _symbolTable = value;
    }

    public void Declare(Symbol symbol, IAstThing astThing, Dictionary<string, object> annotations)
    {
        annotations ??= new Dictionary<string, object>();
        var qualifiedName = annotations.TryGetValue("QualifiedName", out var qnValue) ? qnValue as string : null;
        var isImported = annotations.TryGetValue("IsImported", out var importedValue)
            && importedValue is bool imported && imported;
        var isLocalShadow = annotations.TryGetValue("IsLocalShadow", out var shadowValue)
            && shadowValue is bool shadow && shadow;

        var symTabEntry = new SymbolTableEntry
        {
            Symbol = symbol,
            Annotations = annotations,
            OriginatingAstThing = astThing,
            QualifiedName = qualifiedName,
            IsImported = isImported,
            IsLocalShadow = isLocalShadow
        };
        SymbolTable[symbol] = symTabEntry;
    }

    public bool TryResolve(Symbol symbol, out ISymbolTableEntry result)
    {
        result = null;

        if (SymbolTable.TryGetValue(symbol, out result))
        {
            return true;
        }

        return this.Parent.NearestScope()?.TryResolve(symbol, out result) ?? false;
    }

    public bool TryResolveByName(string symbolName, out ISymbolTableEntry result)
    {
        result = null;
        var tmp = SymbolTable.ResolveByName(symbolName);
        if (tmp != null)
        {
            result = tmp;
            return true;
        }

        return this.Parent.NearestScope()?.TryResolveByName(symbolName, out result) ?? false;
    }

    public ISymbolTableEntry Resolve(Symbol symbol)
    {
        if (TryResolve(symbol, out var ste))
        {
            return ste;
        }

        throw new CompilationException($"Unable to resolve symbol {symbol.Name}");
    }

    public ISymbolTableEntry ResolveByName(string symbolName)
    {
        if (TryResolveByName(symbolName, out var ste))
        {
            return ste;
        }

        throw new CompilationException($"Unable to resolve symbol {symbolName}");
    }
}

#endregion Core Abstractions

#region Definitions

public abstract record Definition : AstThing
{
    public required Visibility Visibility { get; init; }
}

public abstract record ScopedDefinition : ScopeAstThing
{
    public required Visibility Visibility { get; init; }
}

public record AssemblyDef : ScopedDefinition
{
    public required AssemblyName Name { get; init; }
    public required string PublicKeyToken { get; init; }
    public required string Version { get; init; }
    public required List<AssemblyRef> AssemblyRefs { get; init; } = [];
    public required List<ModuleDef> Modules { get; init; } = [];
    public required string TestProperty { get; init; } = ""; // Test property added for code generation demo
}

public record ModuleDef : ScopedDefinition
{
    public required string OriginalModuleName { get; init; }
    public required NamespaceName NamespaceDecl { get; init; }
    public required List<ClassDef> Classes { get; init; } = [];
    public required List<ScopedDefinition> Functions { get; init; } = [];
}

/// <summary>
/// Common interface for functions that can participate in overloading (both free functions and methods)
/// </summary>
public interface IOverloadableFunction
{
    MemberName Name { get; }
    List<ParamDef> Params { get; }
    BlockStatement Body { get; }
    FifthType ReturnType { get; }
}

/// <summary>
/// Represents a type parameter definition in a generic class or function (e.g., T in class Stack&lt;T&gt;)
/// </summary>
public record TypeParameterDef : Definition
{
    public required TypeParameterName Name { get; init; }
    public required List<TypeConstraint> Constraints { get; init; } = [];
}

/// <summary>
/// Base class for type parameter constraints (where T: IComparable, where T: new(), etc.)
/// </summary>
public abstract record TypeConstraint : AstThing;

/// <summary>
/// Interface constraint: requires type parameter to implement an interface (where T: IComparable)
/// </summary>
public record InterfaceConstraint : TypeConstraint
{
    public required TypeName InterfaceName { get; init; }
}

/// <summary>
/// Base class constraint: requires type parameter to derive from a base class (where T: BaseClass)
/// </summary>
public record BaseClassConstraint : TypeConstraint
{
    public required TypeName BaseClassName { get; init; }
}

/// <summary>
/// Constructor constraint: requires type parameter to have a parameterless constructor (where T: new())
/// </summary>
public record ConstructorConstraint : TypeConstraint;

/// <summary>
/// A bare function is a member of a singleton Global type.  A member, in other words.
/// </summary>
public record FunctionDef : ScopedDefinition, IOverloadableFunction
{
    public required List<TypeParameterDef> TypeParameters { get; set; } = [];
    public required List<ParamDef> Params { get; set; } = [];
    public required BlockStatement Body { get; set; }
    public required FifthType ReturnType { get; init; }
    public required MemberName Name { get; init; }
    public required bool IsStatic { get; init; }
    public required bool IsConstructor { get; init; }
    public BaseConstructorCall? BaseCall { get; set; }
}

/// <summary>
/// A scoped function object that can act as the result of a lambda expression.
/// </summary>
/// <remarks>
/// <para>
/// This is modeled on the representation for lambda expressions used by C++. A lambda expression is
/// a function definition, plus a closure that can capture the values of a set of in-scope variables
/// at the point of instantiation.
/// </para>
/// <para>
/// The functor is just a class instance, but when it's value is needed it can be invoked through
/// the use of an operator() function to get the value.
/// </para>
/// </remarks>
/// <seealso cref="ast.ScopeAstThing"/>
/// <seealso cref="ast.IAnnotated"/>
/// <seealso cref="System.IEquatable%3Cast.AnnotatedThing%3E">System.IEquatable&lt;ast.AnnotatedThing&gt;</seealso>
/// <seealso cref="ast.IAstThing"/>
/// <seealso cref="ast_model.IVisitable"/>
/// <seealso cref="System.IEquatable%3Cast.AstThing%3E">System.IEquatable&lt;ast.AstThing&gt;</seealso>
/// <seealso cref="ast_model.Symbols.IScope"/>
/// <seealso cref="System.IEquatable%3Cast.ScopeAstThing%3E">System.IEquatable&lt;ast.ScopeAstThing&gt;</seealso>
/// <seealso cref="System.IEquatable%3Cast.FunctorDef%3E">System.IEquatable&lt;ast.FunctorDef&gt;</seealso>
public record FunctorDef : ScopeAstThing
{
    /// <summary>
    /// The function to be invoked as the lambda. This will be an operator() function on the
    /// instance object.
    /// </summary>
    /// <value>The invocation function dev.</value>
    public FunctionDef InvocationFuncDev { get; init; }
}

public abstract record MemberDef : Definition
{
    public required MemberName Name { get; init; }
    public required TypeName TypeName { get; init; }
    public required CollectionType CollectionType { get; init; }
    public required bool IsReadOnly { get; set; }
}

public record FieldDef : MemberDef
{
    public required AccessConstraint[] AccessConstraints { get; set; } = [];
}

public record PropertyDef : MemberDef
{
    public required AccessConstraint[] AccessConstraints { get; set; } = [];
    public required bool IsWriteOnly { get; set; }
    public required FieldDef? BackingField { get; set; }
    public required MethodDef? Getter { get; set; }
    public required MethodDef? Setter { get; set; }
    public required bool CtorOnlySetter { get; set; }
}
public record MethodDef : MemberDef, IOverloadableFunction
{
    // todo: need the possibility of type parameters here.
    public FunctionDef FunctionDef { get; set; }

    // IOverloadableFunction implementation - delegate to wrapped FunctionDef
    List<ParamDef> IOverloadableFunction.Params => FunctionDef.Params;
    BlockStatement IOverloadableFunction.Body => FunctionDef.Body;
    FifthType IOverloadableFunction.ReturnType => FunctionDef.ReturnType;
}

public record OverloadedFunctionDefinition : MemberDef
{
    public List<IOverloadableFunction> OverloadClauses { get; init; } = [];
    public IFunctionSignature Signature { get; init; }
}

/// <summary>
/// A module-level overloaded function that can contain multiple function clauses with guard conditions
/// </summary>
public record OverloadedFunctionDef : ScopedDefinition
{
    public List<IOverloadableFunction> OverloadClauses { get; init; } = [];
    public IFunctionSignature Signature { get; init; }

    // Properties to make this compatible with FunctionDef for ModuleDef.Functions
    public required List<ParamDef> Params { get; set; } = [];
    public required BlockStatement Body { get; set; }
    public required FifthType ReturnType { get; init; }
    public required MemberName Name { get; init; }
    public required bool IsStatic { get; init; }
    public required bool IsConstructor { get; init; }
}

/// <summary>
/// The definition of an inference rule for knowledge management.
/// </summary>
/// <example>
/// <code>
///  when (calculate_bmi(p:Person) > 30 && p.age > 18)
///  {
///  is_a(p, :ObeseAdult);
///  needs(p, :DietaryAdvice);
///  } offer_health_advice_to_overweight_adults;
/// </code>
/// </example>
/// <remarks>
/// <see cref="obsidian://open?vault=notes&file=me%2Factive%2Fprojects%2Ffifthlang%2FLanguage%20Samples"/>
/// </remarks>
public record InferenceRuleDef : Definition
{
    public required Expression Antecedent { get; set; }

    public required KnowledgeManagementBlock Consequent { get; set; }
}

public record ParamDef : Definition
{
    public required TypeName TypeName { get; init; }
    public required CollectionType CollectionType { get; init; }
    public required string Name { get; init; }
    public required Expression? ParameterConstraint { get; set; }
    public required ParamDestructureDef? DestructureDef { get; set; }
}

public record ParamDestructureDef : Definition
{
    public required List<PropertyBindingDef> Bindings { get; set; } = [];
}

public record PropertyBindingDef : Definition
{
    public required MemberName IntroducedVariable { get; set; }
    public required MemberName ReferencedPropertyName { get; set; }
    public PropertyDef? ReferencedProperty { get; set; }
    public required ParamDestructureDef? DestructureDef { get; set; }
    public Expression? Constraint { get; set; }
}

public record TypeDef : Definition
{
}

public record ClassDef : ScopedDefinition
{
    public required TypeName Name { get; init; }
    public required List<TypeParameterDef> TypeParameters { get; set; } = [];
    public required List<MemberDef> MemberDefs { get; set; } = [];
    public required List<string> BaseClasses { get; set; } = [];
    public required string? AliasScope { get; set; } = default;
}

// out of scope for now...
//public record StructDef : Definition
//{
//    public required List<MemberDef> MemberDefs { get; set; }
//}

public record VariableDecl : Definition
{
    public required string Name { get; init; }
    public required TypeName TypeName { get; init; }
    public required CollectionType CollectionType { get; init; }
}

#endregion Definitions

#region References

public abstract record Reference : AstThing
{
}

public record AssemblyRef : Reference
{
    public required string PublicKeyToken { get; set; }
    public required string Version { get; set; }
}

public record MemberRef : Reference
{
    public required MemberDef Member { get; set; }
}

public record PropertyRef : Reference
{
    public required PropertyDef Property { get; set; }
}

public record TypeRef : Reference
{
}

public record VarRef : Reference
{
    public required MemberName ReferencedVariableName { get; set; }
}

public record GraphNamespaceAlias : AstThing // TODO: is this a reference or something similar?
{
    public required Uri Uri { get; set; }
}

#endregion References

#region Statements

public abstract record Statement : AstThing
{
}

public record AssignmentStatement : Statement
{
    public required Expression LValue { get; init; }
    public required Expression RValue { get; set; }
}

public record BlockStatement : Statement
{
    public required List<Statement> Statements { get; set; } = [];
}
public record KnowledgeManagementBlock : Statement
{
    public required List<KnowledgeManagementStatement> Statements { get; set; } = [];
}

/// <summary>
/// A statement containing a bare expression where the result is discarded
/// </summary>
public record ExpStatement : Statement
{
    public required Expression RHS { get; set; }
}

/// <summary>
/// An empty or no-op statement (e.g., a bare semicolon)
/// </summary>
public record EmptyStatement : Statement
{
}

public record ForStatement : Statement
{
    public required Expression InitialValue { get; set; }
    public required Expression Constraint { get; set; }
    public required Expression IncrementExpression { get; set; }
    public required VariableDecl LoopVariable { get; set; }
    public required BlockStatement Body { get; set; }
}

public record ForeachStatement : Statement
{
    public required Expression Collection { get; set; }
    public required VariableDecl LoopVariable { get; set; }
    public required BlockStatement Body { get; set; }
}

// TODO: work out what I meant by this
// see here: obsidian://open?vault=notes&file=me%2Factive%2Fprojects%2Ffifthlang%2Fprojects.fifthlang.ast.guardstmt
public record GuardStatement : Statement
{
    public required Expression Condition { get; set; }
}

public record IfElseStatement : Statement
{
    public required Expression Condition { get; set; }
    public required BlockStatement ThenBlock { get; set; }
    public required BlockStatement ElseBlock { get; set; }
}

public record ReturnStatement : Statement
{
    public required Expression ReturnValue { get; set; }
}

public record VarDeclStatement : Statement
{
    public required VariableDecl VariableDecl { get; set; }
    public required Expression? InitialValue { get; set; }
}

public record WhileStatement : Statement
{
    public required Expression Condition { get; set; }
    public required BlockStatement Body { get; set; }
}

/// <summary>
/// Try statement with optional catch clauses and optional finally block.
/// Represents C#-style exception handling in Fifth.
/// </summary>
public record TryStatement : Statement
{
    public required BlockStatement TryBlock { get; set; }
    public required List<CatchClause> CatchClauses { get; set; } = [];
    public BlockStatement? FinallyBlock { get; set; }
}

/// <summary>
/// A single catch clause within a try statement.
/// Supports typed catches, exception identifiers, and optional filter expressions.
/// </summary>
public record CatchClause : AstThing
{
    public FifthType? ExceptionType { get; set; }
    public string? ExceptionIdentifier { get; set; }
    public Expression? Filter { get; set; }
    public required BlockStatement Body { get; set; }
}

/// <summary>
/// Throw statement for throwing exceptions.
/// Supports both explicit throw (with exception expression) and rethrow (without expression).
/// </summary>
public record ThrowStatement : Statement
{
    public Expression? Exception { get; set; }
}

public abstract record KnowledgeManagementStatement : Statement
{
}

/// <summary>
/// Asserts some statement to be true, for addition to the knowledge base
/// </summary>
public record AssertionStatement : Statement
{
    public required TripleLiteralExp Assertion { get; set; }

    // the use of a triple is one approach.  The division into separate objects, below, may be more powerful

    public required AssertionSubject AssertionSubject { get; set; }
    public required AssertionPredicate AssertionPredicate { get; set; }
    public required AssertionObject AssertionObject { get; set; }
}

/// <summary>
/// The object of an assertion, the thing about which the assertion is made.
/// </summary>
/// <remarks>
/// possible objects include:
/// - A class definition found in the code space as well as the knowledge base
/// - A class definition only mentioned in the knowledge base
/// - an instance of some type of class
/// </remarks>
public record AssertionObject : Expression
{
    //todo: need to work out how to represent the choices available to this (perhaps as some sort of discriminated union or polymorphic type)
}

/// <summary>
/// The predicate of an assertion, the thing that is asserted about the subject.
/// </summary>
public record AssertionPredicate : Expression
{
}

/// <summary>
/// The subject of an assertion, the thing said about the object.
/// </summary>
public record AssertionSubject : Expression
{
}

public record RetractionStatement : Statement
{
}

public record WithScopeStatement : Statement
{
}

#endregion Statements

#region Expressions

public abstract record Expression : AstThing
{
}

public record BinaryExp : Expression
{
    public required Expression LHS { get; set; }
    public required Operator Operator { get; set; }
    public required Expression RHS { get; set; }
}

public record CastExp : Expression
{
    public required FifthType TargetType { get; init; }
}

/// <summary>
/// A representation of an anonymous function definition to be passed as an expression for future invocation.
/// </summary>
/// <remarks>
/// The expression returns an instance of a functor that can be turned into an expression on demand.
/// The functor instance is the first class function that can be passed around.
/// </remarks>
public record LambdaExp : Expression
{
    /// <summary>
    /// The definition for the functor instance that will be passed the expression.
    /// </summary>
    public FunctorDef FunctorDef { get; set; }
}

public record FuncCallExp : Expression
{
    /// <summary>
    /// Reference to the resolved function definition. Marked [IgnoreDuringVisit] to prevent infinite
    /// recursion when visiting recursive functions (e.g., fibonacci calls itself multiple times).
    /// </summary>
    [IgnoreDuringVisit]
    public FunctionDef FunctionDef { get; set; }
    public List<Expression> InvocationArguments { get; set; }
    /// <summary>
    /// Explicit type arguments for generic function calls (e.g., identity<int>(x))
    /// Empty list if no type arguments provided (will be inferred)
    /// </summary>
    public List<FifthType> TypeArguments { get; set; } = [];
}

/// <summary>
/// Represents a base constructor invocation (: base(...)) in a derived class constructor
/// </summary>
public record BaseConstructorCall : AstThing
{
    public required List<Expression> Arguments { get; set; } = [];
    /// <summary>
    /// Reference to the resolved constructor. Marked [IgnoreDuringVisit] to prevent infinite
    /// recursion when visiting constructor chains.
    /// </summary>
    [IgnoreDuringVisit]
    public FunctionDef? ResolvedConstructor { get; set; }
}

[Ignore]
public abstract record LiteralExpression<T> : Expression
{
    [IgnoreDuringVisit] public T Value { get; set; }
}

// see https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types?devlangs=csharp&f1url=%3FappId%3DDev17IDEF1%26l%3DEN-US%26k%3Dk(ushort_CSharpKeyword)%3Bk(DevLang-csharp)%26rd%3Dtrue
// for the base types and their ranges

// numeric types
public record Int8LiteralExp : LiteralExpression<sbyte>;
public record Int16LiteralExp : LiteralExpression<short>;
public record Int32LiteralExp : LiteralExpression<int>;
public record Int64LiteralExp : LiteralExpression<long>;
public record UnsignedInt8LiteralExp : LiteralExpression<byte>;
public record UnsignedInt16LiteralExp : LiteralExpression<ushort>;
public record UnsignedInt32LiteralExp : LiteralExpression<uint>;
public record UnsignedInt64LiteralExp : LiteralExpression<ulong>;
public record Float4LiteralExp : LiteralExpression<float>;
public record Float8LiteralExp : LiteralExpression<double>;
public record Float16LiteralExp : LiteralExpression<decimal>;

public record BooleanLiteralExp : LiteralExpression<bool>;
public record CharLiteralExp : LiteralExpression<char>;
public record StringLiteralExp : LiteralExpression<string>;
public record DateLiteralExp : LiteralExpression<DateOnly>;
public record TimeLiteralExp : LiteralExpression<TimeOnly>;
public record DateTimeLiteralExp : LiteralExpression<DateTimeOffset>;
public record DurationLiteralExp : LiteralExpression<TimeSpan>;
public record UriLiteralExp : LiteralExpression<Uri>;

/// <summary>
/// An atom in the sense of a Prolog atom.
/// </summary>
public record AtomLiteralExp : LiteralExpression<string>;

/// <summary>
/// Represents a TriG literal expression delimited by @&lt; ... &gt;.
/// The literal contains TriG/RDF dataset content with optional expression interpolations.
/// Evaluates to a Store value at runtime.
/// </summary>
/// <remarks>
/// User Story 1: Basic TriG literal without interpolation
/// User Story 2: Expression interpolation using {{ expression }} syntax
/// Interpolations are evaluated at runtime in the surrounding lexical scope.
/// </remarks>
public record TriGLiteralExpression : Expression
{
    /// <summary>
    /// The raw TriG content as a string, including any interpolation placeholders.
    /// Whitespace and newlines are preserved as-is.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// List of interpolated expressions found in the literal.
    /// Each entry maps a placeholder position to the expression to be evaluated.
    /// </summary>
    public List<InterpolatedExpression> Interpolations { get; set; } = new();
}

/// <summary>
/// Represents an expression interpolated into a TriG literal.
/// </summary>
public record InterpolatedExpression : AstThing
{
    /// <summary>
    /// The expression to be evaluated and serialized into the TriG content.
    /// </summary>
    public required Expression Expression { get; set; }

    /// <summary>
    /// Position in the Content string where this interpolation starts.
    /// Used for diagnostic reporting.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Length of the interpolation placeholder in the original source.
    /// </summary>
    public int Length { get; set; }
}

/// <summary>
/// Represents a SPARQL query literal expression: ?&lt; ... &gt;
/// Example: q: Query = ?&lt;SELECT * WHERE { ?s ?p ?o }>;
/// </summary>
/// <remarks>
/// Supports three user stories:
/// 1. Basic literal parsing - compile SPARQL queries as literal values
/// 2. Variable binding - reference Fifth variables directly in SPARQL (safe parameterization)
/// 3. Interpolation - use {{expr}} for computed value injection
/// </remarks>
public record SparqlLiteralExpression : Expression
{
    /// <summary>
    /// Raw SPARQL text content (SELECT/CONSTRUCT/ASK/DESCRIBE/UPDATE).
    /// Includes variable placeholders and interpolation markers.
    /// </summary>
    public required string SparqlText { get; init; }

    /// <summary>
    /// Variable bindings discovered during resolution.
    /// Populated by SparqlVariableBindingVisitor.
    /// </summary>
    public List<VariableBinding> Bindings { get; init; } = new();

    /// <summary>
    /// Interpolation sites ({{expr}}) for computed value injection.
    /// Populated during parsing if interpolation syntax is present.
    /// </summary>
    public List<Interpolation> Interpolations { get; init; } = new();
}

/// <summary>
/// Represents a Fifth variable reference within SPARQL literal.
/// Example: 'age' in ?&lt;SELECT * WHERE { ?s ex:age age }>;
/// </summary>
public record VariableBinding : AstThing
{
    /// <summary>
    /// Variable name as it appears in SPARQL text.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Resolved Fifth variable reference.
    /// Set by SparqlVariableBindingVisitor after symbol table lookup.
    /// Null if resolution failed (diagnostic emitted).
    /// </summary>
    public Expression? ResolvedExpression { get; set; }

    /// <summary>
    /// Inferred Fifth type.
    /// Set by TypeAnnotationVisitor.
    /// Used to determine RDF node type during lowering.
    /// </summary>
    public FifthType? Type { get; set; }

    /// <summary>
    /// Position within SPARQL text for diagnostics (character offset).
    /// </summary>
    public int PositionInLiteral { get; init; }

    /// <summary>
    /// Length of the variable name in SPARQL text.
    /// </summary>
    public int Length { get; init; }
}

/// <summary>
/// Represents an interpolation placeholder {{expr}} within SPARQL literal.
/// Example: {{prefix}} in ?&lt;SELECT * WHERE { &lt;{{prefix}}resource&gt; ?p ?o }>;
/// </summary>
public record Interpolation : AstThing
{
    /// <summary>
    /// Character position in SparqlText where interpolation starts.
    /// Index of the first '{' in '{{'...'}'.
    /// </summary>
    public required int Position { get; init; }

    /// <summary>
    /// Length of interpolation region (including {{ }}).
    /// Used to replace placeholder during lowering.
    /// </summary>
    public required int Length { get; init; }

    /// <summary>
    /// Fifth expression to evaluate and inject.
    /// Must be constant or simple variable reference (enforced during type checking).
    /// </summary>
    public required Expression Expression { get; init; }

    /// <summary>
    /// Result type after evaluation.
    /// Set by TypeAnnotationVisitor.
    /// Determines serialization strategy (IRI syntax vs literal).
    /// </summary>
    public FifthType? ResultType { get; set; }
}

/// <summary>
/// Query application expression: applying a SPARQL query to a store using the &lt;- operator.
/// Syntax: result = query &lt;- store
/// </summary>
/// <example>
/// <code>result: Result = ?&lt;SELECT ?name WHERE { ?s ex:name ?name }> &lt;- myStore;</code>
/// </example>
/// <remarks>
/// <para>
/// This expression applies a SPARQL Query to a Store, producing a Result discriminated union.
/// The Query operand (LHS) must be of type Fifth.System.Query, and the Store operand (RHS) 
/// must be assignable to a SPARQL-queryable store interface.
/// </para>
/// <para>
/// During lowering, this expression is transformed into a function call to 
/// Fifth.System.QueryApplicationExecutor.Execute(query, store, cancellationToken?).
/// The Result type inference depends on the query form (SELECT, CONSTRUCT, DESCRIBE, ASK).
/// </para>
/// </remarks>
public record QueryApplicationExp : Expression
{
    /// <summary>
    /// Query expression (left-hand side). Must evaluate to Fifth.System.Query type.
    /// Typically a SparqlLiteralExpression (?&lt;...>) or variable reference.
    /// </summary>
    public required Expression Query { get; init; }

    /// <summary>
    /// Store expression (right-hand side). Must evaluate to a SPARQL-queryable Store type.
    /// Typically a variable reference, member access, or function call returning Store.
    /// </summary>
    public required Expression Store { get; init; }

    /// <summary>
    /// Inferred Result type. Set by QueryApplicationTypeCheckVisitor.
    /// Result is a discriminated union (TabularResult | GraphResult | BooleanResult).
    /// </summary>
    public FifthType? InferredType { get; set; }
}

/// <summary>
/// Accessing a member of an object
/// </summary>
/// <example>
/// The member access expression can be treated as a binary operator
/// <code title="accessing the member someProp">var x = inst.someProp;</code>
/// </example>
/// <remarks>
/// <p>In its simplest form, this is simply a variable name without further member accesses or method calls.</p>
/// </remarks>
public record MemberAccessExp : Expression
{
    public required Expression LHS { get; init; }
    public required Expression? RHS { get; init; } // grammar expects var_name or function_call_expression
}

/// <summary>
/// Indexing expression for array/list element access
/// </summary>
/// <example>
/// <code title="accessing an array element">var x = arr[5];</code>
/// <code title="accessing a multidimensional array element">var y = matrix[2][3];</code>
/// </example>
/// <remarks>
/// <p>This expression represents indexing operations like arr[i] where arr is an array or list
/// and i is the index expression. The IndexExpression is the target being indexed,
/// and OffsetExpression is the index value.</p>
/// </remarks>
public record IndexerExpression : Expression
{
    public required Expression IndexExpression { get; init; }
    public required Expression OffsetExpression { get; init; }
}

/// <summary>An expression that supplies a set of values to the properties of an object being created</summary>
/// <example>
///   <code title="Initializing a Person Instance">var p = new Person()
/// { // the initializer starts here
///     FirstName = "Eric",
///     LastName = "Morecombe",
///     Profession = "Comedian"
/// };</code>
/// </example>
public record ObjectInitializerExp : Expression
{
    public FifthType TypeToInitialize { get; set; }
    public List<Expression> ConstructorArguments { get; set; } = [];
    public List<PropertyInitializerExp> PropertyInitialisers { get; set; }
    public FunctionDef? ResolvedConstructor { get; set; } // Set during constructor resolution
}

/// <summary>A part of the expression supplying a value for a specific property of an object being created</summary>
/// <example>
///   <code title="Initializing a Person Instance">var p = new Person()
/// {
///     FirstName = "Eric",    // This is a property initializer
///     LastName = "Morecombe", // so is this...
///     Profession = "Comedian"// and this
/// };</code>
/// </example>
public record PropertyInitializerExp : Expression
{
    public PropertyRef PropertyToInitialize { get; set; }
    public Expression RHS { get; set; }
}

/// <summary>
/// A unary operator applied to an expression
/// </summary>
/// <seealso cref="ast.Expression" />
/// <seealso cref="ast.IAnnotated" />
/// <seealso cref="System.IEquatable&lt;ast.AnnotatedThing&gt;" />
/// <seealso cref="ast.IAstThing" />
/// <seealso cref="ast_model.IVisitable" />
/// <seealso cref="System.IEquatable&lt;ast.AstThing&gt;" />
/// <seealso cref="System.IEquatable&lt;ast.Expression&gt;" />
/// <seealso cref="System.IEquatable&lt;ast.UnaryExp&gt;" />
public record UnaryExp : Expression
{
    public required Operator Operator { get; set; }
    public required Expression Operand { get; set; }
}

/// <summary>
/// Throw expression for use in expression contexts (e.g., null-coalescing, conditional operator).
/// Unlike ThrowStatement, this can appear where expressions are expected.
/// </summary>
/// <example>
/// <code>var x = value ?? throw new ArgumentNullException();</code>
/// <code>var y = condition ? result : throw new InvalidOperationException();</code>
/// </example>
public record ThrowExp : Expression
{
    public required Expression Exception { get; set; }
}

/// <summary>
/// A reference to a variable within an expression
/// </summary>
/// <seealso cref="ast.Expression" />
/// <seealso cref="ast.IAnnotated" />
/// <seealso cref="System.IEquatable&lt;ast.AnnotatedThing&gt;" />
/// <seealso cref="ast.IAstThing" />
/// <seealso cref="ast_model.IVisitable" />
/// <seealso cref="System.IEquatable&lt;ast.AstThing&gt;" />
/// <seealso cref="System.IEquatable&lt;ast.Expression&gt;" />
/// <seealso cref="System.IEquatable&lt;ast.VarRefExp&gt;" />
public record VarRefExp : Expression
{
    public required string VarName { get; init; }
    public VariableDecl VariableDecl { get; set; }
}

public abstract record List : Expression;

/// <summary>
/// A list instantiation expression
/// </summary>
/// <remarks>
/// The reason there is no explicit 'element type' property defined anymore, is that the type
/// information carried by the expression type is rich enough to contain the generic type parameters
/// needed to constrain the type of the list elements.
/// </remarks>
public record ListLiteral : List
{
    /// <summary>
    /// The set of expressions that supply values to insert into the list on creation.
    /// </summary>
    /// <value>
    /// The element expressions.
    /// </value>
    /// <remarks>
    /// All expressions must have the same type
    /// </remarks>
    public List<Expression> ElementExpressions { get; init; }
}

public record ListComprehension : List
{
    /// <summary>
    /// The projection expression - what value to produce for each item.
    /// Can be a VarRefExp, ObjectInstantiationExp, or any expression.
    /// </summary>
    public required Expression Projection { get; init; }

    /// <summary>
    /// The source expression to iterate over.
    /// For general comprehensions: any list/enumerable expression.
    /// For SPARQL comprehensions: expression whose type is a tabular SELECT result.
    /// </summary>
    public required Expression Source { get; init; }

    /// <summary>
    /// The iteration variable name (e.g., "x" in "x from nums").
    /// </summary>
    public required string VarName { get; init; }

    /// <summary>
    /// Zero or more where constraints (AND-ed together).
    /// Each constraint must evaluate to boolean.
    /// </summary>
    public List<Expression> Constraints { get; init; } = new();
}

public record Atom : Expression
{
    public AtomLiteralExp AtomExp { get; set; }
}

public record TripleLiteralExp : Expression
{
    public UriLiteralExp SubjectExp { get; set; }
    public UriLiteralExp PredicateExp { get; set; }
    public Expression ObjectExp { get; set; }
}

/// <summary>
/// Represents a syntactically recognized but malformed triple literal captured by the relaxed grammar
/// so a later diagnostic phase can emit TRPL001 without the parser rejecting the input outright.
/// Forms captured include: <s,p> (missing object), <s,p,o,> (trailing comma), and <s,p,o, extra ...> (too many components).
/// </summary>
public record MalformedTripleExp : Expression
{
    /// <summary>The raw textual components parsed (minimum 2).</summary>
    public List<Expression> Components { get; set; }
    /// <summary>Classification of malformed variant.</summary>
    public required string MalformedKind { get; set; }
}

public record Graph : Expression
{
    public UriLiteralExp GraphUri { get; set; }
    public List<TripleLiteralExp> Triples { get; set; }
}

#endregion Expressions

#region Knowledge Management

#endregion Knowledge Management