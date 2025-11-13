namespace ast_generated;
using ast;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Result of rewriting an AST node, carrying the rewritten node and any statements that should be hoisted.
/// </summary>
public record RewriteResult(AstThing Node, List<Statement> Prologue)
{
    /// <summary>
    /// Creates a RewriteResult with no prologue statements.
    /// </summary>
    public static RewriteResult From(AstThing node) => new(node, []);
}

/// <summary>
/// Interface for AST rewriting with statement-level desugaring support.
/// </summary>
public interface IAstRewriter
{
    RewriteResult Rewrite(AstThing ctx);
    RewriteResult VisitAssemblyDef(AssemblyDef ctx);
    RewriteResult VisitModuleDef(ModuleDef ctx);
    RewriteResult VisitFunctionDef(FunctionDef ctx);
    RewriteResult VisitFunctorDef(FunctorDef ctx);
    RewriteResult VisitFieldDef(FieldDef ctx);
    RewriteResult VisitPropertyDef(PropertyDef ctx);
    RewriteResult VisitMethodDef(MethodDef ctx);
    RewriteResult VisitOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx);
    RewriteResult VisitOverloadedFunctionDef(OverloadedFunctionDef ctx);
    RewriteResult VisitInferenceRuleDef(InferenceRuleDef ctx);
    RewriteResult VisitParamDef(ParamDef ctx);
    RewriteResult VisitParamDestructureDef(ParamDestructureDef ctx);
    RewriteResult VisitPropertyBindingDef(PropertyBindingDef ctx);
    RewriteResult VisitTypeDef(TypeDef ctx);
    RewriteResult VisitClassDef(ClassDef ctx);
    RewriteResult VisitVariableDecl(VariableDecl ctx);
    RewriteResult VisitAssemblyRef(AssemblyRef ctx);
    RewriteResult VisitMemberRef(MemberRef ctx);
    RewriteResult VisitPropertyRef(PropertyRef ctx);
    RewriteResult VisitTypeRef(TypeRef ctx);
    RewriteResult VisitVarRef(VarRef ctx);
    RewriteResult VisitGraphNamespaceAlias(GraphNamespaceAlias ctx);
    RewriteResult VisitAssignmentStatement(AssignmentStatement ctx);
    RewriteResult VisitBlockStatement(BlockStatement ctx);
    RewriteResult VisitKnowledgeManagementBlock(KnowledgeManagementBlock ctx);
    RewriteResult VisitExpStatement(ExpStatement ctx);
    RewriteResult VisitEmptyStatement(EmptyStatement ctx);
    RewriteResult VisitForStatement(ForStatement ctx);
    RewriteResult VisitForeachStatement(ForeachStatement ctx);
    RewriteResult VisitGuardStatement(GuardStatement ctx);
    RewriteResult VisitIfElseStatement(IfElseStatement ctx);
    RewriteResult VisitReturnStatement(ReturnStatement ctx);
    RewriteResult VisitVarDeclStatement(VarDeclStatement ctx);
    RewriteResult VisitWhileStatement(WhileStatement ctx);
    RewriteResult VisitTryStatement(TryStatement ctx);
    RewriteResult VisitCatchClause(CatchClause ctx);
    RewriteResult VisitThrowStatement(ThrowStatement ctx);
    RewriteResult VisitAssertionStatement(AssertionStatement ctx);
    RewriteResult VisitAssertionObject(AssertionObject ctx);
    RewriteResult VisitAssertionPredicate(AssertionPredicate ctx);
    RewriteResult VisitAssertionSubject(AssertionSubject ctx);
    RewriteResult VisitRetractionStatement(RetractionStatement ctx);
    RewriteResult VisitWithScopeStatement(WithScopeStatement ctx);
    RewriteResult VisitBinaryExp(BinaryExp ctx);
    RewriteResult VisitCastExp(CastExp ctx);
    RewriteResult VisitLambdaExp(LambdaExp ctx);
    RewriteResult VisitFuncCallExp(FuncCallExp ctx);
    RewriteResult VisitInt8LiteralExp(Int8LiteralExp ctx);
    RewriteResult VisitInt16LiteralExp(Int16LiteralExp ctx);
    RewriteResult VisitInt32LiteralExp(Int32LiteralExp ctx);
    RewriteResult VisitInt64LiteralExp(Int64LiteralExp ctx);
    RewriteResult VisitUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx);
    RewriteResult VisitUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx);
    RewriteResult VisitUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx);
    RewriteResult VisitUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx);
    RewriteResult VisitFloat4LiteralExp(Float4LiteralExp ctx);
    RewriteResult VisitFloat8LiteralExp(Float8LiteralExp ctx);
    RewriteResult VisitFloat16LiteralExp(Float16LiteralExp ctx);
    RewriteResult VisitBooleanLiteralExp(BooleanLiteralExp ctx);
    RewriteResult VisitCharLiteralExp(CharLiteralExp ctx);
    RewriteResult VisitStringLiteralExp(StringLiteralExp ctx);
    RewriteResult VisitDateLiteralExp(DateLiteralExp ctx);
    RewriteResult VisitTimeLiteralExp(TimeLiteralExp ctx);
    RewriteResult VisitDateTimeLiteralExp(DateTimeLiteralExp ctx);
    RewriteResult VisitDurationLiteralExp(DurationLiteralExp ctx);
    RewriteResult VisitUriLiteralExp(UriLiteralExp ctx);
    RewriteResult VisitAtomLiteralExp(AtomLiteralExp ctx);
    RewriteResult VisitTriGLiteralExpression(TriGLiteralExpression ctx);
    RewriteResult VisitInterpolatedExpression(InterpolatedExpression ctx);
    RewriteResult VisitSparqlLiteralExpression(SparqlLiteralExpression ctx);
    RewriteResult VisitVariableBinding(VariableBinding ctx);
    RewriteResult VisitInterpolation(Interpolation ctx);
    RewriteResult VisitMemberAccessExp(MemberAccessExp ctx);
    RewriteResult VisitIndexerExpression(IndexerExpression ctx);
    RewriteResult VisitObjectInitializerExp(ObjectInitializerExp ctx);
    RewriteResult VisitPropertyInitializerExp(PropertyInitializerExp ctx);
    RewriteResult VisitUnaryExp(UnaryExp ctx);
    RewriteResult VisitThrowExp(ThrowExp ctx);
    RewriteResult VisitVarRefExp(VarRefExp ctx);
    RewriteResult VisitListLiteral(ListLiteral ctx);
    RewriteResult VisitListComprehension(ListComprehension ctx);
    RewriteResult VisitAtom(Atom ctx);
    RewriteResult VisitTripleLiteralExp(TripleLiteralExp ctx);
    RewriteResult VisitMalformedTripleExp(MalformedTripleExp ctx);
    RewriteResult VisitGraph(Graph ctx);
}

