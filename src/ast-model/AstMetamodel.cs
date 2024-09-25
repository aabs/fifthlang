using System;
using ast_model.Attributes;

namespace ast_model;

#region Core Abstractions
public record struct TypeId(ushort Value);

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
    ///     Visible only within this type and sub-types.
    /// </summary>
    Protected,

    /// <summary>
    ///     Visible only within the declaring type.
    /// </summary>
    ProtectedInternal,
}

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
public record struct Symbol(string Name, SourceLocationMetadata? Location, SymbolKind Kind);

public record struct SourceLocationMetadata(
    int Column,
    string Filename,
    int Line,
    string OriginalText);

public record struct TypeMetadata(TypeId TypeId, Symbol Symbol);

public abstract record AstThing
{
    public required TypeMetadata Type { get; init; }
    public required string Name { get; init; }
    public required AstThing Parent { get; init; }
}

#endregion

#region Definitions

public abstract record Definition : AstThing
{
    public required Visibility Visibility { get; init; } = Visibility.Internal;
}

public record AssemblyDef : Definition
{
    public required string PublicKeyToken { get; init; }
    public required string Version { get; init; }
    public required LinkedList<AssemblyRef> AssemblyRefs { get; init; }
    public required LinkedList<ClassDef> ClassDefs { get; init; }
}

public record MemberDef : Definition
{
    public required bool IsReadOnly { get; init; }
}

public record FieldDef : MemberDef
{
    public required AccessConstraint[] AccessConstraints { get; init; } = [];
}

public record PropertyDef : MemberDef
{
    public required AccessConstraint[] AccessConstraints { get; init; } = [];
    public required bool IsWriteOnly { get; init; }
    public required FieldDef? BackingField { get; init; }
    public required MethodDef? Getter { get; init; }
    public required MethodDef? Setter { get; init; }
    public required bool CtorOnlySetter { get; init; }
}

public record MethodDef : MemberDef
{
    // todo: need the possibility of type parameters here.
    public required List<ParamDef> Params { get; init; } = [];
    public required BlockStatement Body { get; init; }
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
    public required Expression Antecedent { get; init; }

    public required KnowledgeManagementBlock Consequent { get; init; }
}

public record ParamDef : Definition
{
    public required Expression? ParameterConstraint { get; init; }
    public required ParamDestructureDef? DestructureDef { get; init; }
}

public record ParamDestructureDef : Definition
{
    public required LinkedList<PropertyBindingDef> Bindings { get; init; }
}

public record PropertyBindingDef : Definition
{
    public required VariableDecl IntroducedVariable { get; init; }
    public required PropertyDef ReferencedProperty { get; init; }
    public required ParamDestructureDef? DestructureDef { get; init; }
}

public record TypeDef : Definition
{
}

public record ClassDef : Definition
{
    public required LinkedList<MemberDef> MemberDefs { get; init; }
}

// out of scope for now...
//public record StructDef : Definition
//{
//    public required LinkedList<MemberDef> MemberDefs { get; init; }
//}


public record VariableDecl : Definition
{
    public required Expression? InitialValue { get; init; }
}

#endregion

#region References

public abstract record Reference : AstThing
{
}

public record AssemblyRef : Reference
{
    public required string PublicKeyToken{get;set;}
    public required string Version{get;set;}

}

public record MemberRef : Reference
{
    public required MemberDef MemberDef { get; init; }
}

public record TypeRef : Reference
{
}

public record VarRef : Reference
{
    public required VarDeclStatement VarDecl { get; init; }
}

public record GraphNamespaceAlias /*: Reference*/
{
    public required string Name { get; init; }
    public required Uri Uri { get; init; }
}

#endregion

#region Statements

public abstract record Statement : AstThing
{
}

public record AssignmentStatement : Statement
{
    public required Expression RHS { get; init; }
}

public record BlockStatement : Statement
{
    public required List<Statement> Statements { get; init; } = [];
}
public record KnowledgeManagementBlock
{
    public required List<KnowledgeManagementStatement> Statements { get; init; } = [];
}

/// <summary>
/// A statement containing a bare expression where the result is discarded
/// </summary>
public record ExpStatement : Statement
{
    public required Expression RHS { get; init; }
}

public record ForStatement : Statement
{
    public required Expression InitialValue { get; init; }
    public required Expression Constraint { get; init; }
    public required Expression IncrementExpression { get; init; }
    public required VariableDecl LoopVariable { get; init; }
    public required BlockStatement Body { get; init; }
}

public record ForeachStatement : Statement
{
    public required Expression Collection { get; init; }
    public required VariableDecl LoopVariable { get; init; }
    public required BlockStatement Body { get; init; }
}

// TODO: work out what I meant by this
// see here: obsidian://open?vault=notes&file=me%2Factive%2Fprojects%2Ffifthlang%2Fprojects.fifthlang.ast.guardstmt
public record GuardStatement : Statement
{
    public required Expression Condition { get; init; }
}

public record IfElseStatement : Statement
{
    public required Expression Condition { get; init; }
    public required BlockStatement ThenBlock { get; init; }
    public required BlockStatement ElseBlock { get; init; }
}

public record ReturnStatement : Statement
{
    public required Expression ReturnValue { get; init; }
}

public record VarDeclStatement : Statement
{
    public required VariableDecl VariableDecl { get; init; }
}

public record WhileStatement : Statement
{
    public required Expression Condition { get; init; }
    public required BlockStatement Body { get; init; }
}

public abstract record KnowledgeManagementStatement : Statement
{
}

/// <summary>
/// Asserts some statement to be true, for addition to the knowledge base
/// </summary>
public record AssertionStatement : Statement
{
    public required AssertionSubject AssertionSubject { get; init; }
    public required AssertionPredicate AssertionPredicate { get; init; }
    public required AssertionObject AssertionObject { get; init; }
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
public record AssertionObject
{
    //todo: need to work out how to represent the choices available to this (perhaps as some sort of discriminated union or polymorphic type)
}

/// <summary>
/// The predicate of an assertion, the thing that is asserted about the subject.
/// </summary>
public record AssertionPredicate
{
}

/// <summary>
/// The subject of an assertion, the thing said about the object.
/// </summary>
public record AssertionSubject
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
