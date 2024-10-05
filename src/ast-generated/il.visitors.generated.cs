

namespace il_ast_generated;
using il_ast;
using System.Collections.Generic;

public interface IAstVisitor
{
    public void EnterAssemblyDeclaration(AssemblyDeclaration ctx);
    public void LeaveAssemblyDeclaration(AssemblyDeclaration ctx);
    public void EnterAssemblyReference(AssemblyReference ctx);
    public void LeaveAssemblyReference(AssemblyReference ctx);
    public void EnterModuleDeclaration(ModuleDeclaration ctx);
    public void LeaveModuleDeclaration(ModuleDeclaration ctx);
    public void EnterVersion(Version ctx);
    public void LeaveVersion(Version ctx);
    public void EnterClassDefinition(ClassDefinition ctx);
    public void LeaveClassDefinition(ClassDefinition ctx);
    public void EnterMemberAccessExpression(MemberAccessExpression ctx);
    public void LeaveMemberAccessExpression(MemberAccessExpression ctx);
    public void EnterParameterDeclaration(ParameterDeclaration ctx);
    public void LeaveParameterDeclaration(ParameterDeclaration ctx);
    public void EnterParameterSignature(ParameterSignature ctx);
    public void LeaveParameterSignature(ParameterSignature ctx);
    public void EnterMethodSignature(MethodSignature ctx);
    public void LeaveMethodSignature(MethodSignature ctx);
    public void EnterMethodHeader(MethodHeader ctx);
    public void LeaveMethodHeader(MethodHeader ctx);
    public void EnterMethodRef(MethodRef ctx);
    public void LeaveMethodRef(MethodRef ctx);
    public void EnterMethodImpl(MethodImpl ctx);
    public void LeaveMethodImpl(MethodImpl ctx);
    public void EnterMethodDefinition(MethodDefinition ctx);
    public void LeaveMethodDefinition(MethodDefinition ctx);
    public void EnterTypeReference(TypeReference ctx);
    public void LeaveTypeReference(TypeReference ctx);
    public void EnterMemberRef(MemberRef ctx);
    public void LeaveMemberRef(MemberRef ctx);
    public void EnterFieldDefinition(FieldDefinition ctx);
    public void LeaveFieldDefinition(FieldDefinition ctx);
    public void EnterPropertyDefinition(PropertyDefinition ctx);
    public void LeavePropertyDefinition(PropertyDefinition ctx);
    public void EnterBlock(Block ctx);
    public void LeaveBlock(Block ctx);
    public void EnterIfStatement(IfStatement ctx);
    public void LeaveIfStatement(IfStatement ctx);
    public void EnterVariableAssignmentStatement(VariableAssignmentStatement ctx);
    public void LeaveVariableAssignmentStatement(VariableAssignmentStatement ctx);
    public void EnterVariableDeclarationStatement(VariableDeclarationStatement ctx);
    public void LeaveVariableDeclarationStatement(VariableDeclarationStatement ctx);
    public void EnterReturnStatement(ReturnStatement ctx);
    public void LeaveReturnStatement(ReturnStatement ctx);
    public void EnterWhileStatement(WhileStatement ctx);
    public void LeaveWhileStatement(WhileStatement ctx);
    public void EnterExpressionStatement(ExpressionStatement ctx);
    public void LeaveExpressionStatement(ExpressionStatement ctx);
    public void EnterUnaryExpression(UnaryExpression ctx);
    public void LeaveUnaryExpression(UnaryExpression ctx);
    public void EnterBinaryExpression(BinaryExpression ctx);
    public void LeaveBinaryExpression(BinaryExpression ctx);
    public void EnterVariableReferenceExpression(VariableReferenceExpression ctx);
    public void LeaveVariableReferenceExpression(VariableReferenceExpression ctx);
    public void EnterTypeCastExpression(TypeCastExpression ctx);
    public void LeaveTypeCastExpression(TypeCastExpression ctx);
    public void EnterFuncCallExp(FuncCallExp ctx);
    public void LeaveFuncCallExp(FuncCallExp ctx);
    public void EnterBoolLiteral(BoolLiteral ctx);
    public void LeaveBoolLiteral(BoolLiteral ctx);
    public void EnterCharLiteral(CharLiteral ctx);
    public void LeaveCharLiteral(CharLiteral ctx);
    public void EnterStringLiteral(StringLiteral ctx);
    public void LeaveStringLiteral(StringLiteral ctx);
    public void EnterUriLiteral(UriLiteral ctx);
    public void LeaveUriLiteral(UriLiteral ctx);
    public void EnterDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx);
    public void LeaveDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx);
    public void EnterDateOnlyLiteral(DateOnlyLiteral ctx);
    public void LeaveDateOnlyLiteral(DateOnlyLiteral ctx);
    public void EnterTimeOnlyLiteral(TimeOnlyLiteral ctx);
    public void LeaveTimeOnlyLiteral(TimeOnlyLiteral ctx);
    public void EnterSByteLiteral(SByteLiteral ctx);
    public void LeaveSByteLiteral(SByteLiteral ctx);
    public void EnterByteLiteral(ByteLiteral ctx);
    public void LeaveByteLiteral(ByteLiteral ctx);
    public void EnterShortLiteral(ShortLiteral ctx);
    public void LeaveShortLiteral(ShortLiteral ctx);
    public void EnterUShortLiteral(UShortLiteral ctx);
    public void LeaveUShortLiteral(UShortLiteral ctx);
    public void EnterIntLiteral(IntLiteral ctx);
    public void LeaveIntLiteral(IntLiteral ctx);
    public void EnterUIntLiteral(UIntLiteral ctx);
    public void LeaveUIntLiteral(UIntLiteral ctx);
    public void EnterLongLiteral(LongLiteral ctx);
    public void LeaveLongLiteral(LongLiteral ctx);
    public void EnterULongLiteral(ULongLiteral ctx);
    public void LeaveULongLiteral(ULongLiteral ctx);
    public void EnterFloatLiteral(FloatLiteral ctx);
    public void LeaveFloatLiteral(FloatLiteral ctx);
    public void EnterDoubleLiteral(DoubleLiteral ctx);
    public void LeaveDoubleLiteral(DoubleLiteral ctx);
    public void EnterDecimalLiteral(DecimalLiteral ctx);
    public void LeaveDecimalLiteral(DecimalLiteral ctx);
}

