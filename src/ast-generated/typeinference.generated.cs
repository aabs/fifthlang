

namespace ast_generated;
using ast_model;
using ast_model.Symbols;
using ast_model.TypeSystem;

public interface ITypeChecker
{
    public IType Infer(ScopeAstThing scope, UserDefinedType node);
    public IType Infer(ScopeAstThing scope, AssemblyDef node);
    public IType Infer(ScopeAstThing scope, FunctionDef node);
    public IType Infer(ScopeAstThing scope, FieldDef node);
    public IType Infer(ScopeAstThing scope, PropertyDef node);
    public IType Infer(ScopeAstThing scope, MethodDef node);
    public IType Infer(ScopeAstThing scope, InferenceRuleDef node);
    public IType Infer(ScopeAstThing scope, ParamDef node);
    public IType Infer(ScopeAstThing scope, ParamDestructureDef node);
    public IType Infer(ScopeAstThing scope, PropertyBindingDef node);
    public IType Infer(ScopeAstThing scope, TypeDef node);
    public IType Infer(ScopeAstThing scope, ClassDef node);
    public IType Infer(ScopeAstThing scope, VariableDecl node);
    public IType Infer(ScopeAstThing scope, AssemblyRef node);
    public IType Infer(ScopeAstThing scope, MemberRef node);
    public IType Infer(ScopeAstThing scope, TypeRef node);
    public IType Infer(ScopeAstThing scope, VarRef node);
    public IType Infer(ScopeAstThing scope, GraphNamespaceAlias node);
    public IType Infer(ScopeAstThing scope, AssignmentStatement node);
    public IType Infer(ScopeAstThing scope, BlockStatement node);
    public IType Infer(ScopeAstThing scope, KnowledgeManagementBlock node);
    public IType Infer(ScopeAstThing scope, ExpStatement node);
    public IType Infer(ScopeAstThing scope, ForStatement node);
    public IType Infer(ScopeAstThing scope, ForeachStatement node);
    public IType Infer(ScopeAstThing scope, GuardStatement node);
    public IType Infer(ScopeAstThing scope, IfElseStatement node);
    public IType Infer(ScopeAstThing scope, ReturnStatement node);
    public IType Infer(ScopeAstThing scope, VarDeclStatement node);
    public IType Infer(ScopeAstThing scope, WhileStatement node);
    public IType Infer(ScopeAstThing scope, AssertionStatement node);
    public IType Infer(ScopeAstThing scope, AssertionObject node);
    public IType Infer(ScopeAstThing scope, AssertionPredicate node);
    public IType Infer(ScopeAstThing scope, AssertionSubject node);
    public IType Infer(ScopeAstThing scope, RetractionStatement node);
    public IType Infer(ScopeAstThing scope, WithScopeStatement node);
    public IType Infer(ScopeAstThing scope, BinaryExp node);
    public IType Infer(ScopeAstThing scope, CastExp node);
    public IType Infer(ScopeAstThing scope, LambdaExp node);
    public IType Infer(ScopeAstThing scope, FuncCallExp node);
    public IType Infer(ScopeAstThing scope, LiteralExp node);
    public IType Infer(ScopeAstThing scope, MemberAccessExp node);
    public IType Infer(ScopeAstThing scope, ObjectInstantiationExp node);
    public IType Infer(ScopeAstThing scope, UnaryExp node);
    public IType Infer(ScopeAstThing scope, VarRefExp node);
    public IType Infer(ScopeAstThing scope, List node);
    public IType Infer(ScopeAstThing scope, Atom node);
    public IType Infer(ScopeAstThing scope, Triple node);
    public IType Infer(ScopeAstThing scope, Graph node);
}

public abstract class FunctionalTypeChecker : ITypeChecker
{

    public IType Infer(AstThing exp)
    {
        if (exp == null) return default;
        var scope = exp.NearestScope();
        return exp switch
        {
            UserDefinedType node => Infer(scope, node),
            AssemblyDef node => Infer(scope, node),
            FunctionDef node => Infer(scope, node),
            FieldDef node => Infer(scope, node),
            PropertyDef node => Infer(scope, node),
            MethodDef node => Infer(scope, node),
            InferenceRuleDef node => Infer(scope, node),
            ParamDef node => Infer(scope, node),
            ParamDestructureDef node => Infer(scope, node),
            PropertyBindingDef node => Infer(scope, node),
            TypeDef node => Infer(scope, node),
            ClassDef node => Infer(scope, node),
            VariableDecl node => Infer(scope, node),
            AssemblyRef node => Infer(scope, node),
            MemberRef node => Infer(scope, node),
            TypeRef node => Infer(scope, node),
            VarRef node => Infer(scope, node),
            GraphNamespaceAlias node => Infer(scope, node),
            AssignmentStatement node => Infer(scope, node),
            BlockStatement node => Infer(scope, node),
            KnowledgeManagementBlock node => Infer(scope, node),
            ExpStatement node => Infer(scope, node),
            ForStatement node => Infer(scope, node),
            ForeachStatement node => Infer(scope, node),
            GuardStatement node => Infer(scope, node),
            IfElseStatement node => Infer(scope, node),
            ReturnStatement node => Infer(scope, node),
            VarDeclStatement node => Infer(scope, node),
            WhileStatement node => Infer(scope, node),
            AssertionStatement node => Infer(scope, node),
            AssertionObject node => Infer(scope, node),
            AssertionPredicate node => Infer(scope, node),
            AssertionSubject node => Infer(scope, node),
            RetractionStatement node => Infer(scope, node),
            WithScopeStatement node => Infer(scope, node),
            BinaryExp node => Infer(scope, node),
            CastExp node => Infer(scope, node),
            LambdaExp node => Infer(scope, node),
            FuncCallExp node => Infer(scope, node),
            LiteralExp node => Infer(scope, node),
            MemberAccessExp node => Infer(scope, node),
            ObjectInstantiationExp node => Infer(scope, node),
            UnaryExp node => Infer(scope, node),
            VarRefExp node => Infer(scope, node),
            List node => Infer(scope, node),
            Atom node => Infer(scope, node),
            Triple node => Infer(scope, node),
            Graph node => Infer(scope, node),

            { } node => throw new TypeCheckingException("Unrecognised type")
        };
    }

