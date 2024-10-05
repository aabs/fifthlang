// ReSharper disable UnusedMember.Global

using ast_model.Symbols;

namespace ast;
using ast_model;
using ast_model.TypeSystem;

#region Core Abstractions

[Ignore, ValueObject<ushort>]
public partial struct TypeId;

[Ignore, ValueObject<ushort>]
[Instance("short", 0, "Short has lowest seniority")]
[Instance("integer", 0)]
[Instance("long", 0)]
[Instance("float", 0)]
[Instance("double", 0)]
[Instance("decimal", 0, "decimal has highest seniority")]
public partial struct TypeCoertionSeniority;

[Ignore, ValueObject<ulong>]
public partial struct OperatorId;

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For things like in-memory assemblies etc")]
public partial struct AssemblyName;

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For anonymous types")]
public partial struct TypeName;

[Ignore, ValueObject<string>]
[Instance("anonymous", "", "For anonymous members")]
public partial struct MemberName;

[Ignore, ValueObject<string>]
public partial struct NamespaceName;


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
    // ArithmeticOperator
    Add,
    Subtract,
    Multiply,
    Divide,
    Rem,
    Mod,

    // LogicalOperators
    And,
    Or,
    Not,
    Nand,
    Nor,
    Xor,

    // RelationalOperators
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual
}

public enum OperatorPosition
{
    Prefix, Infix, Postfix
}
public enum SymbolKind
{
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
public record struct Symbol(string Name, SourceLocationMetadata? Location, SymbolKind Kind);

public record struct SourceLocationMetadata(
    int Column,
    string Filename,
    int Line,
    string OriginalText);

public record struct TypeMetadata(TypeId TypeId, Symbol Symbol);

public abstract record AstThing : AnnotatedThing, IAstThing
{
    public virtual void Accept(IVisitor visitor)
    {
        visitor.OnEnter(this);
        visitor.OnLeave(this);
    }

    public required SourceContext SourceContext { get; init; }
    public required TypeMetadata Type { get; init; }
    public required IAstThing Parent { get; init; }

    public void Deconstruct( out SourceContext sourceContext, out TypeMetadata type, out IAstThing parent, out Dictionary<string, object> annotations)
    {
        sourceContext = SourceContext;
        type = Type;
        parent = Parent;
        annotations = Annotations;
    }
}

public abstract record ScopeAstThing : AstThing, IScope
{
    [IgnoreDuringVisit]
    public IScope EnclosingScope { get; init; }
    [IgnoreDuringVisit]
    public ISymbolTable SymbolTable { get; init; }

    public void Declare(Symbol symbol, IAstThing astThing, SourceContext srcContext, Dictionary<string, object> annotations)
    {
        var symTabEntry = new SymTabEntry
        {
            Symbol = symbol,
            Annotations = annotations,
            SourceContext = srcContext,
            Context = astThing
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

public record UserDefinedType : AstThing, IType
{
    public ClassDef ClassDef { get; init; }
    public TypeName Name { get; init; }
    public NamespaceName Namespace { get; init; }
    public TypeId TypeId { get; init; }

    public void Deconstruct(out ClassDef ClassDef, out TypeName Name, out NamespaceName Namespace, out TypeId TypeId)
    {
        ClassDef = this.ClassDef;
        Name = this.Name;
        Namespace = this.Namespace;
        TypeId = this.TypeId;
    }
}

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
    public required List<ClassDef> ClassDefs { get; init; }
}

/// <summary>
/// A bare function is a member of a singleton Global type.  A member, in other words.
/// </summary>
public record FunctionDef : MemberDef
{
    // todo: need the possibility of type parameters here.
    public required List<ParamDef> Params { get; set; } = [];
    public required BlockStatement Body { get; set; }
}


public abstract record MemberDef : Definition
{
    public required MemberName Name { get; init; }
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
    public required Expression? ParameterConstraint { get; set; }
    public required ParamDestructureDef? DestructureDef { get; set; }
}

public record ParamDestructureDef : Definition
{
    public required List<PropertyBindingDef> Bindings { get; set; }
}

public record PropertyBindingDef : Definition
{
    public required VariableDecl IntroducedVariable { get; set; }
    public required PropertyDef ReferencedProperty { get; set; }
    public required ParamDestructureDef? DestructureDef { get; set; }
}

public record TypeDef : Definition
{
}

public record ClassDef : Definition
{
    public required string Namespace { get; set; }
    public required List<MemberDef> MemberDefs { get; set; }
}

// out of scope for now...
//public record StructDef : Definition
//{
//    public required List<MemberDef> MemberDefs { get; set; }
//}


public record VariableDecl : Definition
{
    public required Expression? InitialValue { get; set; }
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
    public required MemberDef MemberDef { get; set; }
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
    public required Expression RHS { get; set; }
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
    public required Expression Left { get; set; }
    public required Operator Op { get; set; }
    public required Expression Right { get; set; }
}

public record CastExp : Expression
{
}
public record LambdaExp : Expression
{
}

public record FuncCallExp : Expression
{
}

public record LiteralExp : Expression
{
}

public record MemberAccessExp : Expression
{
}

public record ObjectInstantiationExp : Expression
{
}

public record UnaryExp : Expression
{
    public required Operator Op { get; set; }
    public required Expression Operand { get; set; }
}

public record VarRefExp : Expression
{
}

public record List : Expression
{
}

public record Atom : Expression
{
}

public record Triple : Expression
{
}

public record Graph : Expression
{
}

#endregion
