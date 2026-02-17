// ReSharper disable InconsistentNaming

namespace Fifth.LangProcessingPhases;

public class DumpTreeVisitor : DefaultRecursiveDescentVisitor
{
    private readonly TextWriter tw;
    private int depth = 0;

    public DumpTreeVisitor(TextWriter tw)
    {
        this.tw = tw;
    }

    private void EnterNonTerminal<T>(T ctx, string value) where T : AstThing
    {
        var displayName = ctx.GetType().GetProperty("Name")?.GetValue(ctx, null) ?? ctx.GetType().Name;
        tw.WriteLine($"{new string(' ', 2 * depth)} {value}: {displayName}");
        depth++;
    }

    private void EnterTerminal<T>(T ctx, string value) where T : AstThing
    {
        string literalValueOrTypeName = ctx switch
        {
            Int32LiteralExp int32Literal => int32Literal.Value.ToString(),
            BooleanLiteralExp booleanLiteral => booleanLiteral.Value.ToString(),
            CharLiteralExp charLiteral => charLiteral.Value.ToString(),
            StringLiteralExp stringLiteral => stringLiteral.Value,
            UriLiteralExp uriLiteral => uriLiteral.Value.ToString(),
            DateTimeLiteralExp dateTimeLiteral => dateTimeLiteral.Value.ToString(),
            Int8LiteralExp int8Literal => int8Literal.Value.ToString(),
            UnsignedInt8LiteralExp uint8Literal => uint8Literal.Value.ToString(),
            Int16LiteralExp int16Literal => int16Literal.Value.ToString(),
            UnsignedInt16LiteralExp uint16Literal => uint16Literal.Value.ToString(),
            Int64LiteralExp int64Literal => int64Literal.Value.ToString(),
            UnsignedInt64LiteralExp uint64Literal => uint64Literal.Value.ToString(),
            Float4LiteralExp float4Literal => float4Literal.Value.ToString(),
            Float8LiteralExp float8Literal => float8Literal.Value.ToString(),
            Float16LiteralExp float16Literal => float16Literal.Value.ToString(),
            _ => ctx.GetType().Name
        };
        tw.WriteLine($"{new string(' ', (2 * depth) + 1)} . {value}: {literalValueOrTypeName}");
    }

    private void LeaveNonTerminal<T>(T ctx) where T : AstThing
    {
        depth--;
    }

    private void LeaveTerminal<T>(T _) where T : AstThing
    {
    }

    public override AstThing Visit(AstThing ctx)
    {
        if (ctx == null) return ctx;
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.Visit(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitAssemblyDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override AssemblyRef VisitAssemblyRef(AssemblyRef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitAssemblyRef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitModuleDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitClassDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitMemberAccessExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitParamDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override PropertyDef VisitPropertyDef(PropertyDef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitPropertyDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override MethodDef VisitMethodDef(MethodDef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitMethodDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override TypeRef VisitTypeRef(TypeRef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitTypeRef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override MemberRef VisitMemberRef(MemberRef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitMemberRef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override FieldDef VisitFieldDef(FieldDef ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitFieldDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override BlockStatement VisitBlockStatement(BlockStatement ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitBlockStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override IfElseStatement VisitIfElseStatement(IfElseStatement ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitIfElseStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override AssignmentStatement VisitAssignmentStatement(AssignmentStatement ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitAssignmentStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override VarDeclStatement VisitVarDeclStatement(VarDeclStatement ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitVarDeclStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ReturnStatement VisitReturnStatement(ReturnStatement ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitReturnStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override WhileStatement VisitWhileStatement(WhileStatement ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitWhileStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ExpStatement VisitExpStatement(ExpStatement ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitExpStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override UnaryExp VisitUnaryExp(UnaryExp ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitUnaryExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitBinaryExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override VarRefExp VisitVarRefExp(VarRefExp ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitVarRefExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override CastExp VisitCastExp(CastExp ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitCastExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
    {
        EnterNonTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitFuncCallExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override BooleanLiteralExp VisitBooleanLiteralExp(BooleanLiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitBooleanLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override CharLiteralExp VisitCharLiteralExp(CharLiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitCharLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override StringLiteralExp VisitStringLiteralExp(StringLiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitStringLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UriLiteralExp VisitUriLiteralExp(UriLiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitUriLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override DateTimeLiteralExp VisitDateTimeLiteralExp(DateTimeLiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitDateTimeLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Int8LiteralExp VisitInt8LiteralExp(Int8LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitInt8LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnsignedInt8LiteralExp VisitUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitUnsignedInt8LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Int16LiteralExp VisitInt16LiteralExp(Int16LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitInt16LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnsignedInt16LiteralExp VisitUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitUnsignedInt16LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Int32LiteralExp VisitInt32LiteralExp(Int32LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitInt32LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnsignedInt32LiteralExp VisitUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitUnsignedInt32LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Int64LiteralExp VisitInt64LiteralExp(Int64LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitInt64LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnsignedInt64LiteralExp VisitUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitUnsignedInt64LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Float4LiteralExp VisitFloat4LiteralExp(Float4LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitFloat4LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Float8LiteralExp VisitFloat8LiteralExp(Float8LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitFloat8LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Float16LiteralExp VisitFloat16LiteralExp(Float16LiteralExp ctx)
    {
        EnterTerminal(ctx, ctx.GetType().Name);
        var result = base.VisitFloat16LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }
}