    public abstract IType Infer(ScopeAstThing scope, UserDefinedType node);
    public abstract IType Infer(ScopeAstThing scope, AssemblyDef node);
    public abstract IType Infer(ScopeAstThing scope, FunctionDef node);
    public abstract IType Infer(ScopeAstThing scope, FieldDef node);
    public abstract IType Infer(ScopeAstThing scope, PropertyDef node);
    public abstract IType Infer(ScopeAstThing scope, MethodDef node);
    public abstract IType Infer(ScopeAstThing scope, InferenceRuleDef node);
    public abstract IType Infer(ScopeAstThing scope, ParamDef node);
    public abstract IType Infer(ScopeAstThing scope, ParamDestructureDef node);
    public abstract IType Infer(ScopeAstThing scope, PropertyBindingDef node);
    public abstract IType Infer(ScopeAstThing scope, TypeDef node);
    public abstract IType Infer(ScopeAstThing scope, ClassDef node);
    public abstract IType Infer(ScopeAstThing scope, VariableDecl node);
    public abstract IType Infer(ScopeAstThing scope, AssemblyRef node);
    public abstract IType Infer(ScopeAstThing scope, MemberRef node);
    public abstract IType Infer(ScopeAstThing scope, TypeRef node);
    public abstract IType Infer(ScopeAstThing scope, VarRef node);
    public abstract IType Infer(ScopeAstThing scope, GraphNamespaceAlias node);
    public abstract IType Infer(ScopeAstThing scope, AssignmentStatement node);
    public abstract IType Infer(ScopeAstThing scope, BlockStatement node);
    public abstract IType Infer(ScopeAstThing scope, KnowledgeManagementBlock node);
    public abstract IType Infer(ScopeAstThing scope, ExpStatement node);
    public abstract IType Infer(ScopeAstThing scope, ForStatement node);
    public abstract IType Infer(ScopeAstThing scope, ForeachStatement node);
    public abstract IType Infer(ScopeAstThing scope, GuardStatement node);
    public abstract IType Infer(ScopeAstThing scope, IfElseStatement node);
    public abstract IType Infer(ScopeAstThing scope, ReturnStatement node);
    public abstract IType Infer(ScopeAstThing scope, VarDeclStatement node);
    public abstract IType Infer(ScopeAstThing scope, WhileStatement node);
    public abstract IType Infer(ScopeAstThing scope, AssertionStatement node);
    public abstract IType Infer(ScopeAstThing scope, AssertionObject node);
    public abstract IType Infer(ScopeAstThing scope, AssertionPredicate node);
    public abstract IType Infer(ScopeAstThing scope, AssertionSubject node);
    public abstract IType Infer(ScopeAstThing scope, RetractionStatement node);
    public abstract IType Infer(ScopeAstThing scope, WithScopeStatement node);
    public abstract IType Infer(ScopeAstThing scope, BinaryExp node);
    public abstract IType Infer(ScopeAstThing scope, CastExp node);
    public abstract IType Infer(ScopeAstThing scope, LambdaExp node);
    public abstract IType Infer(ScopeAstThing scope, FuncCallExp node);
    public abstract IType Infer(ScopeAstThing scope, LiteralExp node);
    public abstract IType Infer(ScopeAstThing scope, MemberAccessExp node);
    public abstract IType Infer(ScopeAstThing scope, ObjectInstantiationExp node);
    public abstract IType Infer(ScopeAstThing scope, UnaryExp node);
    public abstract IType Infer(ScopeAstThing scope, VarRefExp node);
    public abstract IType Infer(ScopeAstThing scope, List node);
    public abstract IType Infer(ScopeAstThing scope, Atom node);
    public abstract IType Infer(ScopeAstThing scope, Triple node);
    public abstract IType Infer(ScopeAstThing scope, Graph node);

}