public partial class BaseAstVisitor : IAstVisitor
{
    public virtual void EnterAssemblyDeclaration(AssemblyDeclaration ctx){}
    public virtual void LeaveAssemblyDeclaration(AssemblyDeclaration ctx){}
    public virtual void EnterAssemblyReference(AssemblyReference ctx){}
    public virtual void LeaveAssemblyReference(AssemblyReference ctx){}
    public virtual void EnterModuleDeclaration(ModuleDeclaration ctx){}
    public virtual void LeaveModuleDeclaration(ModuleDeclaration ctx){}
    public virtual void EnterVersion(Version ctx){}
    public virtual void LeaveVersion(Version ctx){}
    public virtual void EnterClassDefinition(ClassDefinition ctx){}
    public virtual void LeaveClassDefinition(ClassDefinition ctx){}
    public virtual void EnterMemberAccessExpression(MemberAccessExpression ctx){}
    public virtual void LeaveMemberAccessExpression(MemberAccessExpression ctx){}
    public virtual void EnterParameterDeclaration(ParameterDeclaration ctx){}
    public virtual void LeaveParameterDeclaration(ParameterDeclaration ctx){}
    public virtual void EnterParameterSignature(ParameterSignature ctx){}
    public virtual void LeaveParameterSignature(ParameterSignature ctx){}
    public virtual void EnterMethodSignature(MethodSignature ctx){}
    public virtual void LeaveMethodSignature(MethodSignature ctx){}
    public virtual void EnterMethodHeader(MethodHeader ctx){}
    public virtual void LeaveMethodHeader(MethodHeader ctx){}
    public virtual void EnterMethodRef(MethodRef ctx){}
    public virtual void LeaveMethodRef(MethodRef ctx){}
    public virtual void EnterMethodImpl(MethodImpl ctx){}
    public virtual void LeaveMethodImpl(MethodImpl ctx){}
    public virtual void EnterMethodDefinition(MethodDefinition ctx){}
    public virtual void LeaveMethodDefinition(MethodDefinition ctx){}
    public virtual void EnterTypeReference(TypeReference ctx){}
    public virtual void LeaveTypeReference(TypeReference ctx){}
    public virtual void EnterMemberRef(MemberRef ctx){}
    public virtual void LeaveMemberRef(MemberRef ctx){}
    public virtual void EnterFieldDefinition(FieldDefinition ctx){}
    public virtual void LeaveFieldDefinition(FieldDefinition ctx){}
    public virtual void EnterPropertyDefinition(PropertyDefinition ctx){}
    public virtual void LeavePropertyDefinition(PropertyDefinition ctx){}
    public virtual void EnterBlock(Block ctx){}
    public virtual void LeaveBlock(Block ctx){}
    public virtual void EnterIfStatement(IfStatement ctx){}
    public virtual void LeaveIfStatement(IfStatement ctx){}
    public virtual void EnterVariableAssignmentStatement(VariableAssignmentStatement ctx){}
    public virtual void LeaveVariableAssignmentStatement(VariableAssignmentStatement ctx){}
    public virtual void EnterVariableDeclarationStatement(VariableDeclarationStatement ctx){}
    public virtual void LeaveVariableDeclarationStatement(VariableDeclarationStatement ctx){}
    public virtual void EnterReturnStatement(ReturnStatement ctx){}
    public virtual void LeaveReturnStatement(ReturnStatement ctx){}
    public virtual void EnterWhileStatement(WhileStatement ctx){}
    public virtual void LeaveWhileStatement(WhileStatement ctx){}
    public virtual void EnterExpressionStatement(ExpressionStatement ctx){}
    public virtual void LeaveExpressionStatement(ExpressionStatement ctx){}
    public virtual void EnterUnaryExpression(UnaryExpression ctx){}
    public virtual void LeaveUnaryExpression(UnaryExpression ctx){}
    public virtual void EnterBinaryExpression(BinaryExpression ctx){}
    public virtual void LeaveBinaryExpression(BinaryExpression ctx){}
    public virtual void EnterVariableReferenceExpression(VariableReferenceExpression ctx){}
    public virtual void LeaveVariableReferenceExpression(VariableReferenceExpression ctx){}
    public virtual void EnterTypeCastExpression(TypeCastExpression ctx){}
    public virtual void LeaveTypeCastExpression(TypeCastExpression ctx){}
    public virtual void EnterFuncCallExp(FuncCallExp ctx){}
    public virtual void LeaveFuncCallExp(FuncCallExp ctx){}
    public virtual void EnterBoolLiteral(BoolLiteral ctx){}
    public virtual void LeaveBoolLiteral(BoolLiteral ctx){}
    public virtual void EnterCharLiteral(CharLiteral ctx){}
    public virtual void LeaveCharLiteral(CharLiteral ctx){}
    public virtual void EnterStringLiteral(StringLiteral ctx){}
    public virtual void LeaveStringLiteral(StringLiteral ctx){}
    public virtual void EnterUriLiteral(UriLiteral ctx){}
    public virtual void LeaveUriLiteral(UriLiteral ctx){}
    public virtual void EnterDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx){}
    public virtual void LeaveDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx){}
    public virtual void EnterDateOnlyLiteral(DateOnlyLiteral ctx){}
    public virtual void LeaveDateOnlyLiteral(DateOnlyLiteral ctx){}
    public virtual void EnterTimeOnlyLiteral(TimeOnlyLiteral ctx){}
    public virtual void LeaveTimeOnlyLiteral(TimeOnlyLiteral ctx){}
    public virtual void EnterSByteLiteral(SByteLiteral ctx){}
    public virtual void LeaveSByteLiteral(SByteLiteral ctx){}
    public virtual void EnterByteLiteral(ByteLiteral ctx){}
    public virtual void LeaveByteLiteral(ByteLiteral ctx){}
    public virtual void EnterShortLiteral(ShortLiteral ctx){}
    public virtual void LeaveShortLiteral(ShortLiteral ctx){}
    public virtual void EnterUShortLiteral(UShortLiteral ctx){}
    public virtual void LeaveUShortLiteral(UShortLiteral ctx){}
    public virtual void EnterIntLiteral(IntLiteral ctx){}
    public virtual void LeaveIntLiteral(IntLiteral ctx){}
    public virtual void EnterUIntLiteral(UIntLiteral ctx){}
    public virtual void LeaveUIntLiteral(UIntLiteral ctx){}
    public virtual void EnterLongLiteral(LongLiteral ctx){}
    public virtual void LeaveLongLiteral(LongLiteral ctx){}
    public virtual void EnterULongLiteral(ULongLiteral ctx){}
    public virtual void LeaveULongLiteral(ULongLiteral ctx){}
    public virtual void EnterFloatLiteral(FloatLiteral ctx){}
    public virtual void LeaveFloatLiteral(FloatLiteral ctx){}
    public virtual void EnterDoubleLiteral(DoubleLiteral ctx){}
    public virtual void LeaveDoubleLiteral(DoubleLiteral ctx){}
    public virtual void EnterDecimalLiteral(DecimalLiteral ctx){}
    public virtual void LeaveDecimalLiteral(DecimalLiteral ctx){}
}


