namespace ast_generated;
using ast;
using ast_model.Symbols;
using ast_model.TypeSystem;

public interface ITypeChecker
{
    public FifthType Infer(ScopeAstThing scope, FunctionDef node);
    public FifthType Infer(ScopeAstThing scope, FunctorDef node);
    public FifthType Infer(ScopeAstThing scope, MethodDef node);
    public FifthType Infer(ScopeAstThing scope, InferenceRuleDef node);
    public FifthType Infer(ScopeAstThing scope, KnowledgeManagementBlock node);
    public FifthType Infer(ScopeAstThing scope, ExpStatement node);
    public FifthType Infer(ScopeAstThing scope, GuardStatement node);
    public FifthType Infer(ScopeAstThing scope, AssertionStatement node);
    public FifthType Infer(ScopeAstThing scope, AssertionObject node);
    public FifthType Infer(ScopeAstThing scope, AssertionPredicate node);
    public FifthType Infer(ScopeAstThing scope, AssertionSubject node);
    public FifthType Infer(ScopeAstThing scope, LambdaExp node);
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
}

public abstract class FunctionalTypeChecker : ITypeChecker
{
    public FifthType Infer(AstThing exp)
    {
        if (exp == null) return default;
        var scope = exp.NearestScope();
        return exp switch
        {
            FunctionDef node => Infer(scope, node),
            FunctorDef node => Infer(scope, node),
            MethodDef node => Infer(scope, node),
            InferenceRuleDef node => Infer(scope, node),
            KnowledgeManagementBlock node => Infer(scope, node),
            ExpStatement node => Infer(scope, node),
            GuardStatement node => Infer(scope, node),
            AssertionStatement node => Infer(scope, node),
            AssertionObject node => Infer(scope, node),
            AssertionPredicate node => Infer(scope, node),
            AssertionSubject node => Infer(scope, node),
            LambdaExp node => Infer(scope, node),
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
            { } node => throw new ast_model.TypeCheckingException("Unrecognised type")
        };
    }

    public abstract FifthType Infer(ScopeAstThing scope, FunctionDef node);
    public abstract FifthType Infer(ScopeAstThing scope, FunctorDef node);
    public abstract FifthType Infer(ScopeAstThing scope, MethodDef node);
    public abstract FifthType Infer(ScopeAstThing scope, InferenceRuleDef node);
    public abstract FifthType Infer(ScopeAstThing scope, KnowledgeManagementBlock node);
    public abstract FifthType Infer(ScopeAstThing scope, ExpStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, GuardStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionStatement node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionObject node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionPredicate node);
    public abstract FifthType Infer(ScopeAstThing scope, AssertionSubject node);
    public abstract FifthType Infer(ScopeAstThing scope, LambdaExp node);
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
}
