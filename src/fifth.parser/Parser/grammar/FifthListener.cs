//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from src/fifth.parser/Parser/grammar/Fifth.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="FifthParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public interface IFifthListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.fifth"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFifth([NotNull] FifthParser.FifthContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.fifth"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFifth([NotNull] FifthParser.FifthContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.alias"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAlias([NotNull] FifthParser.AliasContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.alias"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAlias([NotNull] FifthParser.AliasContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBlock([NotNull] FifthParser.BlockContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBlock([NotNull] FifthParser.BlockContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EFuncCall</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEFuncCall([NotNull] FifthParser.EFuncCallContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EFuncCall</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEFuncCall([NotNull] FifthParser.EFuncCallContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ETypeCreate</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterETypeCreate([NotNull] FifthParser.ETypeCreateContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ETypeCreate</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitETypeCreate([NotNull] FifthParser.ETypeCreateContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EVarname</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEVarname([NotNull] FifthParser.EVarnameContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EVarname</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEVarname([NotNull] FifthParser.EVarnameContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EInt</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEInt([NotNull] FifthParser.EIntContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EInt</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEInt([NotNull] FifthParser.EIntContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ELT</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterELT([NotNull] FifthParser.ELTContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ELT</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitELT([NotNull] FifthParser.ELTContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EDiv</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEDiv([NotNull] FifthParser.EDivContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EDiv</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEDiv([NotNull] FifthParser.EDivContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EGEQ</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEGEQ([NotNull] FifthParser.EGEQContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EGEQ</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEGEQ([NotNull] FifthParser.EGEQContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EAnd</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEAnd([NotNull] FifthParser.EAndContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EAnd</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEAnd([NotNull] FifthParser.EAndContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EGT</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEGT([NotNull] FifthParser.EGTContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EGT</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEGT([NotNull] FifthParser.EGTContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ELEQ</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterELEQ([NotNull] FifthParser.ELEQContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ELEQ</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitELEQ([NotNull] FifthParser.ELEQContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ENegation</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterENegation([NotNull] FifthParser.ENegationContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ENegation</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitENegation([NotNull] FifthParser.ENegationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ESub</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterESub([NotNull] FifthParser.ESubContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ESub</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitESub([NotNull] FifthParser.ESubContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EDouble</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEDouble([NotNull] FifthParser.EDoubleContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EDouble</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEDouble([NotNull] FifthParser.EDoubleContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EFuncParen</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEFuncParen([NotNull] FifthParser.EFuncParenContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EFuncParen</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEFuncParen([NotNull] FifthParser.EFuncParenContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EAdd</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEAdd([NotNull] FifthParser.EAddContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EAdd</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEAdd([NotNull] FifthParser.EAddContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EString</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEString([NotNull] FifthParser.EStringContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EString</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEString([NotNull] FifthParser.EStringContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EStatement</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEStatement([NotNull] FifthParser.EStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EStatement</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEStatement([NotNull] FifthParser.EStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EMul</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEMul([NotNull] FifthParser.EMulContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EMul</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEMul([NotNull] FifthParser.EMulContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.formal_parameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFormal_parameters([NotNull] FifthParser.Formal_parametersContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.formal_parameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFormal_parameters([NotNull] FifthParser.Formal_parametersContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.function_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_declaration([NotNull] FifthParser.Function_declarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.function_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_declaration([NotNull] FifthParser.Function_declarationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.function_args"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_args([NotNull] FifthParser.Function_argsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.function_args"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_args([NotNull] FifthParser.Function_argsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.function_body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_body([NotNull] FifthParser.Function_bodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.function_body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_body([NotNull] FifthParser.Function_bodyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.function_call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_call([NotNull] FifthParser.Function_callContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.function_call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_call([NotNull] FifthParser.Function_callContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.function_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_name([NotNull] FifthParser.Function_nameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.function_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_name([NotNull] FifthParser.Function_nameContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.iri"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIri([NotNull] FifthParser.IriContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.iri"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIri([NotNull] FifthParser.IriContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.iri_query"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIri_query([NotNull] FifthParser.Iri_queryContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.iri_query"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIri_query([NotNull] FifthParser.Iri_queryContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.iri_query_param"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIri_query_param([NotNull] FifthParser.Iri_query_paramContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.iri_query_param"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIri_query_param([NotNull] FifthParser.Iri_query_paramContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.module_import"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterModule_import([NotNull] FifthParser.Module_importContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.module_import"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitModule_import([NotNull] FifthParser.Module_importContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.module_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterModule_name([NotNull] FifthParser.Module_nameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.module_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitModule_name([NotNull] FifthParser.Module_nameContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.packagename"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPackagename([NotNull] FifthParser.PackagenameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.packagename"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPackagename([NotNull] FifthParser.PackagenameContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.parameter_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParameter_declaration([NotNull] FifthParser.Parameter_declarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.parameter_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParameter_declaration([NotNull] FifthParser.Parameter_declarationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.parameter_type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParameter_type([NotNull] FifthParser.Parameter_typeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.parameter_type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParameter_type([NotNull] FifthParser.Parameter_typeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.parameter_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParameter_name([NotNull] FifthParser.Parameter_nameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.parameter_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParameter_name([NotNull] FifthParser.Parameter_nameContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>VarDeclStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVarDeclStmt([NotNull] FifthParser.VarDeclStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>VarDeclStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVarDeclStmt([NotNull] FifthParser.VarDeclStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AssignmentStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAssignmentStmt([NotNull] FifthParser.AssignmentStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AssignmentStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAssignmentStmt([NotNull] FifthParser.AssignmentStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ReturnStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReturnStmt([NotNull] FifthParser.ReturnStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ReturnStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReturnStmt([NotNull] FifthParser.ReturnStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>IfElseStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIfElseStmt([NotNull] FifthParser.IfElseStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>IfElseStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIfElseStmt([NotNull] FifthParser.IfElseStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>WithStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterWithStmt([NotNull] FifthParser.WithStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>WithStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitWithStmt([NotNull] FifthParser.WithStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.type_initialiser"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterType_initialiser([NotNull] FifthParser.Type_initialiserContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.type_initialiser"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitType_initialiser([NotNull] FifthParser.Type_initialiserContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.type_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterType_name([NotNull] FifthParser.Type_nameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.type_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitType_name([NotNull] FifthParser.Type_nameContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.type_property_init"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterType_property_init([NotNull] FifthParser.Type_property_initContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.type_property_init"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitType_property_init([NotNull] FifthParser.Type_property_initContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="FifthParser.var_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVar_name([NotNull] FifthParser.Var_nameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="FifthParser.var_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVar_name([NotNull] FifthParser.Var_nameContext context);
}
