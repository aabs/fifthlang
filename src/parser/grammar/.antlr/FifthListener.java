// Generated from c:/dev/aabs/5th-related/ast-builder/src/parser/grammar/Fifth.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link FifthParser}.
 */
public interface FifthListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by the {@code exp_callsite_varname}
	 * labeled alternative in {@link FifthParser#call_site}.
	 * @param ctx the parse tree
	 */
	void enterExp_callsite_varname(FifthParser.Exp_callsite_varnameContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_callsite_varname}
	 * labeled alternative in {@link FifthParser#call_site}.
	 * @param ctx the parse tree
	 */
	void exitExp_callsite_varname(FifthParser.Exp_callsite_varnameContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_callsite_func_call}
	 * labeled alternative in {@link FifthParser#call_site}.
	 * @param ctx the parse tree
	 */
	void enterExp_callsite_func_call(FifthParser.Exp_callsite_func_callContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_callsite_func_call}
	 * labeled alternative in {@link FifthParser#call_site}.
	 * @param ctx the parse tree
	 */
	void exitExp_callsite_func_call(FifthParser.Exp_callsite_func_callContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_callsite_parenthesised}
	 * labeled alternative in {@link FifthParser#call_site}.
	 * @param ctx the parse tree
	 */
	void enterExp_callsite_parenthesised(FifthParser.Exp_callsite_parenthesisedContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_callsite_parenthesised}
	 * labeled alternative in {@link FifthParser#call_site}.
	 * @param ctx the parse tree
	 */
	void exitExp_callsite_parenthesised(FifthParser.Exp_callsite_parenthesisedContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#fifth}.
	 * @param ctx the parse tree
	 */
	void enterFifth(FifthParser.FifthContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#fifth}.
	 * @param ctx the parse tree
	 */
	void exitFifth(FifthParser.FifthContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#function_call}.
	 * @param ctx the parse tree
	 */
	void enterFunction_call(FifthParser.Function_callContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#function_call}.
	 * @param ctx the parse tree
	 */
	void exitFunction_call(FifthParser.Function_callContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#member_access_expression}.
	 * @param ctx the parse tree
	 */
	void enterMember_access_expression(FifthParser.Member_access_expressionContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#member_access_expression}.
	 * @param ctx the parse tree
	 */
	void exitMember_access_expression(FifthParser.Member_access_expressionContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#module_import}.
	 * @param ctx the parse tree
	 */
	void enterModule_import(FifthParser.Module_importContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#module_import}.
	 * @param ctx the parse tree
	 */
	void exitModule_import(FifthParser.Module_importContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#module_name}.
	 * @param ctx the parse tree
	 */
	void enterModule_name(FifthParser.Module_nameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#module_name}.
	 * @param ctx the parse tree
	 */
	void exitModule_name(FifthParser.Module_nameContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#packagename}.
	 * @param ctx the parse tree
	 */
	void enterPackagename(FifthParser.PackagenameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#packagename}.
	 * @param ctx the parse tree
	 */
	void exitPackagename(FifthParser.PackagenameContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#class_definition}.
	 * @param ctx the parse tree
	 */
	void enterClass_definition(FifthParser.Class_definitionContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#class_definition}.
	 * @param ctx the parse tree
	 */
	void exitClass_definition(FifthParser.Class_definitionContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#property_declaration}.
	 * @param ctx the parse tree
	 */
	void enterProperty_declaration(FifthParser.Property_declarationContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#property_declaration}.
	 * @param ctx the parse tree
	 */
	void exitProperty_declaration(FifthParser.Property_declarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#type_initialiser}.
	 * @param ctx the parse tree
	 */
	void enterType_initialiser(FifthParser.Type_initialiserContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#type_initialiser}.
	 * @param ctx the parse tree
	 */
	void exitType_initialiser(FifthParser.Type_initialiserContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#type_name}.
	 * @param ctx the parse tree
	 */
	void enterType_name(FifthParser.Type_nameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#type_name}.
	 * @param ctx the parse tree
	 */
	void exitType_name(FifthParser.Type_nameContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#type_property_init}.
	 * @param ctx the parse tree
	 */
	void enterType_property_init(FifthParser.Type_property_initContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#type_property_init}.
	 * @param ctx the parse tree
	 */
	void exitType_property_init(FifthParser.Type_property_initContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#function_declaration}.
	 * @param ctx the parse tree
	 */
	void enterFunction_declaration(FifthParser.Function_declarationContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#function_declaration}.
	 * @param ctx the parse tree
	 */
	void exitFunction_declaration(FifthParser.Function_declarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#function_body}.
	 * @param ctx the parse tree
	 */
	void enterFunction_body(FifthParser.Function_bodyContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#function_body}.
	 * @param ctx the parse tree
	 */
	void exitFunction_body(FifthParser.Function_bodyContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#function_name}.
	 * @param ctx the parse tree
	 */
	void enterFunction_name(FifthParser.Function_nameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#function_name}.
	 * @param ctx the parse tree
	 */
	void exitFunction_name(FifthParser.Function_nameContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#function_type}.
	 * @param ctx the parse tree
	 */
	void enterFunction_type(FifthParser.Function_typeContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#function_type}.
	 * @param ctx the parse tree
	 */
	void exitFunction_type(FifthParser.Function_typeContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#variable_constraint}.
	 * @param ctx the parse tree
	 */
	void enterVariable_constraint(FifthParser.Variable_constraintContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#variable_constraint}.
	 * @param ctx the parse tree
	 */
	void exitVariable_constraint(FifthParser.Variable_constraintContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#paramdecl}.
	 * @param ctx the parse tree
	 */
	void enterParamdecl(FifthParser.ParamdeclContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#paramdecl}.
	 * @param ctx the parse tree
	 */
	void exitParamdecl(FifthParser.ParamdeclContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#param_name}.
	 * @param ctx the parse tree
	 */
	void enterParam_name(FifthParser.Param_nameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#param_name}.
	 * @param ctx the parse tree
	 */
	void exitParam_name(FifthParser.Param_nameContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#param_type}.
	 * @param ctx the parse tree
	 */
	void enterParam_type(FifthParser.Param_typeContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#param_type}.
	 * @param ctx the parse tree
	 */
	void exitParam_type(FifthParser.Param_typeContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#destructuring_decl}.
	 * @param ctx the parse tree
	 */
	void enterDestructuring_decl(FifthParser.Destructuring_declContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#destructuring_decl}.
	 * @param ctx the parse tree
	 */
	void exitDestructuring_decl(FifthParser.Destructuring_declContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#destructure_binding}.
	 * @param ctx the parse tree
	 */
	void enterDestructure_binding(FifthParser.Destructure_bindingContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#destructure_binding}.
	 * @param ctx the parse tree
	 */
	void exitDestructure_binding(FifthParser.Destructure_bindingContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#block}.
	 * @param ctx the parse tree
	 */
	void enterBlock(FifthParser.BlockContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#block}.
	 * @param ctx the parse tree
	 */
	void exitBlock(FifthParser.BlockContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stmt_ifelse}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStmt_ifelse(FifthParser.Stmt_ifelseContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stmt_ifelse}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStmt_ifelse(FifthParser.Stmt_ifelseContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stmt_while}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStmt_while(FifthParser.Stmt_whileContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stmt_while}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStmt_while(FifthParser.Stmt_whileContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stmt_with}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStmt_with(FifthParser.Stmt_withContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stmt_with}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStmt_with(FifthParser.Stmt_withContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stmt_vardecl}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStmt_vardecl(FifthParser.Stmt_vardeclContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stmt_vardecl}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStmt_vardecl(FifthParser.Stmt_vardeclContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stmt_assignment}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStmt_assignment(FifthParser.Stmt_assignmentContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stmt_assignment}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStmt_assignment(FifthParser.Stmt_assignmentContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stmt_return}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStmt_return(FifthParser.Stmt_returnContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stmt_return}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStmt_return(FifthParser.Stmt_returnContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stmt_bareexpression}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStmt_bareexpression(FifthParser.Stmt_bareexpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stmt_bareexpression}
	 * labeled alternative in {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStmt_bareexpression(FifthParser.Stmt_bareexpressionContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#var_decl}.
	 * @param ctx the parse tree
	 */
	void enterVar_decl(FifthParser.Var_declContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#var_decl}.
	 * @param ctx the parse tree
	 */
	void exitVar_decl(FifthParser.Var_declContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#identifier_chain}.
	 * @param ctx the parse tree
	 */
	void enterIdentifier_chain(FifthParser.Identifier_chainContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#identifier_chain}.
	 * @param ctx the parse tree
	 */
	void exitIdentifier_chain(FifthParser.Identifier_chainContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#explist}.
	 * @param ctx the parse tree
	 */
	void enterExplist(FifthParser.ExplistContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#explist}.
	 * @param ctx the parse tree
	 */
	void exitExplist(FifthParser.ExplistContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_bool}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_bool(FifthParser.Exp_boolContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_bool}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_bool(FifthParser.Exp_boolContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_short}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_short(FifthParser.Exp_shortContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_short}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_short(FifthParser.Exp_shortContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_int}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_int(FifthParser.Exp_intContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_int}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_int(FifthParser.Exp_intContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_long}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_long(FifthParser.Exp_longContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_long}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_long(FifthParser.Exp_longContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_float}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_float(FifthParser.Exp_floatContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_float}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_float(FifthParser.Exp_floatContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_double}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_double(FifthParser.Exp_doubleContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_double}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_double(FifthParser.Exp_doubleContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_string}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_string(FifthParser.Exp_stringContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_string}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_string(FifthParser.Exp_stringContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_list}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_list(FifthParser.Exp_listContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_list}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_list(FifthParser.Exp_listContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_varname}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_varname(FifthParser.Exp_varnameContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_varname}
	 * labeled alternative in {@link FifthParser#literal_exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_varname(FifthParser.Exp_varnameContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#unary_exp}.
	 * @param ctx the parse tree
	 */
	void enterUnary_exp(FifthParser.Unary_expContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#unary_exp}.
	 * @param ctx the parse tree
	 */
	void exitUnary_exp(FifthParser.Unary_expContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#mult_exp}.
	 * @param ctx the parse tree
	 */
	void enterMult_exp(FifthParser.Mult_expContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#mult_exp}.
	 * @param ctx the parse tree
	 */
	void exitMult_exp(FifthParser.Mult_expContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#add_exp}.
	 * @param ctx the parse tree
	 */
	void enterAdd_exp(FifthParser.Add_expContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#add_exp}.
	 * @param ctx the parse tree
	 */
	void exitAdd_exp(FifthParser.Add_expContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_paren}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_paren(FifthParser.Exp_parenContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_paren}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_paren(FifthParser.Exp_parenContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_typecast}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_typecast(FifthParser.Exp_typecastContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_typecast}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_typecast(FifthParser.Exp_typecastContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_addexp}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_addexp(FifthParser.Exp_addexpContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_addexp}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_addexp(FifthParser.Exp_addexpContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_funccall}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_funccall(FifthParser.Exp_funccallContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_funccall}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_funccall(FifthParser.Exp_funccallContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_typecreateinst}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_typecreateinst(FifthParser.Exp_typecreateinstContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_typecreateinst}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_typecreateinst(FifthParser.Exp_typecreateinstContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_memberaccess}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void enterExp_memberaccess(FifthParser.Exp_memberaccessContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_memberaccess}
	 * labeled alternative in {@link FifthParser#exp}.
	 * @param ctx the parse tree
	 */
	void exitExp_memberaccess(FifthParser.Exp_memberaccessContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#truth_value}.
	 * @param ctx the parse tree
	 */
	void enterTruth_value(FifthParser.Truth_valueContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#truth_value}.
	 * @param ctx the parse tree
	 */
	void exitTruth_value(FifthParser.Truth_valueContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#absoluteIri}.
	 * @param ctx the parse tree
	 */
	void enterAbsoluteIri(FifthParser.AbsoluteIriContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#absoluteIri}.
	 * @param ctx the parse tree
	 */
	void exitAbsoluteIri(FifthParser.AbsoluteIriContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#alias}.
	 * @param ctx the parse tree
	 */
	void enterAlias(FifthParser.AliasContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#alias}.
	 * @param ctx the parse tree
	 */
	void exitAlias(FifthParser.AliasContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#iri}.
	 * @param ctx the parse tree
	 */
	void enterIri(FifthParser.IriContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#iri}.
	 * @param ctx the parse tree
	 */
	void exitIri(FifthParser.IriContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#iri_query_param}.
	 * @param ctx the parse tree
	 */
	void enterIri_query_param(FifthParser.Iri_query_paramContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#iri_query_param}.
	 * @param ctx the parse tree
	 */
	void exitIri_query_param(FifthParser.Iri_query_paramContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#qNameIri}.
	 * @param ctx the parse tree
	 */
	void enterQNameIri(FifthParser.QNameIriContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#qNameIri}.
	 * @param ctx the parse tree
	 */
	void exitQNameIri(FifthParser.QNameIriContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#list}.
	 * @param ctx the parse tree
	 */
	void enterList(FifthParser.ListContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#list}.
	 * @param ctx the parse tree
	 */
	void exitList(FifthParser.ListContext ctx);
	/**
	 * Enter a parse tree produced by the {@code EListLiteral}
	 * labeled alternative in {@link FifthParser#list_body}.
	 * @param ctx the parse tree
	 */
	void enterEListLiteral(FifthParser.EListLiteralContext ctx);
	/**
	 * Exit a parse tree produced by the {@code EListLiteral}
	 * labeled alternative in {@link FifthParser#list_body}.
	 * @param ctx the parse tree
	 */
	void exitEListLiteral(FifthParser.EListLiteralContext ctx);
	/**
	 * Enter a parse tree produced by the {@code EListComprehension}
	 * labeled alternative in {@link FifthParser#list_body}.
	 * @param ctx the parse tree
	 */
	void enterEListComprehension(FifthParser.EListComprehensionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code EListComprehension}
	 * labeled alternative in {@link FifthParser#list_body}.
	 * @param ctx the parse tree
	 */
	void exitEListComprehension(FifthParser.EListComprehensionContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#list_comp_constraint}.
	 * @param ctx the parse tree
	 */
	void enterList_comp_constraint(FifthParser.List_comp_constraintContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#list_comp_constraint}.
	 * @param ctx the parse tree
	 */
	void exitList_comp_constraint(FifthParser.List_comp_constraintContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#list_comp_generator}.
	 * @param ctx the parse tree
	 */
	void enterList_comp_generator(FifthParser.List_comp_generatorContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#list_comp_generator}.
	 * @param ctx the parse tree
	 */
	void exitList_comp_generator(FifthParser.List_comp_generatorContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#list_literal}.
	 * @param ctx the parse tree
	 */
	void enterList_literal(FifthParser.List_literalContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#list_literal}.
	 * @param ctx the parse tree
	 */
	void exitList_literal(FifthParser.List_literalContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#list_comprehension}.
	 * @param ctx the parse tree
	 */
	void enterList_comprehension(FifthParser.List_comprehensionContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#list_comprehension}.
	 * @param ctx the parse tree
	 */
	void exitList_comprehension(FifthParser.List_comprehensionContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#list_type_signature}.
	 * @param ctx the parse tree
	 */
	void enterList_type_signature(FifthParser.List_type_signatureContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#list_type_signature}.
	 * @param ctx the parse tree
	 */
	void exitList_type_signature(FifthParser.List_type_signatureContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#var_name}.
	 * @param ctx the parse tree
	 */
	void enterVar_name(FifthParser.Var_nameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#var_name}.
	 * @param ctx the parse tree
	 */
	void exitVar_name(FifthParser.Var_nameContext ctx);
}