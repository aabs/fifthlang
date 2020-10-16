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
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="FifthParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public interface IFifthVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.fifth"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFifth([NotNull] FifthParser.FifthContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.alias"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAlias([NotNull] FifthParser.AliasContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlock([NotNull] FifthParser.BlockContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EFuncCall</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEFuncCall([NotNull] FifthParser.EFuncCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ETypeCreate</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitETypeCreate([NotNull] FifthParser.ETypeCreateContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EVarname</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEVarname([NotNull] FifthParser.EVarnameContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EInt</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEInt([NotNull] FifthParser.EIntContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ELT</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitELT([NotNull] FifthParser.ELTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EDiv</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEDiv([NotNull] FifthParser.EDivContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EGEQ</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEGEQ([NotNull] FifthParser.EGEQContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EAnd</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEAnd([NotNull] FifthParser.EAndContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EGT</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEGT([NotNull] FifthParser.EGTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ELEQ</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitELEQ([NotNull] FifthParser.ELEQContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ENegation</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitENegation([NotNull] FifthParser.ENegationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ESub</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitESub([NotNull] FifthParser.ESubContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EDouble</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEDouble([NotNull] FifthParser.EDoubleContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EFuncParen</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEFuncParen([NotNull] FifthParser.EFuncParenContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EAdd</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEAdd([NotNull] FifthParser.EAddContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EString</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEString([NotNull] FifthParser.EStringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EMul</c>
	/// labeled alternative in <see cref="FifthParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEMul([NotNull] FifthParser.EMulContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.formal_parameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFormal_parameters([NotNull] FifthParser.Formal_parametersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.function_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunction_declaration([NotNull] FifthParser.Function_declarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.function_args"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunction_args([NotNull] FifthParser.Function_argsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.function_body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunction_body([NotNull] FifthParser.Function_bodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.function_call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunction_call([NotNull] FifthParser.Function_callContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.function_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunction_name([NotNull] FifthParser.Function_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iri"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIri([NotNull] FifthParser.IriContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iri_query"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIri_query([NotNull] FifthParser.Iri_queryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iri_query_param"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIri_query_param([NotNull] FifthParser.Iri_query_paramContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.module_import"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitModule_import([NotNull] FifthParser.Module_importContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.module_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitModule_name([NotNull] FifthParser.Module_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.packagename"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPackagename([NotNull] FifthParser.PackagenameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.parameter_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter_declaration([NotNull] FifthParser.Parameter_declarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.parameter_type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter_type([NotNull] FifthParser.Parameter_typeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.parameter_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter_name([NotNull] FifthParser.Parameter_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>VarDeclStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarDeclStmt([NotNull] FifthParser.VarDeclStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>AssignmentStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignmentStmt([NotNull] FifthParser.AssignmentStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ReturnStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStmt([NotNull] FifthParser.ReturnStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>IfStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStmt([NotNull] FifthParser.IfStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>IfElseStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfElseStmt([NotNull] FifthParser.IfElseStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>WithStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWithStmt([NotNull] FifthParser.WithStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ExpStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpStmt([NotNull] FifthParser.ExpStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.type_initialiser"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_initialiser([NotNull] FifthParser.Type_initialiserContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.type_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_name([NotNull] FifthParser.Type_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.type_property_init"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_property_init([NotNull] FifthParser.Type_property_initContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.var_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVar_name([NotNull] FifthParser.Var_nameContext context);
}