public interface IAstRecursiveDescentVisitor
{
    public AstThing Visit(AstThing ctx);
    public AssemblyDeclaration VisitAssemblyDeclaration(AssemblyDeclaration ctx);
    public AssemblyReference VisitAssemblyReference(AssemblyReference ctx);
    public ModuleDeclaration VisitModuleDeclaration(ModuleDeclaration ctx);
    public Version VisitVersion(Version ctx);
    public ClassDefinition VisitClassDefinition(ClassDefinition ctx);
    public MemberAccessExpression VisitMemberAccessExpression(MemberAccessExpression ctx);
    public ParameterDeclaration VisitParameterDeclaration(ParameterDeclaration ctx);
    public ParameterSignature VisitParameterSignature(ParameterSignature ctx);
    public MethodSignature VisitMethodSignature(MethodSignature ctx);
    public MethodHeader VisitMethodHeader(MethodHeader ctx);
    public MethodRef VisitMethodRef(MethodRef ctx);
    public MethodImpl VisitMethodImpl(MethodImpl ctx);
    public MethodDefinition VisitMethodDefinition(MethodDefinition ctx);
    public TypeReference VisitTypeReference(TypeReference ctx);
    public MemberRef VisitMemberRef(MemberRef ctx);
    public FieldDefinition VisitFieldDefinition(FieldDefinition ctx);
    public PropertyDefinition VisitPropertyDefinition(PropertyDefinition ctx);
    public Block VisitBlock(Block ctx);
    public IfStatement VisitIfStatement(IfStatement ctx);
    public VariableAssignmentStatement VisitVariableAssignmentStatement(VariableAssignmentStatement ctx);
    public VariableDeclarationStatement VisitVariableDeclarationStatement(VariableDeclarationStatement ctx);
    public ReturnStatement VisitReturnStatement(ReturnStatement ctx);
    public WhileStatement VisitWhileStatement(WhileStatement ctx);
    public ExpressionStatement VisitExpressionStatement(ExpressionStatement ctx);
    public UnaryExpression VisitUnaryExpression(UnaryExpression ctx);
    public BinaryExpression VisitBinaryExpression(BinaryExpression ctx);
    public VariableReferenceExpression VisitVariableReferenceExpression(VariableReferenceExpression ctx);
    public TypeCastExpression VisitTypeCastExpression(TypeCastExpression ctx);
    public FuncCallExp VisitFuncCallExp(FuncCallExp ctx);
    public BoolLiteral VisitBoolLiteral(BoolLiteral ctx);
    public CharLiteral VisitCharLiteral(CharLiteral ctx);
    public StringLiteral VisitStringLiteral(StringLiteral ctx);
    public UriLiteral VisitUriLiteral(UriLiteral ctx);
    public DateTimeOffsetLiteral VisitDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx);
    public DateOnlyLiteral VisitDateOnlyLiteral(DateOnlyLiteral ctx);
    public TimeOnlyLiteral VisitTimeOnlyLiteral(TimeOnlyLiteral ctx);
    public SByteLiteral VisitSByteLiteral(SByteLiteral ctx);
    public ByteLiteral VisitByteLiteral(ByteLiteral ctx);
    public ShortLiteral VisitShortLiteral(ShortLiteral ctx);
    public UShortLiteral VisitUShortLiteral(UShortLiteral ctx);
    public IntLiteral VisitIntLiteral(IntLiteral ctx);
    public UIntLiteral VisitUIntLiteral(UIntLiteral ctx);
    public LongLiteral VisitLongLiteral(LongLiteral ctx);
    public ULongLiteral VisitULongLiteral(ULongLiteral ctx);
    public FloatLiteral VisitFloatLiteral(FloatLiteral ctx);
    public DoubleLiteral VisitDoubleLiteral(DoubleLiteral ctx);
    public DecimalLiteral VisitDecimalLiteral(DecimalLiteral ctx);
}