/// <summary>
/// Default AST rewriter that performs structure-preserving rewrites while aggregating prologue statements.
/// Prologue statements are hoisted upward until consumed by a BlockStatement.
/// </summary>
public class DefaultAstRewriter : IAstRewriter
{
    public virtual RewriteResult Rewrite(AstThing ctx)
    {
        if(ctx == null) return RewriteResult.From(ctx);
        return ctx switch
        {
             AssemblyDef node => VisitAssemblyDef(node),
             ModuleDef node => VisitModuleDef(node),
             FunctionDef node => VisitFunctionDef(node),
             FunctorDef node => VisitFunctorDef(node),
             FieldDef node => VisitFieldDef(node),
             PropertyDef node => VisitPropertyDef(node),
             MethodDef node => VisitMethodDef(node),
             OverloadedFunctionDefinition node => VisitOverloadedFunctionDefinition(node),
             OverloadedFunctionDef node => VisitOverloadedFunctionDef(node),
             InferenceRuleDef node => VisitInferenceRuleDef(node),
             ParamDef node => VisitParamDef(node),
             ParamDestructureDef node => VisitParamDestructureDef(node),
             PropertyBindingDef node => VisitPropertyBindingDef(node),
             TypeDef node => VisitTypeDef(node),
             ClassDef node => VisitClassDef(node),
             VariableDecl node => VisitVariableDecl(node),
             AssemblyRef node => VisitAssemblyRef(node),
             MemberRef node => VisitMemberRef(node),
             PropertyRef node => VisitPropertyRef(node),
             TypeRef node => VisitTypeRef(node),
             VarRef node => VisitVarRef(node),
             GraphNamespaceAlias node => VisitGraphNamespaceAlias(node),
             AssignmentStatement node => VisitAssignmentStatement(node),
             BlockStatement node => VisitBlockStatement(node),
             KnowledgeManagementBlock node => VisitKnowledgeManagementBlock(node),
             ExpStatement node => VisitExpStatement(node),
             EmptyStatement node => VisitEmptyStatement(node),
             ForStatement node => VisitForStatement(node),
             ForeachStatement node => VisitForeachStatement(node),
             GuardStatement node => VisitGuardStatement(node),
             IfElseStatement node => VisitIfElseStatement(node),
             ReturnStatement node => VisitReturnStatement(node),
             VarDeclStatement node => VisitVarDeclStatement(node),
             WhileStatement node => VisitWhileStatement(node),
             TryStatement node => VisitTryStatement(node),
             CatchClause node => VisitCatchClause(node),
             ThrowStatement node => VisitThrowStatement(node),
             AssertionStatement node => VisitAssertionStatement(node),
             AssertionObject node => VisitAssertionObject(node),
             AssertionPredicate node => VisitAssertionPredicate(node),
             AssertionSubject node => VisitAssertionSubject(node),
             RetractionStatement node => VisitRetractionStatement(node),
             WithScopeStatement node => VisitWithScopeStatement(node),
             BinaryExp node => VisitBinaryExp(node),
             CastExp node => VisitCastExp(node),
             LambdaExp node => VisitLambdaExp(node),
             FuncCallExp node => VisitFuncCallExp(node),
             Int8LiteralExp node => VisitInt8LiteralExp(node),
             Int16LiteralExp node => VisitInt16LiteralExp(node),
             Int32LiteralExp node => VisitInt32LiteralExp(node),
             Int64LiteralExp node => VisitInt64LiteralExp(node),
             UnsignedInt8LiteralExp node => VisitUnsignedInt8LiteralExp(node),
             UnsignedInt16LiteralExp node => VisitUnsignedInt16LiteralExp(node),
             UnsignedInt32LiteralExp node => VisitUnsignedInt32LiteralExp(node),
             UnsignedInt64LiteralExp node => VisitUnsignedInt64LiteralExp(node),
             Float4LiteralExp node => VisitFloat4LiteralExp(node),
             Float8LiteralExp node => VisitFloat8LiteralExp(node),
             Float16LiteralExp node => VisitFloat16LiteralExp(node),
             BooleanLiteralExp node => VisitBooleanLiteralExp(node),
             CharLiteralExp node => VisitCharLiteralExp(node),
             StringLiteralExp node => VisitStringLiteralExp(node),
             DateLiteralExp node => VisitDateLiteralExp(node),
             TimeLiteralExp node => VisitTimeLiteralExp(node),
             DateTimeLiteralExp node => VisitDateTimeLiteralExp(node),
             DurationLiteralExp node => VisitDurationLiteralExp(node),
             UriLiteralExp node => VisitUriLiteralExp(node),
             AtomLiteralExp node => VisitAtomLiteralExp(node),
             TriGLiteralExpression node => VisitTriGLiteralExpression(node),
             InterpolatedExpression node => VisitInterpolatedExpression(node),
             SparqlLiteralExpression node => VisitSparqlLiteralExpression(node),
             VariableBinding node => VisitVariableBinding(node),
             Interpolation node => VisitInterpolation(node),
             MemberAccessExp node => VisitMemberAccessExp(node),
             IndexerExpression node => VisitIndexerExpression(node),
             ObjectInitializerExp node => VisitObjectInitializerExp(node),
             PropertyInitializerExp node => VisitPropertyInitializerExp(node),
             UnaryExp node => VisitUnaryExp(node),
             ThrowExp node => VisitThrowExp(node),
             VarRefExp node => VisitVarRefExp(node),
             ListLiteral node => VisitListLiteral(node),
             ListComprehension node => VisitListComprehension(node),
             Atom node => VisitAtom(node),
             TripleLiteralExp node => VisitTripleLiteralExp(node),
             MalformedTripleExp node => VisitMalformedTripleExp(node),
             Graph node => VisitGraph(node),

            { } node => RewriteResult.From(null),
        };
    }

