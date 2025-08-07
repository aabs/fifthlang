namespace ast_generated;
using ast;
using ast_model.Symbols;
using ast_model.TypeSystem;

public interface ITypeChecker
{
    public FifthType Infer(ScopeAstThing scope, AssemblyDef node);
    public FifthType Infer(ScopeAstThing scope, ModuleDef node);
    public FifthType Infer(ScopeAstThing scope, FunctionDef node);
    public FifthType Infer(ScopeAstThing scope, FunctorDef node);
    public FifthType Infer(ScopeAstThing scope, FieldDef node);
    public FifthType Infer(ScopeAstThing scope, PropertyDef node);
    public FifthType Infer(ScopeAstThing scope, MethodDef node);
    public FifthType Infer(ScopeAstThing scope, OverloadedFunctionDefinition node);
    public FifthType Infer(ScopeAstThing scope, InferenceRuleDef node);
    public FifthType Infer(ScopeAstThing scope, ParamDef node);
    public FifthType Infer(ScopeAstThing scope, ParamDestructureDef node);
    public FifthType Infer(ScopeAstThing scope, PropertyBindingDef node);
    public FifthType Infer(ScopeAstThing scope, TypeDef node);
    public FifthType Infer(ScopeAstThing scope, ClassDef node);
    public FifthType Infer(ScopeAstThing scope, VariableDecl node);
    public FifthType Infer(ScopeAstThing scope, AssemblyRef node);
    public FifthType Infer(ScopeAstThing scope, MemberRef node);
    public FifthType Infer(ScopeAstThing scope, PropertyRef node);
    public FifthType Infer(ScopeAstThing scope, TypeRef node);
    public FifthType Infer(ScopeAstThing scope, VarRef node);
    public FifthType Infer(ScopeAstThing scope, GraphNamespaceAlias node);
    public FifthType Infer(ScopeAstThing scope, AssignmentStatement node);
    public FifthType Infer(ScopeAstThing scope, BlockStatement node);
    public FifthType Infer(ScopeAstThing scope, KnowledgeManagementBlock node);
    public FifthType Infer(ScopeAstThing scope, ExpStatement node);
    public FifthType Infer(ScopeAstThing scope, ForStatement node);
    public FifthType Infer(ScopeAstThing scope, ForeachStatement node);
    public FifthType Infer(ScopeAstThing scope, GuardStatement node);
    public FifthType Infer(ScopeAstThing scope, IfElseStatement node);
    public FifthType Infer(ScopeAstThing scope, ReturnStatement node);
    public FifthType Infer(ScopeAstThing scope, VarDeclStatement node);
    public FifthType Infer(ScopeAstThing scope, WhileStatement node);
    public FifthType Infer(ScopeAstThing scope, AssertionStatement node);
    public FifthType Infer(ScopeAstThing scope, AssertionObject node);
    public FifthType Infer(ScopeAstThing scope, AssertionPredicate node);
    public FifthType Infer(ScopeAstThing scope, AssertionSubject node);
    public FifthType Infer(ScopeAstThing scope, RetractionStatement node);
    public FifthType Infer(ScopeAstThing scope, WithScopeStatement node);
    public FifthType Infer(ScopeAstThing scope, BinaryExp node);
    public FifthType Infer(ScopeAstThing scope, CastExp node);
    public FifthType Infer(ScopeAstThing scope, LambdaExp node);
    public FifthType Infer(ScopeAstThing scope, FuncCallExp node);
    public FifthType Infer(ScopeAstThing scope, Int8LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, Int16LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, Int32LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, Int64LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, UnsignedInt8LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, UnsignedInt16LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, UnsignedInt32LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, UnsignedInt64LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, Float4LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, Float8LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, Float16LiteralExp node);
    public FifthType Infer(ScopeAstThing scope, BooleanLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, CharLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, StringLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, DateLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, TimeLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, DateTimeLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, DurationLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, UriLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, AtomLiteralExp node);
    public FifthType Infer(ScopeAstThing scope, MemberAccessExp node);
    public FifthType Infer(ScopeAstThing scope, ObjectInitializerExp node);
    public FifthType Infer(ScopeAstThing scope, PropertyInitializerExp node);
    public FifthType Infer(ScopeAstThing scope, UnaryExp node);
    public FifthType Infer(ScopeAstThing scope, VarRefExp node);
    public FifthType Infer(ScopeAstThing scope, ListLiteral node);
    public FifthType Infer(ScopeAstThing scope, ListComprehension node);
    public FifthType Infer(ScopeAstThing scope, Atom node);
    public FifthType Infer(ScopeAstThing scope, Triple node);
    public FifthType Infer(ScopeAstThing scope, Graph node);
}

public abstract class FunctionalTypeChecker : ITypeChecker
{

