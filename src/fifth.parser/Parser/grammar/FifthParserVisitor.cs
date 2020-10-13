//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from src/fifth.parser/Parser/grammar/FifthParser.g4 by ANTLR 4.8

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
public interface IFifthParserVisitor<Result> : IParseTreeVisitor<Result> {
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
	/// Visit a parse tree produced by the <c>ScientificNumber</c>
	/// labeled alternative in <see cref="FifthParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitScientificNumber([NotNull] FifthParser.ScientificNumberContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>VarReference</c>
	/// labeled alternative in <see cref="FifthParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarReference([NotNull] FifthParser.VarReferenceContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>String</c>
	/// labeled alternative in <see cref="FifthParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString([NotNull] FifthParser.StringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ParenthesisedExp</c>
	/// labeled alternative in <see cref="FifthParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParenthesisedExp([NotNull] FifthParser.ParenthesisedExpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlock([NotNull] FifthParser.BlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.equation"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEquation([NotNull] FifthParser.EquationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] FifthParser.ExpressionContext context);
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
	/// Visit a parse tree produced by <see cref="FifthParser.multiplying_expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiplying_expression([NotNull] FifthParser.Multiplying_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.packagename"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPackagename([NotNull] FifthParser.PackagenameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.pow_expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPow_expression([NotNull] FifthParser.Pow_expressionContext context);
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
	/// Visit a parse tree produced by <see cref="FifthParser.q_function_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitQ_function_name([NotNull] FifthParser.Q_function_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.qvarname"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitQvarname([NotNull] FifthParser.QvarnameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.q_type_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitQ_type_name([NotNull] FifthParser.Q_type_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Equals</c>
	/// labeled alternative in <see cref="FifthParser.relop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEquals([NotNull] FifthParser.EqualsContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>GreaterThan</c>
	/// labeled alternative in <see cref="FifthParser.relop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGreaterThan([NotNull] FifthParser.GreaterThanContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>LessThan</c>
	/// labeled alternative in <see cref="FifthParser.relop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLessThan([NotNull] FifthParser.LessThanContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.scientific"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitScientific([NotNull] FifthParser.ScientificContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Plus</c>
	/// labeled alternative in <see cref="FifthParser.signed_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPlus([NotNull] FifthParser.PlusContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Minus</c>
	/// labeled alternative in <see cref="FifthParser.signed_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMinus([NotNull] FifthParser.MinusContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>FunctionCall</c>
	/// labeled alternative in <see cref="FifthParser.signed_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionCall([NotNull] FifthParser.FunctionCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>PlainAtom</c>
	/// labeled alternative in <see cref="FifthParser.signed_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPlainAtom([NotNull] FifthParser.PlainAtomContext context);
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
	/// Visit a parse tree produced by the <c>ExpnStmt</c>
	/// labeled alternative in <see cref="FifthParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpnStmt([NotNull] FifthParser.ExpnStmtContext context);
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
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ihier_part"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIhier_part([NotNull] FifthParser.Ihier_partContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iri_reference"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIri_reference([NotNull] FifthParser.Iri_referenceContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.absolute_iri"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAbsolute_iri([NotNull] FifthParser.Absolute_iriContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.irelative_ref"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIrelative_ref([NotNull] FifthParser.Irelative_refContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.irelative_part"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIrelative_part([NotNull] FifthParser.Irelative_partContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iauthority"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIauthority([NotNull] FifthParser.IauthorityContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iuserinfo"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIuserinfo([NotNull] FifthParser.IuserinfoContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ihost"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIhost([NotNull] FifthParser.IhostContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ireg_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIreg_name([NotNull] FifthParser.Ireg_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ipath"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIpath([NotNull] FifthParser.IpathContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ipath_abempty"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIpath_abempty([NotNull] FifthParser.Ipath_abemptyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ipath_absolute"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIpath_absolute([NotNull] FifthParser.Ipath_absoluteContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ipath_noscheme"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIpath_noscheme([NotNull] FifthParser.Ipath_noschemeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ipath_rootless"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIpath_rootless([NotNull] FifthParser.Ipath_rootlessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ipath_empty"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIpath_empty([NotNull] FifthParser.Ipath_emptyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.isegment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIsegment([NotNull] FifthParser.IsegmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.isegment_nz"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIsegment_nz([NotNull] FifthParser.Isegment_nzContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.isegment_nz_nc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIsegment_nz_nc([NotNull] FifthParser.Isegment_nz_ncContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ipchar"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIpchar([NotNull] FifthParser.IpcharContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iquery"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIquery([NotNull] FifthParser.IqueryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ifragment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfragment([NotNull] FifthParser.IfragmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.iunreserved"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIunreserved([NotNull] FifthParser.IunreservedContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.scheme"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitScheme([NotNull] FifthParser.SchemeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.port"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPort([NotNull] FifthParser.PortContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ip_literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIp_literal([NotNull] FifthParser.Ip_literalContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ip_v_future"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIp_v_future([NotNull] FifthParser.Ip_v_futureContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ip_v6_address"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIp_v6_address([NotNull] FifthParser.Ip_v6_addressContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.h16"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitH16([NotNull] FifthParser.H16Context context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ls32"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLs32([NotNull] FifthParser.Ls32Context context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.ip_v4_address"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIp_v4_address([NotNull] FifthParser.Ip_v4_addressContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.dec_octet"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDec_octet([NotNull] FifthParser.Dec_octetContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.pct_encoded"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPct_encoded([NotNull] FifthParser.Pct_encodedContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.unreserved"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnreserved([NotNull] FifthParser.UnreservedContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.reserved"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReserved([NotNull] FifthParser.ReservedContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.gen_delims"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGen_delims([NotNull] FifthParser.Gen_delimsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.sub_delims"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSub_delims([NotNull] FifthParser.Sub_delimsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.alpha"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAlpha([NotNull] FifthParser.AlphaContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.hexdig"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitHexdig([NotNull] FifthParser.HexdigContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.digit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDigit([NotNull] FifthParser.DigitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="FifthParser.non_zero_digit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNon_zero_digit([NotNull] FifthParser.Non_zero_digitContext context);
}