    public virtual RewriteResult VisitAssemblyDef(AssemblyDef ctx)
    {
        var prologue = new List<Statement>();
        List<ast.AssemblyRef> tmpAssemblyRefs = [];
        foreach (var item in ctx.AssemblyRefs)
        {
            var rr = Rewrite(item);
            tmpAssemblyRefs.Add((ast.AssemblyRef)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        List<ast.ModuleDef> tmpModules = [];
        foreach (var item in ctx.Modules)
        {
            var rr = Rewrite(item);
            tmpModules.Add((ast.ModuleDef)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         AssemblyRefs = tmpAssemblyRefs
        ,Modules = tmpModules
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitModuleDef(ModuleDef ctx)
    {
        var prologue = new List<Statement>();
        List<ast.ClassDef> tmpClasses = [];
        foreach (var item in ctx.Classes)
        {
            var rr = Rewrite(item);
            tmpClasses.Add((ast.ClassDef)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        List<ast.ScopedDefinition> tmpFunctions = [];
        foreach (var item in ctx.Functions)
        {
            var rr = Rewrite(item);
            tmpFunctions.Add((ast.ScopedDefinition)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Classes = tmpClasses
        ,Functions = tmpFunctions
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFunctionDef(FunctionDef ctx)
    {
        var prologue = new List<Statement>();
        List<ast.ParamDef> tmpParams = [];
        foreach (var item in ctx.Params)
        {
            var rr = Rewrite(item);
            tmpParams.Add((ast.ParamDef)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rrBody = Rewrite((AstThing)ctx.Body);
        prologue.AddRange(rrBody.Prologue);
        var rebuilt = ctx with {
         Params = tmpParams
        ,Body = (ast.BlockStatement)rrBody.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFunctorDef(FunctorDef ctx)
    {
        var prologue = new List<Statement>();
        var rrInvocationFuncDev = Rewrite((AstThing)ctx.InvocationFuncDev);
        prologue.AddRange(rrInvocationFuncDev.Prologue);
        var rebuilt = ctx with {
         InvocationFuncDev = (ast.FunctionDef)rrInvocationFuncDev.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFieldDef(FieldDef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitPropertyDef(PropertyDef ctx)
    {
        var prologue = new List<Statement>();
        var rrBackingField = Rewrite((AstThing)ctx.BackingField);
        prologue.AddRange(rrBackingField.Prologue);
        var rrGetter = Rewrite((AstThing)ctx.Getter);
        prologue.AddRange(rrGetter.Prologue);
        var rrSetter = Rewrite((AstThing)ctx.Setter);
        prologue.AddRange(rrSetter.Prologue);
        var rebuilt = ctx with {
         BackingField = (ast.FieldDef)rrBackingField.Node
        ,Getter = (ast.MethodDef)rrGetter.Node
        ,Setter = (ast.MethodDef)rrSetter.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMethodDef(MethodDef ctx)
    {
        var prologue = new List<Statement>();
        var rrFunctionDef = Rewrite((AstThing)ctx.FunctionDef);
        prologue.AddRange(rrFunctionDef.Prologue);
        var rebuilt = ctx with {
         FunctionDef = (ast.FunctionDef)rrFunctionDef.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitOverloadedFunctionDef(OverloadedFunctionDef ctx)
    {
        var prologue = new List<Statement>();
        List<ast.ParamDef> tmpParams = [];
        foreach (var item in ctx.Params)
        {
            var rr = Rewrite(item);
            tmpParams.Add((ast.ParamDef)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rrBody = Rewrite((AstThing)ctx.Body);
        prologue.AddRange(rrBody.Prologue);
        var rebuilt = ctx with {
         Params = tmpParams
        ,Body = (ast.BlockStatement)rrBody.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInferenceRuleDef(InferenceRuleDef ctx)
    {
        var prologue = new List<Statement>();
        var rrAntecedent = Rewrite((AstThing)ctx.Antecedent);
        prologue.AddRange(rrAntecedent.Prologue);
        var rrConsequent = Rewrite((AstThing)ctx.Consequent);
        prologue.AddRange(rrConsequent.Prologue);
        var rebuilt = ctx with {
         Antecedent = (ast.Expression)rrAntecedent.Node
        ,Consequent = (ast.KnowledgeManagementBlock)rrConsequent.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitParamDef(ParamDef ctx)
    {
        var prologue = new List<Statement>();
        var rrParameterConstraint = Rewrite((AstThing)ctx.ParameterConstraint);
        prologue.AddRange(rrParameterConstraint.Prologue);
        var rrDestructureDef = Rewrite((AstThing)ctx.DestructureDef);
        prologue.AddRange(rrDestructureDef.Prologue);
        var rebuilt = ctx with {
         ParameterConstraint = (ast.Expression)rrParameterConstraint.Node
        ,DestructureDef = (ast.ParamDestructureDef)rrDestructureDef.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        var prologue = new List<Statement>();
        List<ast.PropertyBindingDef> tmpBindings = [];
        foreach (var item in ctx.Bindings)
        {
            var rr = Rewrite(item);
            tmpBindings.Add((ast.PropertyBindingDef)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Bindings = tmpBindings
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
        var prologue = new List<Statement>();
        var rrReferencedProperty = Rewrite((AstThing)ctx.ReferencedProperty);
        prologue.AddRange(rrReferencedProperty.Prologue);
        var rrDestructureDef = Rewrite((AstThing)ctx.DestructureDef);
        prologue.AddRange(rrDestructureDef.Prologue);
        var rrConstraint = Rewrite((AstThing)ctx.Constraint);
        prologue.AddRange(rrConstraint.Prologue);
        var rebuilt = ctx with {
         ReferencedProperty = (ast.PropertyDef)rrReferencedProperty.Node
        ,DestructureDef = (ast.ParamDestructureDef)rrDestructureDef.Node
        ,Constraint = (ast.Expression)rrConstraint.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTypeDef(TypeDef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitClassDef(ClassDef ctx)
    {
        var prologue = new List<Statement>();
        List<ast.MemberDef> tmpMemberDefs = [];
        foreach (var item in ctx.MemberDefs)
        {
            var rr = Rewrite(item);
            tmpMemberDefs.Add((ast.MemberDef)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         MemberDefs = tmpMemberDefs
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVariableDecl(VariableDecl ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssemblyRef(AssemblyRef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMemberRef(MemberRef ctx)
    {
        var prologue = new List<Statement>();
        var rrMember = Rewrite((AstThing)ctx.Member);
        prologue.AddRange(rrMember.Prologue);
        var rebuilt = ctx with {
         Member = (ast.MemberDef)rrMember.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitPropertyRef(PropertyRef ctx)
    {
        var prologue = new List<Statement>();
        var rrProperty = Rewrite((AstThing)ctx.Property);
        prologue.AddRange(rrProperty.Prologue);
        var rebuilt = ctx with {
         Property = (ast.PropertyDef)rrProperty.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTypeRef(TypeRef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVarRef(VarRef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitGraphNamespaceAlias(GraphNamespaceAlias ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssignmentStatement(AssignmentStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrLValue = Rewrite((AstThing)ctx.LValue);
        prologue.AddRange(rrLValue.Prologue);
        var rrRValue = Rewrite((AstThing)ctx.RValue);
        prologue.AddRange(rrRValue.Prologue);
        var rebuilt = ctx with {
         LValue = (ast.Expression)rrLValue.Node
        ,RValue = (ast.Expression)rrRValue.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitBlockStatement(BlockStatement ctx)
    {
        // BlockStatement consumes prologue: splice hoisted statements into the block
        List<Statement> outStatements = [];
        foreach (var st in ctx.Statements)
        {
            var rr = Rewrite(st);
            outStatements.AddRange(rr.Prologue);
            outStatements.Add((Statement)rr.Node);
        }
        return new RewriteResult(ctx with { Statements = outStatements }, []);
    }
    public virtual RewriteResult VisitKnowledgeManagementBlock(KnowledgeManagementBlock ctx)
    {
        var prologue = new List<Statement>();
        List<ast.KnowledgeManagementStatement> tmpStatements = [];
        foreach (var item in ctx.Statements)
        {
            var rr = Rewrite(item);
            tmpStatements.Add((ast.KnowledgeManagementStatement)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Statements = tmpStatements
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitExpStatement(ExpStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrRHS = Rewrite((AstThing)ctx.RHS);
        prologue.AddRange(rrRHS.Prologue);
        var rebuilt = ctx with {
         RHS = (ast.Expression)rrRHS.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitEmptyStatement(EmptyStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitForStatement(ForStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrInitialValue = Rewrite((AstThing)ctx.InitialValue);
        prologue.AddRange(rrInitialValue.Prologue);
        var rrConstraint = Rewrite((AstThing)ctx.Constraint);
        prologue.AddRange(rrConstraint.Prologue);
        var rrIncrementExpression = Rewrite((AstThing)ctx.IncrementExpression);
        prologue.AddRange(rrIncrementExpression.Prologue);
        var rrLoopVariable = Rewrite((AstThing)ctx.LoopVariable);
        prologue.AddRange(rrLoopVariable.Prologue);
        var rrBody = Rewrite((AstThing)ctx.Body);
        prologue.AddRange(rrBody.Prologue);
        var rebuilt = ctx with {
         InitialValue = (ast.Expression)rrInitialValue.Node
        ,Constraint = (ast.Expression)rrConstraint.Node
        ,IncrementExpression = (ast.Expression)rrIncrementExpression.Node
        ,LoopVariable = (ast.VariableDecl)rrLoopVariable.Node
        ,Body = (ast.BlockStatement)rrBody.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitForeachStatement(ForeachStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrCollection = Rewrite((AstThing)ctx.Collection);
        prologue.AddRange(rrCollection.Prologue);
        var rrLoopVariable = Rewrite((AstThing)ctx.LoopVariable);
        prologue.AddRange(rrLoopVariable.Prologue);
        var rrBody = Rewrite((AstThing)ctx.Body);
        prologue.AddRange(rrBody.Prologue);
        var rebuilt = ctx with {
         Collection = (ast.Expression)rrCollection.Node
        ,LoopVariable = (ast.VariableDecl)rrLoopVariable.Node
        ,Body = (ast.BlockStatement)rrBody.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitGuardStatement(GuardStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrCondition = Rewrite((AstThing)ctx.Condition);
        prologue.AddRange(rrCondition.Prologue);
        var rebuilt = ctx with {
         Condition = (ast.Expression)rrCondition.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitIfElseStatement(IfElseStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrCondition = Rewrite((AstThing)ctx.Condition);
        prologue.AddRange(rrCondition.Prologue);
        var rrThenBlock = Rewrite((AstThing)ctx.ThenBlock);
        prologue.AddRange(rrThenBlock.Prologue);
        var rrElseBlock = Rewrite((AstThing)ctx.ElseBlock);
        prologue.AddRange(rrElseBlock.Prologue);
        var rebuilt = ctx with {
         Condition = (ast.Expression)rrCondition.Node
        ,ThenBlock = (ast.BlockStatement)rrThenBlock.Node
        ,ElseBlock = (ast.BlockStatement)rrElseBlock.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitReturnStatement(ReturnStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrReturnValue = Rewrite((AstThing)ctx.ReturnValue);
        prologue.AddRange(rrReturnValue.Prologue);
        var rebuilt = ctx with {
         ReturnValue = (ast.Expression)rrReturnValue.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVarDeclStatement(VarDeclStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrVariableDecl = Rewrite((AstThing)ctx.VariableDecl);
        prologue.AddRange(rrVariableDecl.Prologue);
        var rrInitialValue = Rewrite((AstThing)ctx.InitialValue);
        prologue.AddRange(rrInitialValue.Prologue);
        var rebuilt = ctx with {
         VariableDecl = (ast.VariableDecl)rrVariableDecl.Node
        ,InitialValue = (ast.Expression)rrInitialValue.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitWhileStatement(WhileStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrCondition = Rewrite((AstThing)ctx.Condition);
        prologue.AddRange(rrCondition.Prologue);
        var rrBody = Rewrite((AstThing)ctx.Body);
        prologue.AddRange(rrBody.Prologue);
        var rebuilt = ctx with {
         Condition = (ast.Expression)rrCondition.Node
        ,Body = (ast.BlockStatement)rrBody.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTryStatement(TryStatement ctx)
    {
        var prologue = new List<Statement>();
        List<ast.CatchClause> tmpCatchClauses = [];
        foreach (var item in ctx.CatchClauses)
        {
            var rr = Rewrite(item);
            tmpCatchClauses.Add((ast.CatchClause)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rrTryBlock = Rewrite((AstThing)ctx.TryBlock);
        prologue.AddRange(rrTryBlock.Prologue);
        var rrFinallyBlock = Rewrite((AstThing)ctx.FinallyBlock);
        prologue.AddRange(rrFinallyBlock.Prologue);
        var rebuilt = ctx with {
         TryBlock = (ast.BlockStatement)rrTryBlock.Node
        ,CatchClauses = tmpCatchClauses
        ,FinallyBlock = (ast.BlockStatement)rrFinallyBlock.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitCatchClause(CatchClause ctx)
    {
        var prologue = new List<Statement>();
        var rrFilter = Rewrite((AstThing)ctx.Filter);
        prologue.AddRange(rrFilter.Prologue);
        var rrBody = Rewrite((AstThing)ctx.Body);
        prologue.AddRange(rrBody.Prologue);
        var rebuilt = ctx with {
         Filter = (ast.Expression)rrFilter.Node
        ,Body = (ast.BlockStatement)rrBody.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitThrowStatement(ThrowStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrException = Rewrite((AstThing)ctx.Exception);
        prologue.AddRange(rrException.Prologue);
        var rebuilt = ctx with {
         Exception = (ast.Expression)rrException.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssertionStatement(AssertionStatement ctx)
    {
        var prologue = new List<Statement>();
        var rrAssertion = Rewrite((AstThing)ctx.Assertion);
        prologue.AddRange(rrAssertion.Prologue);
        var rrAssertionSubject = Rewrite((AstThing)ctx.AssertionSubject);
        prologue.AddRange(rrAssertionSubject.Prologue);
        var rrAssertionPredicate = Rewrite((AstThing)ctx.AssertionPredicate);
        prologue.AddRange(rrAssertionPredicate.Prologue);
        var rrAssertionObject = Rewrite((AstThing)ctx.AssertionObject);
        prologue.AddRange(rrAssertionObject.Prologue);
        var rebuilt = ctx with {
         Assertion = (ast.TripleLiteralExp)rrAssertion.Node
        ,AssertionSubject = (ast.AssertionSubject)rrAssertionSubject.Node
        ,AssertionPredicate = (ast.AssertionPredicate)rrAssertionPredicate.Node
        ,AssertionObject = (ast.AssertionObject)rrAssertionObject.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssertionObject(AssertionObject ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssertionPredicate(AssertionPredicate ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssertionSubject(AssertionSubject ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitRetractionStatement(RetractionStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitWithScopeStatement(WithScopeStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitBinaryExp(BinaryExp ctx)
    {
        var prologue = new List<Statement>();
        var rrLHS = Rewrite((AstThing)ctx.LHS);
        prologue.AddRange(rrLHS.Prologue);
        var rrRHS = Rewrite((AstThing)ctx.RHS);
        prologue.AddRange(rrRHS.Prologue);
        var rebuilt = ctx with {
         LHS = (ast.Expression)rrLHS.Node
        ,RHS = (ast.Expression)rrRHS.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitCastExp(CastExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitLambdaExp(LambdaExp ctx)
    {
        var prologue = new List<Statement>();
        var rrFunctorDef = Rewrite((AstThing)ctx.FunctorDef);
        prologue.AddRange(rrFunctorDef.Prologue);
        var rebuilt = ctx with {
         FunctorDef = (ast.FunctorDef)rrFunctorDef.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFuncCallExp(FuncCallExp ctx)
    {
        var prologue = new List<Statement>();
        List<ast.Expression> tmpInvocationArguments = [];
        foreach (var item in ctx.InvocationArguments)
        {
            var rr = Rewrite(item);
            tmpInvocationArguments.Add((ast.Expression)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rrFunctionDef = Rewrite((AstThing)ctx.FunctionDef);
        prologue.AddRange(rrFunctionDef.Prologue);
        var rebuilt = ctx with {
         FunctionDef = (ast.FunctionDef)rrFunctionDef.Node
        ,InvocationArguments = tmpInvocationArguments
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInt8LiteralExp(Int8LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInt16LiteralExp(Int16LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInt32LiteralExp(Int32LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInt64LiteralExp(Int64LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFloat4LiteralExp(Float4LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFloat8LiteralExp(Float8LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFloat16LiteralExp(Float16LiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitBooleanLiteralExp(BooleanLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitCharLiteralExp(CharLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitStringLiteralExp(StringLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitDateLiteralExp(DateLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTimeLiteralExp(TimeLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitDateTimeLiteralExp(DateTimeLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitDurationLiteralExp(DurationLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitUriLiteralExp(UriLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAtomLiteralExp(AtomLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTriGLiteralExpression(TriGLiteralExpression ctx)
    {
        var prologue = new List<Statement>();
        List<ast.InterpolatedExpression> tmpInterpolations = [];
        foreach (var item in ctx.Interpolations)
        {
            var rr = Rewrite(item);
            tmpInterpolations.Add((ast.InterpolatedExpression)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Interpolations = tmpInterpolations
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInterpolatedExpression(InterpolatedExpression ctx)
    {
        var prologue = new List<Statement>();
        var rrExpression = Rewrite((AstThing)ctx.Expression);
        prologue.AddRange(rrExpression.Prologue);
        var rebuilt = ctx with {
         Expression = (ast.Expression)rrExpression.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitSparqlLiteralExpression(SparqlLiteralExpression ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVariableBinding(VariableBinding ctx)
    {
        var prologue = new List<Statement>();
        var rrResolvedExpression = Rewrite((AstThing)ctx.ResolvedExpression);
        prologue.AddRange(rrResolvedExpression.Prologue);
        var rebuilt = ctx with {
         ResolvedExpression = (ast.Expression)rrResolvedExpression.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInterpolation(Interpolation ctx)
    {
        var prologue = new List<Statement>();
        var rrExpression = Rewrite((AstThing)ctx.Expression);
        prologue.AddRange(rrExpression.Prologue);
        var rebuilt = ctx with {
         Expression = (ast.Expression)rrExpression.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMemberAccessExp(MemberAccessExp ctx)
    {
        var prologue = new List<Statement>();
        var rrLHS = Rewrite((AstThing)ctx.LHS);
        prologue.AddRange(rrLHS.Prologue);
        var rrRHS = Rewrite((AstThing)ctx.RHS);
        prologue.AddRange(rrRHS.Prologue);
        var rebuilt = ctx with {
         LHS = (ast.Expression)rrLHS.Node
        ,RHS = (ast.Expression)rrRHS.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitIndexerExpression(IndexerExpression ctx)
    {
        var prologue = new List<Statement>();
        var rrIndexExpression = Rewrite((AstThing)ctx.IndexExpression);
        prologue.AddRange(rrIndexExpression.Prologue);
        var rrOffsetExpression = Rewrite((AstThing)ctx.OffsetExpression);
        prologue.AddRange(rrOffsetExpression.Prologue);
        var rebuilt = ctx with {
         IndexExpression = (ast.Expression)rrIndexExpression.Node
        ,OffsetExpression = (ast.Expression)rrOffsetExpression.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitObjectInitializerExp(ObjectInitializerExp ctx)
    {
        var prologue = new List<Statement>();
        List<ast.PropertyInitializerExp> tmpPropertyInitialisers = [];
        foreach (var item in ctx.PropertyInitialisers)
        {
            var rr = Rewrite(item);
            tmpPropertyInitialisers.Add((ast.PropertyInitializerExp)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         PropertyInitialisers = tmpPropertyInitialisers
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitPropertyInitializerExp(PropertyInitializerExp ctx)
    {
        var prologue = new List<Statement>();
        var rrPropertyToInitialize = Rewrite((AstThing)ctx.PropertyToInitialize);
        prologue.AddRange(rrPropertyToInitialize.Prologue);
        var rrRHS = Rewrite((AstThing)ctx.RHS);
        prologue.AddRange(rrRHS.Prologue);
        var rebuilt = ctx with {
         PropertyToInitialize = (ast.PropertyRef)rrPropertyToInitialize.Node
        ,RHS = (ast.Expression)rrRHS.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitUnaryExp(UnaryExp ctx)
    {
        var prologue = new List<Statement>();
        var rrOperand = Rewrite((AstThing)ctx.Operand);
        prologue.AddRange(rrOperand.Prologue);
        var rebuilt = ctx with {
         Operand = (ast.Expression)rrOperand.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitThrowExp(ThrowExp ctx)
    {
        var prologue = new List<Statement>();
        var rrException = Rewrite((AstThing)ctx.Exception);
        prologue.AddRange(rrException.Prologue);
        var rebuilt = ctx with {
         Exception = (ast.Expression)rrException.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVarRefExp(VarRefExp ctx)
    {
        var prologue = new List<Statement>();
        var rrVariableDecl = Rewrite((AstThing)ctx.VariableDecl);
        prologue.AddRange(rrVariableDecl.Prologue);
        var rebuilt = ctx with {
         VariableDecl = (ast.VariableDecl)rrVariableDecl.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitListLiteral(ListLiteral ctx)
    {
        var prologue = new List<Statement>();
        List<ast.Expression> tmpElementExpressions = [];
        foreach (var item in ctx.ElementExpressions)
        {
            var rr = Rewrite(item);
            tmpElementExpressions.Add((ast.Expression)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         ElementExpressions = tmpElementExpressions
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitListComprehension(ListComprehension ctx)
    {
        var prologue = new List<Statement>();
        var rrMembershipConstraint = Rewrite((AstThing)ctx.MembershipConstraint);
        prologue.AddRange(rrMembershipConstraint.Prologue);
        var rebuilt = ctx with {
         MembershipConstraint = (ast.Expression)rrMembershipConstraint.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAtom(Atom ctx)
    {
        var prologue = new List<Statement>();
        var rrAtomExp = Rewrite((AstThing)ctx.AtomExp);
        prologue.AddRange(rrAtomExp.Prologue);
        var rebuilt = ctx with {
         AtomExp = (ast.AtomLiteralExp)rrAtomExp.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTripleLiteralExp(TripleLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rrSubjectExp = Rewrite((AstThing)ctx.SubjectExp);
        prologue.AddRange(rrSubjectExp.Prologue);
        var rrPredicateExp = Rewrite((AstThing)ctx.PredicateExp);
        prologue.AddRange(rrPredicateExp.Prologue);
        var rrObjectExp = Rewrite((AstThing)ctx.ObjectExp);
        prologue.AddRange(rrObjectExp.Prologue);
        var rebuilt = ctx with {
         SubjectExp = (ast.UriLiteralExp)rrSubjectExp.Node
        ,PredicateExp = (ast.UriLiteralExp)rrPredicateExp.Node
        ,ObjectExp = (ast.Expression)rrObjectExp.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMalformedTripleExp(MalformedTripleExp ctx)
    {
        var prologue = new List<Statement>();
        List<ast.Expression> tmpComponents = [];
        foreach (var item in ctx.Components)
        {
            var rr = Rewrite(item);
            tmpComponents.Add((ast.Expression)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Components = tmpComponents
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitGraph(Graph ctx)
    {
        var prologue = new List<Statement>();
        List<ast.TripleLiteralExp> tmpTriples = [];
        foreach (var item in ctx.Triples)
        {
            var rr = Rewrite(item);
            tmpTriples.Add((ast.TripleLiteralExp)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rrGraphUri = Rewrite((AstThing)ctx.GraphUri);
        prologue.AddRange(rrGraphUri.Prologue);
        var rebuilt = ctx with {
         GraphUri = (ast.UriLiteralExp)rrGraphUri.Node
        ,Triples = tmpTriples
        };
        return new RewriteResult(rebuilt, prologue);
    }

}
