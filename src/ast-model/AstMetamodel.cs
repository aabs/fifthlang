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
    ClassDef,
    Expression,
    ExpStatement,
    FieldDef,
    ForeachStatement,
    ForStatement,
    FuncCallExp,
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
    Triple,
    TypeDef,
    TypeRef,
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
public enum CollectionType
{
    SingleInstance,
    Array,
    List
}

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For things like in-memory assemblies etc")]
public partial struct AssemblyName;

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For anonymous members")]
public partial struct MemberName;

[Ignore, ValueObject<string>]
public partial struct NamespaceName;

[Ignore, ValueObject<ulong>]
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
    [IgnoreDuringVisit]
    public IScope EnclosingScope { get; init; }
    [IgnoreDuringVisit]
    public ISymbolTable SymbolTable { get; init; }

    public void Declare(Symbol symbol, IAstThing astThing, Dictionary<string, object> annotations)
    {
        var symTabEntry = new SymbolTableEntry
        {
            Symbol = symbol,
            Annotations = annotations,
            OriginatingAstThing = astThing
        };
        SymbolTable[symbol] = symTabEntry;
    }

    public bool TryResolve(Symbol symbol, out ISymbolTableEntry result)
    {
        result = null;
        var tmp = SymbolTable.Resolve(symbol);
        if (tmp != null)
        {
            result = tmp;
            return true;
        }

        return this.Parent.NearestScope()?.TryResolve(symbol, out result) ?? false;
    }

    public ISymbolTableEntry Resolve(Symbol symbol)
    {
        if (TryResolve(symbol, out var ste))
        {
            return ste;
        }

        throw new CompilationException($"Unable to resolve symbol {symbol.Name}");
    }
}


#endregion

#region Definitions

public abstract record Definition : AstThing
{
    public required Visibility Visibility { get; init; }
}

public record AssemblyDef : Definition
{
    public required AssemblyName Name { get; init; }
    public required string PublicKeyToken { get; init; }
    public required string Version { get; init; }
    public required List<AssemblyRef> AssemblyRefs { get; init; }
    public required List<ModuleDef> Modules { get; init; }
}

public record ModuleDef : Definition
{
    public required string OriginalModuleName { get; init; }
    public required NamespaceName NamespaceDecl { get; init; }
    public required List<ClassDef> Classes { get; init; }
    public required List<FunctionDef> Functions { get; init; }
}

/// <summary>
/// A bare function is a member of a singleton Global type.  A member, in other words.
/// </summary>
public record FunctionDef : MemberDef
{
    // todo: need the possibility of type parameters here.
    public required List<ParamDef> Params { get; set; } = [];
    public required BlockStatement Body { get; set; }
    public required TypeName? ReturnType { get; init; }
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
public record MethodDef : MemberDef
{
    // todo: need the possibility of type parameters here.
    public FunctionDef FunctionDef { get; set; }
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
    public required string Name { get; init; }
    public required Expression? ParameterConstraint { get; set; }
    public required ParamDestructureDef? DestructureDef { get; set; }
}

public record ParamDestructureDef : Definition
{
    public required List<PropertyBindingDef> Bindings { get; set; }
}

public record PropertyBindingDef : Definition
{
    public required MemberName IntroducedVariable { get; set; }
    public required MemberName ReferencedProperty { get; set; }
    public required ParamDestructureDef? DestructureDef { get; set; }
}

public record TypeDef : Definition
{
}

public record ClassDef : Definition
{
    public required TypeName Name { get; init; }
    public required List<MemberDef> MemberDefs { get; set; }
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
#endregion

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
    public required VarDeclStatement VarDecl { get; set; }
}

public record GraphNamespaceAlias : AstThing // TODO: is this a reference or something similar?
{
    public required Uri Uri { get; set; }
}

#endregion

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

public abstract record KnowledgeManagementStatement : Statement
{
}

/// <summary>
/// Asserts some statement to be true, for addition to the knowledge base
/// </summary>
public record AssertionStatement : Statement
{
    public Triple Assertion { get; set; }

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

#endregion

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
    public FunctionDef FunctionDef { get; set; }
    public List<Expression> InvocationArguments { get; set; }
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
    public List<PropertyInitializerExp> PropertyInitialisers { get; set; }
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
    public required string VarName { get; init; }
    public required string SourceName { get; init; }
    public Expression? MembershipConstraint { get; init; }
}

public record Atom : Expression
{
    public AtomLiteralExp AtomExp { get; set; }
}

public record Triple : Expression
{
    public UriLiteralExp SubjectExp { get; set; }
    public UriLiteralExp PredicateExp { get; set; }
    public Expression ObjectExp { get; set; }
}

public record Graph : Expression
{
    public UriLiteralExp GraphUri { get; set; }
    public List<Triple> Triples { get; set; }
}

#endregion
