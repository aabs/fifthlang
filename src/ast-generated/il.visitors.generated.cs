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
    public void EnterLiteral(Literal ctx);
    public void LeaveLiteral(Literal ctx);
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
    public void EnterUriLiteral(UriLiteral ctx);
    public void LeaveUriLiteral(UriLiteral ctx);
    public void EnterUriLiteral(UriLiteral ctx);
    public void LeaveUriLiteral(UriLiteral ctx);
    public void EnterUriLiteral(UriLiteral ctx);
    public void LeaveUriLiteral(UriLiteral ctx);
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
    public virtual void EnterAssemblyDeclaration(AssemblyDeclaration ctx) { }
    public virtual void LeaveAssemblyDeclaration(AssemblyDeclaration ctx) { }
    public virtual void EnterAssemblyReference(AssemblyReference ctx) { }
    public virtual void LeaveAssemblyReference(AssemblyReference ctx) { }
    public virtual void EnterModuleDeclaration(ModuleDeclaration ctx) { }
    public virtual void LeaveModuleDeclaration(ModuleDeclaration ctx) { }
    public virtual void EnterVersion(Version ctx) { }
    public virtual void LeaveVersion(Version ctx) { }
    public virtual void EnterClassDefinition(ClassDefinition ctx) { }
    public virtual void LeaveClassDefinition(ClassDefinition ctx) { }
    public virtual void EnterMemberAccessExpression(MemberAccessExpression ctx) { }
    public virtual void LeaveMemberAccessExpression(MemberAccessExpression ctx) { }
    public virtual void EnterParameterDeclaration(ParameterDeclaration ctx) { }
    public virtual void LeaveParameterDeclaration(ParameterDeclaration ctx) { }
    public virtual void EnterParameterSignature(ParameterSignature ctx) { }
    public virtual void LeaveParameterSignature(ParameterSignature ctx) { }
    public virtual void EnterMethodSignature(MethodSignature ctx) { }
    public virtual void LeaveMethodSignature(MethodSignature ctx) { }
    public virtual void EnterMethodHeader(MethodHeader ctx) { }
    public virtual void LeaveMethodHeader(MethodHeader ctx) { }
    public virtual void EnterMethodRef(MethodRef ctx) { }
    public virtual void LeaveMethodRef(MethodRef ctx) { }
    public virtual void EnterMethodImpl(MethodImpl ctx) { }
    public virtual void LeaveMethodImpl(MethodImpl ctx) { }
    public virtual void EnterMethodDefinition(MethodDefinition ctx) { }
    public virtual void LeaveMethodDefinition(MethodDefinition ctx) { }
    public virtual void EnterTypeReference(TypeReference ctx) { }
    public virtual void LeaveTypeReference(TypeReference ctx) { }
    public virtual void EnterMemberRef(MemberRef ctx) { }
    public virtual void LeaveMemberRef(MemberRef ctx) { }
    public virtual void EnterFieldDefinition(FieldDefinition ctx) { }
    public virtual void LeaveFieldDefinition(FieldDefinition ctx) { }
    public virtual void EnterPropertyDefinition(PropertyDefinition ctx) { }
    public virtual void LeavePropertyDefinition(PropertyDefinition ctx) { }
    public virtual void EnterBlock(Block ctx) { }
    public virtual void LeaveBlock(Block ctx) { }
    public virtual void EnterIfStatement(IfStatement ctx) { }
    public virtual void LeaveIfStatement(IfStatement ctx) { }
    public virtual void EnterVariableAssignmentStatement(VariableAssignmentStatement ctx) { }
    public virtual void LeaveVariableAssignmentStatement(VariableAssignmentStatement ctx) { }
    public virtual void EnterVariableDeclarationStatement(VariableDeclarationStatement ctx) { }
    public virtual void LeaveVariableDeclarationStatement(VariableDeclarationStatement ctx) { }
    public virtual void EnterReturnStatement(ReturnStatement ctx) { }
    public virtual void LeaveReturnStatement(ReturnStatement ctx) { }
    public virtual void EnterWhileStatement(WhileStatement ctx) { }
    public virtual void LeaveWhileStatement(WhileStatement ctx) { }
    public virtual void EnterExpressionStatement(ExpressionStatement ctx) { }
    public virtual void LeaveExpressionStatement(ExpressionStatement ctx) { }
    public virtual void EnterUnaryExpression(UnaryExpression ctx) { }
    public virtual void LeaveUnaryExpression(UnaryExpression ctx) { }
    public virtual void EnterBinaryExpression(BinaryExpression ctx) { }
    public virtual void LeaveBinaryExpression(BinaryExpression ctx) { }
    public virtual void EnterVariableReferenceExpression(VariableReferenceExpression ctx) { }
    public virtual void LeaveVariableReferenceExpression(VariableReferenceExpression ctx) { }
    public virtual void EnterTypeCastExpression(TypeCastExpression ctx) { }
    public virtual void LeaveTypeCastExpression(TypeCastExpression ctx) { }
    public virtual void EnterFuncCallExp(FuncCallExp ctx) { }
    public virtual void LeaveFuncCallExp(FuncCallExp ctx) { }
    public virtual void EnterLiteral(Literal ctx) { }
    public virtual void LeaveLiteral(Literal ctx) { }
    public virtual void EnterBoolLiteral(BoolLiteral ctx) { }
    public virtual void LeaveBoolLiteral(BoolLiteral ctx) { }
    public virtual void EnterCharLiteral(CharLiteral ctx) { }
    public virtual void LeaveCharLiteral(CharLiteral ctx) { }
    public virtual void EnterStringLiteral(StringLiteral ctx) { }
    public virtual void LeaveStringLiteral(StringLiteral ctx) { }
    public virtual void EnterUriLiteral(UriLiteral ctx) { }
    public virtual void LeaveUriLiteral(UriLiteral ctx) { }
    public virtual void EnterDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx) { }
    public virtual void LeaveDateTimeOffsetLiteral(DateTimeOffsetLiteral ctx) { }
    public virtual void EnterDateOnlyLiteral(DateOnlyLiteral ctx) { }
    public virtual void LeaveDateOnlyLiteral(DateOnlyLiteral ctx) { }
    public virtual void EnterTimeOnlyLiteral(TimeOnlyLiteral ctx) { }
    public virtual void LeaveTimeOnlyLiteral(TimeOnlyLiteral ctx) { }
    public virtual void EnterUriLiteral(UriLiteral ctx) { }
    public virtual void LeaveUriLiteral(UriLiteral ctx) { }
    public virtual void EnterUriLiteral(UriLiteral ctx) { }
    public virtual void LeaveUriLiteral(UriLiteral ctx) { }
    public virtual void EnterUriLiteral(UriLiteral ctx) { }
    public virtual void LeaveUriLiteral(UriLiteral ctx) { }
    public virtual void EnterSByteLiteral(SByteLiteral ctx) { }
    public virtual void LeaveSByteLiteral(SByteLiteral ctx) { }
    public virtual void EnterByteLiteral(ByteLiteral ctx) { }
    public virtual void LeaveByteLiteral(ByteLiteral ctx) { }
    public virtual void EnterShortLiteral(ShortLiteral ctx) { }
    public virtual void LeaveShortLiteral(ShortLiteral ctx) { }
    public virtual void EnterUShortLiteral(UShortLiteral ctx) { }
    public virtual void LeaveUShortLiteral(UShortLiteral ctx) { }
    public virtual void EnterIntLiteral(IntLiteral ctx) { }
    public virtual void LeaveIntLiteral(IntLiteral ctx) { }
    public virtual void EnterUIntLiteral(UIntLiteral ctx) { }
    public virtual void LeaveUIntLiteral(UIntLiteral ctx) { }
    public virtual void EnterLongLiteral(LongLiteral ctx) { }
    public virtual void LeaveLongLiteral(LongLiteral ctx) { }
    public virtual void EnterULongLiteral(ULongLiteral ctx) { }
    public virtual void LeaveULongLiteral(ULongLiteral ctx) { }
    public virtual void EnterFloatLiteral(FloatLiteral ctx) { }
    public virtual void LeaveFloatLiteral(FloatLiteral ctx) { }
    public virtual void EnterDoubleLiteral(DoubleLiteral ctx) { }
    public virtual void LeaveDoubleLiteral(DoubleLiteral ctx) { }
    public virtual void EnterDecimalLiteral(DecimalLiteral ctx) { }
    public virtual void LeaveDecimalLiteral(DecimalLiteral ctx) { }
}