public class DefaultRecursiveDescentVisitor : IAstRecursiveDescentVisitor
{
    public virtual AstThing Visit(AstThing ctx){
        if(ctx == null) return ctx;
        return ctx switch
        {
             AssemblyDeclaration node => VisitAssemblyDeclaration(node),
             AssemblyReference node => VisitAssemblyReference(node),
             ModuleDeclaration node => VisitModuleDeclaration(node),
             Version node => VisitVersion(node),
             ClassDefinition node => VisitClassDefinition(node),
             MemberAccessExpression node => VisitMemberAccessExpression(node),
             ParameterDeclaration node => VisitParameterDeclaration(node),
             ParameterSignature node => VisitParameterSignature(node),
             MethodSignature node => VisitMethodSignature(node),
             MethodHeader node => VisitMethodHeader(node),
             MethodRef node => VisitMethodRef(node),
             MethodImpl node => VisitMethodImpl(node),
             MethodDefinition node => VisitMethodDefinition(node),
             TypeReference node => VisitTypeReference(node),
             MemberRef node => VisitMemberRef(node),
             FieldDefinition node => VisitFieldDefinition(node),
             PropertyDefinition node => VisitPropertyDefinition(node),
             Block node => VisitBlock(node),
             IfStatement node => VisitIfStatement(node),
             VariableAssignmentStatement node => VisitVariableAssignmentStatement(node),
             VariableDeclarationStatement node => VisitVariableDeclarationStatement(node),
             ReturnStatement node => VisitReturnStatement(node),
             WhileStatement node => VisitWhileStatement(node),
             ExpressionStatement node => VisitExpressionStatement(node),
             UnaryExpression node => VisitUnaryExpression(node),
             BinaryExpression node => VisitBinaryExpression(node),
             VariableReferenceExpression node => VisitVariableReferenceExpression(node),
             TypeCastExpression node => VisitTypeCastExpression(node),
             FuncCallExp node => VisitFuncCallExp(node),
             BoolLiteral node => VisitBoolLiteral(node),
             CharLiteral node => VisitCharLiteral(node),
             StringLiteral node => VisitStringLiteral(node),
             UriLiteral node => VisitUriLiteral(node),
             DateTimeOffsetLiteral node => VisitDateTimeOffsetLiteral(node),
             DateOnlyLiteral node => VisitDateOnlyLiteral(node),
             TimeOnlyLiteral node => VisitTimeOnlyLiteral(node),
             SByteLiteral node => VisitSByteLiteral(node),
             ByteLiteral node => VisitByteLiteral(node),
             ShortLiteral node => VisitShortLiteral(node),
             UShortLiteral node => VisitUShortLiteral(node),
             IntLiteral node => VisitIntLiteral(node),
             UIntLiteral node => VisitUIntLiteral(node),
             LongLiteral node => VisitLongLiteral(node),
             ULongLiteral node => VisitULongLiteral(node),
             FloatLiteral node => VisitFloatLiteral(node),
             DoubleLiteral node => VisitDoubleLiteral(node),
             DecimalLiteral node => VisitDecimalLiteral(node),

            { } node => null,
        };
    }

