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
    RewriteResult VisitForStatement(ForStatement ctx);
    RewriteResult VisitForeachStatement(ForeachStatement ctx);
    RewriteResult VisitGuardStatement(GuardStatement ctx);
    RewriteResult VisitIfElseStatement(IfElseStatement ctx);
    RewriteResult VisitReturnStatement(ReturnStatement ctx);
    RewriteResult VisitVarDeclStatement(VarDeclStatement ctx);
    RewriteResult VisitWhileStatement(WhileStatement ctx);
    RewriteResult VisitGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx);
    RewriteResult VisitAssertionStatement(AssertionStatement ctx);
    RewriteResult VisitAssertionObject(AssertionObject ctx);
    RewriteResult VisitAssertionPredicate(AssertionPredicate ctx);
    RewriteResult VisitAssertionSubject(AssertionSubject ctx);
    RewriteResult VisitRetractionStatement(RetractionStatement ctx);
    RewriteResult VisitWithScopeStatement(WithScopeStatement ctx);
    RewriteResult VisitGraphAssertionBlockExp(GraphAssertionBlockExp ctx);
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
    RewriteResult VisitMemberAccessExp(MemberAccessExp ctx);
    RewriteResult VisitIndexerExpression(IndexerExpression ctx);
    RewriteResult VisitObjectInitializerExp(ObjectInitializerExp ctx);
    RewriteResult VisitPropertyInitializerExp(PropertyInitializerExp ctx);
    RewriteResult VisitUnaryExp(UnaryExp ctx);
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
             ForStatement node => VisitForStatement(node),
             ForeachStatement node => VisitForeachStatement(node),
             GuardStatement node => VisitGuardStatement(node),
             IfElseStatement node => VisitIfElseStatement(node),
             ReturnStatement node => VisitReturnStatement(node),
             VarDeclStatement node => VisitVarDeclStatement(node),
             WhileStatement node => VisitWhileStatement(node),
             GraphAssertionBlockStatement node => VisitGraphAssertionBlockStatement(node),
             AssertionStatement node => VisitAssertionStatement(node),
             AssertionObject node => VisitAssertionObject(node),
             AssertionPredicate node => VisitAssertionPredicate(node),
             AssertionSubject node => VisitAssertionSubject(node),
             RetractionStatement node => VisitRetractionStatement(node),
             WithScopeStatement node => VisitWithScopeStatement(node),
             GraphAssertionBlockExp node => VisitGraphAssertionBlockExp(node),
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
             MemberAccessExp node => VisitMemberAccessExp(node),
             IndexerExpression node => VisitIndexerExpression(node),
             ObjectInitializerExp node => VisitObjectInitializerExp(node),
             PropertyInitializerExp node => VisitPropertyInitializerExp(node),
             UnaryExp node => VisitUnaryExp(node),
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
        var rebuilt = ctx with {
         Params = tmpParams
        ,Body = (ast.BlockStatement)Rewrite((AstThing)ctx.Body).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFunctorDef(FunctorDef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         InvocationFuncDev = (ast.FunctionDef)Rewrite((AstThing)ctx.InvocationFuncDev).Node
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
        var rebuilt = ctx with {
         BackingField = (ast.FieldDef)Rewrite((AstThing)ctx.BackingField).Node
        ,Getter = (ast.MethodDef)Rewrite((AstThing)ctx.Getter).Node
        ,Setter = (ast.MethodDef)Rewrite((AstThing)ctx.Setter).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMethodDef(MethodDef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         FunctionDef = (ast.FunctionDef)Rewrite((AstThing)ctx.FunctionDef).Node
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
        var rebuilt = ctx with {
         Params = tmpParams
        ,Body = (ast.BlockStatement)Rewrite((AstThing)ctx.Body).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInferenceRuleDef(InferenceRuleDef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Antecedent = (ast.Expression)Rewrite((AstThing)ctx.Antecedent).Node
        ,Consequent = (ast.KnowledgeManagementBlock)Rewrite((AstThing)ctx.Consequent).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitParamDef(ParamDef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         ParameterConstraint = (ast.Expression)Rewrite((AstThing)ctx.ParameterConstraint).Node
        ,DestructureDef = (ast.ParamDestructureDef)Rewrite((AstThing)ctx.DestructureDef).Node
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
        var rebuilt = ctx with {
         ReferencedProperty = (ast.PropertyDef)Rewrite((AstThing)ctx.ReferencedProperty).Node
        ,DestructureDef = (ast.ParamDestructureDef)Rewrite((AstThing)ctx.DestructureDef).Node
        ,Constraint = (ast.Expression)Rewrite((AstThing)ctx.Constraint).Node
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
        var rebuilt = ctx with {
         Member = (ast.MemberDef)Rewrite((AstThing)ctx.Member).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitPropertyRef(PropertyRef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Property = (ast.PropertyDef)Rewrite((AstThing)ctx.Property).Node
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
        var rebuilt = ctx with {
         LValue = (ast.Expression)Rewrite((AstThing)ctx.LValue).Node
        ,RValue = (ast.Expression)Rewrite((AstThing)ctx.RValue).Node
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
        var rebuilt = ctx with {
         RHS = (ast.Expression)Rewrite((AstThing)ctx.RHS).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitForStatement(ForStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         InitialValue = (ast.Expression)Rewrite((AstThing)ctx.InitialValue).Node
        ,Constraint = (ast.Expression)Rewrite((AstThing)ctx.Constraint).Node
        ,IncrementExpression = (ast.Expression)Rewrite((AstThing)ctx.IncrementExpression).Node
        ,LoopVariable = (ast.VariableDecl)Rewrite((AstThing)ctx.LoopVariable).Node
        ,Body = (ast.BlockStatement)Rewrite((AstThing)ctx.Body).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitForeachStatement(ForeachStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Collection = (ast.Expression)Rewrite((AstThing)ctx.Collection).Node
        ,LoopVariable = (ast.VariableDecl)Rewrite((AstThing)ctx.LoopVariable).Node
        ,Body = (ast.BlockStatement)Rewrite((AstThing)ctx.Body).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitGuardStatement(GuardStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Condition = (ast.Expression)Rewrite((AstThing)ctx.Condition).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitIfElseStatement(IfElseStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Condition = (ast.Expression)Rewrite((AstThing)ctx.Condition).Node
        ,ThenBlock = (ast.BlockStatement)Rewrite((AstThing)ctx.ThenBlock).Node
        ,ElseBlock = (ast.BlockStatement)Rewrite((AstThing)ctx.ElseBlock).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitReturnStatement(ReturnStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         ReturnValue = (ast.Expression)Rewrite((AstThing)ctx.ReturnValue).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVarDeclStatement(VarDeclStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         VariableDecl = (ast.VariableDecl)Rewrite((AstThing)ctx.VariableDecl).Node
        ,InitialValue = (ast.Expression)Rewrite((AstThing)ctx.InitialValue).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitWhileStatement(WhileStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Condition = (ast.Expression)Rewrite((AstThing)ctx.Condition).Node
        ,Body = (ast.BlockStatement)Rewrite((AstThing)ctx.Body).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Content = (ast.GraphAssertionBlockExp)Rewrite((AstThing)ctx.Content).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssertionStatement(AssertionStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Assertion = (ast.TripleLiteralExp)Rewrite((AstThing)ctx.Assertion).Node
        ,AssertionSubject = (ast.AssertionSubject)Rewrite((AstThing)ctx.AssertionSubject).Node
        ,AssertionPredicate = (ast.AssertionPredicate)Rewrite((AstThing)ctx.AssertionPredicate).Node
        ,AssertionObject = (ast.AssertionObject)Rewrite((AstThing)ctx.AssertionObject).Node
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
    public virtual RewriteResult VisitGraphAssertionBlockExp(GraphAssertionBlockExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Content = (ast.BlockStatement)Rewrite((AstThing)ctx.Content).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitBinaryExp(BinaryExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         LHS = (ast.Expression)Rewrite((AstThing)ctx.LHS).Node
        ,RHS = (ast.Expression)Rewrite((AstThing)ctx.RHS).Node
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
        var rebuilt = ctx with {
         FunctorDef = (ast.FunctorDef)Rewrite((AstThing)ctx.FunctorDef).Node
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
        var rebuilt = ctx with {
         FunctionDef = (ast.FunctionDef)Rewrite((AstThing)ctx.FunctionDef).Node
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
    public virtual RewriteResult VisitMemberAccessExp(MemberAccessExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         LHS = (ast.Expression)Rewrite((AstThing)ctx.LHS).Node
        ,RHS = (ast.Expression)Rewrite((AstThing)ctx.RHS).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitIndexerExpression(IndexerExpression ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         IndexExpression = (ast.Expression)Rewrite((AstThing)ctx.IndexExpression).Node
        ,OffsetExpression = (ast.Expression)Rewrite((AstThing)ctx.OffsetExpression).Node
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
        var rebuilt = ctx with {
         PropertyToInitialize = (ast.PropertyRef)Rewrite((AstThing)ctx.PropertyToInitialize).Node
        ,RHS = (ast.Expression)Rewrite((AstThing)ctx.RHS).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitUnaryExp(UnaryExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Operand = (ast.Expression)Rewrite((AstThing)ctx.Operand).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVarRefExp(VarRefExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         VariableDecl = (ast.VariableDecl)Rewrite((AstThing)ctx.VariableDecl).Node
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
        var rebuilt = ctx with {
         MembershipConstraint = (ast.Expression)Rewrite((AstThing)ctx.MembershipConstraint).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAtom(Atom ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         AtomExp = (ast.AtomLiteralExp)Rewrite((AstThing)ctx.AtomExp).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTripleLiteralExp(TripleLiteralExp ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         SubjectExp = (ast.UriLiteralExp)Rewrite((AstThing)ctx.SubjectExp).Node
        ,PredicateExp = (ast.UriLiteralExp)Rewrite((AstThing)ctx.PredicateExp).Node
        ,ObjectExp = (ast.Expression)Rewrite((AstThing)ctx.ObjectExp).Node
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
        var rebuilt = ctx with {
         GraphUri = (ast.UriLiteralExp)Rewrite((AstThing)ctx.GraphUri).Node
        ,Triples = tmpTriples
        };
        return new RewriteResult(rebuilt, prologue);
    }

}