    public FifthType Infer(AstThing exp)
    {
        if (exp == null) return default;
        var scope = exp.NearestScope();
        return exp switch
        {
            AssemblyDef node => Infer(scope, node),
            ModuleDef node => Infer(scope, node),
            FunctionDef node => Infer(scope, node),
            FunctorDef node => Infer(scope, node),
            FieldDef node => Infer(scope, node),
            PropertyDef node => Infer(scope, node),
            MethodDef node => Infer(scope, node),
            OverloadedFunctionDefinition node => Infer(scope, node),
            InferenceRuleDef node => Infer(scope, node),
            ParamDef node => Infer(scope, node),
            ParamDestructureDef node => Infer(scope, node),
            PropertyBindingDef node => Infer(scope, node),
            TypeDef node => Infer(scope, node),
            ClassDef node => Infer(scope, node),
            VariableDecl node => Infer(scope, node),
            AssemblyRef node => Infer(scope, node),
            MemberRef node => Infer(scope, node),
            PropertyRef node => Infer(scope, node),
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
            Int8LiteralExp node => Infer(scope, node),
            Int16LiteralExp node => Infer(scope, node),
            Int32LiteralExp node => Infer(scope, node),
            Int64LiteralExp node => Infer(scope, node),
            UnsignedInt8LiteralExp node => Infer(scope, node),
            UnsignedInt16LiteralExp node => Infer(scope, node),
            UnsignedInt32LiteralExp node => Infer(scope, node),
            UnsignedInt64LiteralExp node => Infer(scope, node),
            Float4LiteralExp node => Infer(scope, node),
            Float8LiteralExp node => Infer(scope, node),
            Float16LiteralExp node => Infer(scope, node),
            BooleanLiteralExp node => Infer(scope, node),
            CharLiteralExp node => Infer(scope, node),
            StringLiteralExp node => Infer(scope, node),
            DateLiteralExp node => Infer(scope, node),
            TimeLiteralExp node => Infer(scope, node),
            DateTimeLiteralExp node => Infer(scope, node),
            DurationLiteralExp node => Infer(scope, node),
            UriLiteralExp node => Infer(scope, node),
            AtomLiteralExp node => Infer(scope, node),
            MemberAccessExp node => Infer(scope, node),
            ObjectInitializerExp node => Infer(scope, node),
            PropertyInitializerExp node => Infer(scope, node),
            UnaryExp node => Infer(scope, node),
            VarRefExp node => Infer(scope, node),
            ListLiteral node => Infer(scope, node),
            ListComprehension node => Infer(scope, node),
            Atom node => Infer(scope, node),
            Triple node => Infer(scope, node),
            Graph node => Infer(scope, node),

            { } node => throw new ast_model.TypeCheckingException("Unrecognised type")
        };
    }

    public abstract FifthType Infer(ScopeAstThing scope, AssemblyDef node);
    public abstract FifthType Infer(ScopeAstThing scope, ModuleDef node);
    public abstract FifthType Infer(ScopeAstThing scope, FunctionDef node);
    public abstract FifthType Infer(ScopeAstThing scope, FunctorDef node);
    public abstract FifthType Infer(ScopeAstThing scope, FieldDef node);
    public abstract FifthType Infer(ScopeAstThing scope, PropertyDef node);
    public abstract FifthType Infer(ScopeAstThing scope, MethodDef node);
    public abstract FifthType Infer(ScopeAstThing scope, OverloadedFunctionDefinition node);
    public abstract FifthType Infer(ScopeAstThing scope, InferenceRuleDef node);
    public abstract FifthType Infer(ScopeAstThing scope, ParamDef node);
    public abstract FifthType Infer(ScopeAstThing scope, ParamDestructureDef node);
    public abstract FifthType Infer(ScopeAstThing scope, PropertyBindingDef node);
    public abstract FifthType Infer(ScopeAstThing scope, TypeDef node);
    public abstract FifthType Infer(ScopeAstThing scope, ClassDef node);
    public abstract FifthType Infer(ScopeAstThing scope, VariableDecl node);
    public abstract FifthType Infer(ScopeAstThing scope, AssemblyRef node);
    public abstract FifthType Infer(ScopeAstThing scope, MemberRef node);
    public abstract FifthType Infer(ScopeAstThing scope, PropertyRef node);
    public abstract FifthType Infer(ScopeAstThing scope, TypeRef node);
    public abstract FifthType Infer(ScopeAstThing scope, VarRef node);
    public abstract FifthType Infer(ScopeAstThing scope, GraphNamespaceAlias node);
    public abstract FifthType Infer(ScopeAstThing scope, AssignmentStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, BlockStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, KnowledgeManagementBlock node);
    public abstract FifthType Infer(ScopeAstThing scope, ExpStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, ForStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, ForeachStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, GuardStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, IfElseStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, ReturnStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, VarDeclStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, WhileStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionObject node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionPredicate node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionSubject node);
    public abstract FifthType Infer(ScopeAstThing scope, RetractionStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, WithScopeStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, BinaryExp node);
    public abstract FifthType Infer(ScopeAstThing scope, CastExp node);
    public abstract FifthType Infer(ScopeAstThing scope, LambdaExp node);
    public abstract FifthType Infer(ScopeAstThing scope, FuncCallExp node);
    public abstract FifthType Infer(ScopeAstThing scope, Int8LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, Int16LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, Int32LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, Int64LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, UnsignedInt8LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, UnsignedInt16LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, UnsignedInt32LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, UnsignedInt64LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, Float4LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, Float8LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, Float16LiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, BooleanLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, CharLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, StringLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, DateLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, TimeLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, DateTimeLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, DurationLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, UriLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, AtomLiteralExp node);
    public abstract FifthType Infer(ScopeAstThing scope, MemberAccessExp node);
    public abstract FifthType Infer(ScopeAstThing scope, ObjectInitializerExp node);
    public abstract FifthType Infer(ScopeAstThing scope, PropertyInitializerExp node);
    public abstract FifthType Infer(ScopeAstThing scope, UnaryExp node);
    public abstract FifthType Infer(ScopeAstThing scope, VarRefExp node);
    public abstract FifthType Infer(ScopeAstThing scope, ListLiteral node);
    public abstract FifthType Infer(ScopeAstThing scope, ListComprehension node);
    public abstract FifthType Infer(ScopeAstThing scope, Atom node);
    public abstract FifthType Infer(ScopeAstThing scope, Triple node);
    public abstract FifthType Infer(ScopeAstThing scope, Graph node);

}