    public virtual AssemblyDeclaration VisitAssemblyDeclaration(AssemblyDeclaration ctx)
    {
     return ctx with {
        };
    }
    public virtual AssemblyReference VisitAssemblyReference(AssemblyReference ctx)
    {
     return ctx with {
        };
    }
    public virtual ModuleDeclaration VisitModuleDeclaration(ModuleDeclaration ctx)
    {
     return ctx with {
        };
    }
    public virtual Version VisitVersion(Version ctx)
    {
     return ctx with {
        };
    }
    public virtual ClassDefinition VisitClassDefinition(ClassDefinition ctx)
    {
     return ctx with {
        };
    }
    public virtual MemberAccessExpression VisitMemberAccessExpression(MemberAccessExpression ctx)
    {
     return ctx with {
        };
    }
    public virtual ParameterDeclaration VisitParameterDeclaration(ParameterDeclaration ctx)
    {
     return ctx with {
        };
    }
    public virtual ParameterSignature VisitParameterSignature(ParameterSignature ctx)
    {
     return ctx with {
        };
    }
    public virtual MethodSignature VisitMethodSignature(MethodSignature ctx)
    {
     return ctx with {
        };
    }
    public virtual MethodHeader VisitMethodHeader(MethodHeader ctx)
    {
     return ctx with {
        };
    }
    public virtual MethodRef VisitMethodRef(MethodRef ctx)
    {
     return ctx with {
        };
    }
    public virtual MethodImpl VisitMethodImpl(MethodImpl ctx)
    {
     return ctx with {
        };
    }
    public virtual MethodDefinition VisitMethodDefinition(MethodDefinition ctx)
    {
     return ctx with {
        };
    }
    public virtual TypeReference VisitTypeReference(TypeReference ctx)
    {
     return ctx with {
        };
    }
    public virtual MemberRef VisitMemberRef(MemberRef ctx)
    {
     return ctx with {
        };
    }
    public virtual FieldDefinition VisitFieldDefinition(FieldDefinition ctx)
    {
     return ctx with {
        };
    }
    public virtual PropertyDefinition VisitPropertyDefinition(PropertyDefinition ctx)
    {
     return ctx with {
        };
    }
    public virtual Block VisitBlock(Block ctx)
    {
     return ctx with {
        };
    }
    public virtual IfStatement VisitIfStatement(IfStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual VariableAssignmentStatement VisitVariableAssignmentStatement(VariableAssignmentStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual VariableDeclarationStatement VisitVariableDeclarationStatement(VariableDeclarationStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual ReturnStatement VisitReturnStatement(ReturnStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual WhileStatement VisitWhileStatement(WhileStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual ExpressionStatement VisitExpressionStatement(ExpressionStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual UnaryExpression VisitUnaryExpression(UnaryExpression ctx)
    {
     return ctx with {
        };
    }
    public virtual BinaryExpression VisitBinaryExpression(BinaryExpression ctx)
    {
     return ctx with {
        };
    }
    public virtual VariableReferenceExpression VisitVariableReferenceExpression(VariableReferenceExpression ctx)
    {
     return ctx with {
        };
    }
    public virtual TypeCastExpression VisitTypeCastExpression(TypeCastExpression ctx)
    {
     return ctx with {
        };
    }
    public virtual FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
    {
     return ctx with {
        };
    }
    public virtual BoolLiteral VisitBoolLiteral(BoolLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual CharLiteral VisitCharLiteral(CharLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual StringLiteral VisitStringLiteral(StringLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual UriLiteral VisitUriLiteral(UriLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual DateTimeOffsetLiteral VisitDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual DateOnlyLiteral VisitDateOnlyLiteral(DateOnlyLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual TimeOnlyLiteral VisitTimeOnlyLiteral(TimeOnlyLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual SByteLiteral VisitSByteLiteral(SByteLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual ByteLiteral VisitByteLiteral(ByteLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual ShortLiteral VisitShortLiteral(ShortLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual UShortLiteral VisitUShortLiteral(UShortLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual IntLiteral VisitIntLiteral(IntLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual UIntLiteral VisitUIntLiteral(UIntLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual LongLiteral VisitLongLiteral(LongLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual ULongLiteral VisitULongLiteral(ULongLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual FloatLiteral VisitFloatLiteral(FloatLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual DoubleLiteral VisitDoubleLiteral(DoubleLiteral ctx)
    {
     return ctx with {
        };
    }
    public virtual DecimalLiteral VisitDecimalLiteral(DecimalLiteral ctx)
    {
     return ctx with {
        };
    }

}


