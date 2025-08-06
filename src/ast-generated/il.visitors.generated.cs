namespace il_ast_generated;
using il_ast;
using System.Collections.Generic;

public interface IAstVisitor
{
    public void EnterMethodSignature(MethodSignature ctx);
    public void LeaveMethodSignature(MethodSignature ctx);
    public void EnterMemberRef(MemberRef ctx);
    public void LeaveMemberRef(MemberRef ctx);
    public void EnterSByteLiteral(SByteLiteral ctx);
    public void LeaveSByteLiteral(SByteLiteral ctx);
    public void EnterFloatLiteral(FloatLiteral ctx);
    public void LeaveFloatLiteral(FloatLiteral ctx);
}

public partial class BaseAstVisitor : IAstVisitor
{
    public virtual void EnterMethodSignature(MethodSignature ctx) { }
    public virtual void LeaveMethodSignature(MethodSignature ctx) { }
    public virtual void EnterMemberRef(MemberRef ctx) { }
    public virtual void LeaveMemberRef(MemberRef ctx) { }
    public virtual void EnterSByteLiteral(SByteLiteral ctx) { }
    public virtual void LeaveSByteLiteral(SByteLiteral ctx) { }
    public virtual void EnterFloatLiteral(FloatLiteral ctx) { }
    public virtual void LeaveFloatLiteral(FloatLiteral ctx) { }
}
