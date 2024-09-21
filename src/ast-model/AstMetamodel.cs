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
    WhileStatement, WithScopeStatement
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
}

public class PropertyDef : MemberDef
{
    public bool IsWriteOnly { get; set; }
    public FieldDef? BackingField { get; set; }
    public MethodDef? Getter { get; set; }
    public MethodDef? Setter { get; set; }
    public bool CtorOnlySetter { get; set; }
}

public class MethodDef : MemberDef
{
    public List<ParamDef> Params { get; set; } = [];
    public BlockStatement Body { get; set; }
}

public class InferenceRuleDef : Definition
{
    // antecedent
    // consequent
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

public class StructDef : Definition
{
    public LinkedList<MemberDef> MemberDefs { get; set; }
}

public class LambdaDef : Definition
{
}

public class VariableDecl : Definition
{

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
}

public class TypeRef : Reference
{
}

public class VarRef : Reference
{
}

public class GraphNamespaceAlias : Reference
{
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

public class ExpStatement : Statement
{
}

public class ForStatement : Statement
{
}

public class ForeachStatement : Statement
{
}

public class GuardStatement : Statement
{
}

public class IfElseStatement : Statement
{
}

public class ReturnStatement : Statement
{
}

public class VarDeclStatement : Statement
{
    public VariableDecl VariableDecl { get; set; }
}

public class WhileStatement : Statement
{
}

public abstract class KnowledgeManagementStatement : Statement
{
}

public class AssertionStatement : Statement
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
