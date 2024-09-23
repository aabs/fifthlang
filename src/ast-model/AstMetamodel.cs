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

public abstract class AstThing
{
    public TypeMetadata Type { get; set; }
    public string Name { get; set; }
    public AstThing Parent { get; set; }
}

#endregion

#region Definitions

public abstract class Definition : AstThing
{
    public Visibility Visibility { get; set; } = Visibility.Internal;
}

public class AssemblyDef : Definition
{
    public string PublicKeyToken { get; set; }
    public string Version { get; set; }
    public LinkedList<AssemblyRef> AssemblyRefs { get; set; }
    public LinkedList<ClassDef> ClassDefs { get; set; }
}

public class MemberDef : Definition
{
    public Visibility Visibility { get; set; }
    public bool IsReadOnly { get; set; }
}

public class FieldDef : MemberDef
{
    public AccessConstraint[] AccessConstraints { get; set; } = [];
}

public class PropertyDef : MemberDef
{
    public AccessConstraint[] AccessConstraints { get; set; } = [];
    public bool IsWriteOnly { get; set; }
    public FieldDef? BackingField { get; set; }
    public MethodDef? Getter { get; set; }
    public MethodDef? Setter { get; set; }
    public bool CtorOnlySetter { get; set; }
}

public class MethodDef : MemberDef
{
    // todo: need the possibility of type parameters here.
    public List<ParamDef> Params { get; set; } = [];
    public BlockStatement Body { get; set; }
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
public class InferenceRuleDef : Definition
{
    public Expression Antecedent { get; set; }

    public KnowledgeManagementBlock Consequent { get; set; }
}

public class ParamDef : Definition
{
    public Expression? ParameterConstraint { get; set; }
    public ParamDestructureDef? DestructureDef { get; set; }
}

public class ParamDestructureDef : Definition
{
    public LinkedList<PropertyBindingDef> Bindings { get; set; }
}

public class PropertyBindingDef : Definition
{
    public VariableDecl IntroducedVariable { get; set; }
    public PropertyDef ReferencedProperty { get; set; }
    public ParamDestructureDef? DestructureDef { get; set; }
}

public class TypeDef : Definition
{
}

public class ClassDef : Definition
{
    public LinkedList<MemberDef> MemberDefs { get; set; }
}

// out of scope for now...
//public class StructDef : Definition
//{
//    public LinkedList<MemberDef> MemberDefs { get; set; }
//}


public class VariableDecl : Definition
{
    public Expression? InitialValue { get; set; }
}

#endregion

#region References

public abstract class Reference : AstThing
{
}

public class AssemblyRef : Reference
{
    public string PublicKeyToken{get;set;}
    public string Version{get;set;}

}

public class MemberRef : Reference
{
    public MemberDef MemberDef { get; set; }
}

public class TypeRef : Reference
{
}

public class VarRef : Reference
{
    public VarDeclStatement VarDecl { get; set; }
}

public class GraphNamespaceAlias /*: Reference*/
{
    public string Name { get; set; }
    public Uri Uri { get; set; }
}

#endregion

#region Statements

public abstract class Statement : AstThing
{
}

public class AssignmentStatement : Statement
{
    public Expression RHS { get; set; }
}

public class BlockStatement : Statement
{
    public List<Statement> Statements { get; set; } = [];
}
public class KnowledgeManagementBlock
{
    public List<KnowledgeManagementStatement> Statements { get; set; } = [];
}

/// <summary>
/// A statement containing a bare expression where the result is discarded
/// </summary>
public class ExpStatement : Statement
{
    public Expression RHS { get; set; }
}

public class ForStatement : Statement
{
    public Expression InitialValue { get; set; }
    public Expression Constraint { get; set; }
    public Expression IncrementExpression { get; set; }
    public VariableDecl LoopVariable { get; set; }
    public BlockStatement Body { get; set; }
}

public class ForeachStatement : Statement
{
    public Expression Collection { get; set; }
    public VariableDecl LoopVariable { get; set; }
    public BlockStatement Body { get; set; }
}

// TODO: work out what I meant by this
// see here: obsidian://open?vault=notes&file=me%2Factive%2Fprojects%2Ffifthlang%2Fprojects.fifthlang.ast.guardstmt
public class GuardStatement : Statement
{
    public Expression Condition { get; set; }
}

public class IfElseStatement : Statement
{
    public Expression Condition { get; set; }
    public BlockStatement ThenBlock { get; set; }
    public BlockStatement ElseBlock { get; set; }
}

public class ReturnStatement : Statement
{
    public Expression ReturnValue { get; set; }
}

public class VarDeclStatement : Statement
{
    public VariableDecl VariableDecl { get; set; }
}

public class WhileStatement : Statement
{
    public Expression Condition { get; set; }
    public BlockStatement Body { get; set; }
}

public abstract class KnowledgeManagementStatement : Statement
{
}

/// <summary>
/// Asserts some statement to be true, for addition to the knowledge base
/// </summary>
public class AssertionStatement : Statement
{
    public AssertionSubject AssertionSubject { get; set; }
    public AssertionPredicate AssertionPredicate { get; set; }
    public AssertionObject AssertionObject { get; set; }
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
public class AssertionObject
{
    //todo: need to work out how to represent the choices available to this (perhaps as some sort of discriminated union or polymorphic type)
}

/// <summary>
/// The predicate of an assertion, the thing that is asserted about the subject.
/// </summary>
public class AssertionPredicate
{
}

/// <summary>
/// The subject of an assertion, the thing said about the object.
/// </summary>
public class AssertionSubject
{
}

public class RetractionStatement : Statement
{
}

public class WithScopeStatement : Statement
{
}

#endregion

#region Expressions

public abstract class Expression : AstThing
{
}

public class BinaryExp : Expression
{
}

public class CastExp : Expression
{
}
public class LambdaExp : Expression
{
}

public class FuncCallExp : Expression
{
}

public class LiteralExp : Expression
{
}

public class MemberAccessExp : Expression
{
}

public class ObjectInstantiationExp : Expression
{
}

public class UnaryExp : Expression
{
}

public class VarRefExp : Expression
{
}

public class List : Expression
{
}

public class Atom : Expression
{
}

public class Triple : Expression
{
}

public class Graph : Expression
{
}

#endregion
