// Generated from /Users/aabs/dev/aabs/active/5th-related/ast-builder/src/parser/grammar/Fifth.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link FifthParser}.
 */
public interface FifthListener extends ParseTreeListener {
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
	 * Enter a parse tree produced by {@link FifthParser#declaration}.
	 * @param ctx the parse tree
	 */
	void enterDeclaration(FifthParser.DeclarationContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#declaration}.
	 * @param ctx the parse tree
	 */
	void exitDeclaration(FifthParser.DeclarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStatement(FifthParser.StatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStatement(FifthParser.StatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#assignment_statement}.
	 * @param ctx the parse tree
	 */
	void enterAssignment_statement(FifthParser.Assignment_statementContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#assignment_statement}.
	 * @param ctx the parse tree
	 */
	void exitAssignment_statement(FifthParser.Assignment_statementContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#expression_statement}.
	 * @param ctx the parse tree
	 */
	void enterExpression_statement(FifthParser.Expression_statementContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#expression_statement}.
	 * @param ctx the parse tree
	 */
	void exitExpression_statement(FifthParser.Expression_statementContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#if_statement}.
	 * @param ctx the parse tree
	 */
	void enterIf_statement(FifthParser.If_statementContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#if_statement}.
	 * @param ctx the parse tree
	 */
	void exitIf_statement(FifthParser.If_statementContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#return_statement}.
	 * @param ctx the parse tree
	 */
	void enterReturn_statement(FifthParser.Return_statementContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#return_statement}.
	 * @param ctx the parse tree
	 */
	void exitReturn_statement(FifthParser.Return_statementContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#while_statement}.
	 * @param ctx the parse tree
	 */
	void enterWhile_statement(FifthParser.While_statementContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#while_statement}.
	 * @param ctx the parse tree
	 */
	void exitWhile_statement(FifthParser.While_statementContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#with_statement}.
	 * @param ctx the parse tree
	 */
	void enterWith_statement(FifthParser.With_statementContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#with_statement}.
	 * @param ctx the parse tree
	 */
	void exitWith_statement(FifthParser.With_statementContext ctx);
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
	 * Enter a parse tree produced by {@link FifthParser#var_name}.
	 * @param ctx the parse tree
	 */
	void enterVar_name(FifthParser.Var_nameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#var_name}.
	 * @param ctx the parse tree
	 */
	void exitVar_name(FifthParser.Var_nameContext ctx);
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
	 * Enter a parse tree produced by {@link FifthParser#list_body}.
	 * @param ctx the parse tree
	 */
	void enterList_body(FifthParser.List_bodyContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#list_body}.
	 * @param ctx the parse tree
	 */
	void exitList_body(FifthParser.List_bodyContext ctx);
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
	 * Enter a parse tree produced by {@link FifthParser#array_type_signature}.
	 * @param ctx the parse tree
	 */
	void enterArray_type_signature(FifthParser.Array_type_signatureContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#array_type_signature}.
	 * @param ctx the parse tree
	 */
	void exitArray_type_signature(FifthParser.Array_type_signatureContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#expressionList}.
	 * @param ctx the parse tree
	 */
	void enterExpressionList(FifthParser.ExpressionListContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#expressionList}.
	 * @param ctx the parse tree
	 */
	void exitExpressionList(FifthParser.ExpressionListContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_mul}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_mul(FifthParser.Exp_mulContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_mul}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_mul(FifthParser.Exp_mulContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_and}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_and(FifthParser.Exp_andContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_and}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_and(FifthParser.Exp_andContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_rel}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_rel(FifthParser.Exp_relContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_rel}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_rel(FifthParser.Exp_relContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_operand}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_operand(FifthParser.Exp_operandContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_operand}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_operand(FifthParser.Exp_operandContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_unary_postfix}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_unary_postfix(FifthParser.Exp_unary_postfixContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_unary_postfix}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_unary_postfix(FifthParser.Exp_unary_postfixContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_unary}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_unary(FifthParser.Exp_unaryContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_unary}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_unary(FifthParser.Exp_unaryContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_exp}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_exp(FifthParser.Exp_expContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_exp}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_exp(FifthParser.Exp_expContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_member_access}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_member_access(FifthParser.Exp_member_accessContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_member_access}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_member_access(FifthParser.Exp_member_accessContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_funccall}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_funccall(FifthParser.Exp_funccallContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_funccall}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_funccall(FifthParser.Exp_funccallContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_or}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_or(FifthParser.Exp_orContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_or}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_or(FifthParser.Exp_orContext ctx);
	/**
	 * Enter a parse tree produced by the {@code exp_add}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExp_add(FifthParser.Exp_addContext ctx);
	/**
	 * Exit a parse tree produced by the {@code exp_add}
	 * labeled alternative in {@link FifthParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExp_add(FifthParser.Exp_addContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#function_call_expression}.
	 * @param ctx the parse tree
	 */
	void enterFunction_call_expression(FifthParser.Function_call_expressionContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#function_call_expression}.
	 * @param ctx the parse tree
	 */
	void exitFunction_call_expression(FifthParser.Function_call_expressionContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#operand}.
	 * @param ctx the parse tree
	 */
	void enterOperand(FifthParser.OperandContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#operand}.
	 * @param ctx the parse tree
	 */
	void exitOperand(FifthParser.OperandContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#object_instantiation_expression}.
	 * @param ctx the parse tree
	 */
	void enterObject_instantiation_expression(FifthParser.Object_instantiation_expressionContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#object_instantiation_expression}.
	 * @param ctx the parse tree
	 */
	void exitObject_instantiation_expression(FifthParser.Object_instantiation_expressionContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#initialiser_property_assignment}.
	 * @param ctx the parse tree
	 */
	void enterInitialiser_property_assignment(FifthParser.Initialiser_property_assignmentContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#initialiser_property_assignment}.
	 * @param ctx the parse tree
	 */
	void exitInitialiser_property_assignment(FifthParser.Initialiser_property_assignmentContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#index}.
	 * @param ctx the parse tree
	 */
	void enterIndex(FifthParser.IndexContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#index}.
	 * @param ctx the parse tree
	 */
	void exitIndex(FifthParser.IndexContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#slice_}.
	 * @param ctx the parse tree
	 */
	void enterSlice_(FifthParser.Slice_Context ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#slice_}.
	 * @param ctx the parse tree
	 */
	void exitSlice_(FifthParser.Slice_Context ctx);
	/**
	 * Enter a parse tree produced by the {@code lit_nil}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void enterLit_nil(FifthParser.Lit_nilContext ctx);
	/**
	 * Exit a parse tree produced by the {@code lit_nil}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void exitLit_nil(FifthParser.Lit_nilContext ctx);
	/**
	 * Enter a parse tree produced by the {@code lit_int}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void enterLit_int(FifthParser.Lit_intContext ctx);
	/**
	 * Exit a parse tree produced by the {@code lit_int}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void exitLit_int(FifthParser.Lit_intContext ctx);
	/**
	 * Enter a parse tree produced by the {@code lit_bool}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void enterLit_bool(FifthParser.Lit_boolContext ctx);
	/**
	 * Exit a parse tree produced by the {@code lit_bool}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void exitLit_bool(FifthParser.Lit_boolContext ctx);
	/**
	 * Enter a parse tree produced by the {@code lit_string}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void enterLit_string(FifthParser.Lit_stringContext ctx);
	/**
	 * Exit a parse tree produced by the {@code lit_string}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void exitLit_string(FifthParser.Lit_stringContext ctx);
	/**
	 * Enter a parse tree produced by the {@code lit_float}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void enterLit_float(FifthParser.Lit_floatContext ctx);
	/**
	 * Exit a parse tree produced by the {@code lit_float}
	 * labeled alternative in {@link FifthParser#literal}.
	 * @param ctx the parse tree
	 */
	void exitLit_float(FifthParser.Lit_floatContext ctx);
	/**
	 * Enter a parse tree produced by the {@code str_plain}
	 * labeled alternative in {@link FifthParser#string_}.
	 * @param ctx the parse tree
	 */
	void enterStr_plain(FifthParser.Str_plainContext ctx);
	/**
	 * Exit a parse tree produced by the {@code str_plain}
	 * labeled alternative in {@link FifthParser#string_}.
	 * @param ctx the parse tree
	 */
	void exitStr_plain(FifthParser.Str_plainContext ctx);
	/**
	 * Enter a parse tree produced by the {@code str_interpolated}
	 * labeled alternative in {@link FifthParser#string_}.
	 * @param ctx the parse tree
	 */
	void enterStr_interpolated(FifthParser.Str_interpolatedContext ctx);
	/**
	 * Exit a parse tree produced by the {@code str_interpolated}
	 * labeled alternative in {@link FifthParser#string_}.
	 * @param ctx the parse tree
	 */
	void exitStr_interpolated(FifthParser.Str_interpolatedContext ctx);
	/**
	 * Enter a parse tree produced by the {@code str_raw}
	 * labeled alternative in {@link FifthParser#string_}.
	 * @param ctx the parse tree
	 */
	void enterStr_raw(FifthParser.Str_rawContext ctx);
	/**
	 * Exit a parse tree produced by the {@code str_raw}
	 * labeled alternative in {@link FifthParser#string_}.
	 * @param ctx the parse tree
	 */
	void exitStr_raw(FifthParser.Str_rawContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#boolean}.
	 * @param ctx the parse tree
	 */
	void enterBoolean(FifthParser.BooleanContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#boolean}.
	 * @param ctx the parse tree
	 */
	void exitBoolean(FifthParser.BooleanContext ctx);
	/**
	 * Enter a parse tree produced by the {@code num_decimal}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void enterNum_decimal(FifthParser.Num_decimalContext ctx);
	/**
	 * Exit a parse tree produced by the {@code num_decimal}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void exitNum_decimal(FifthParser.Num_decimalContext ctx);
	/**
	 * Enter a parse tree produced by the {@code num_binary}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void enterNum_binary(FifthParser.Num_binaryContext ctx);
	/**
	 * Exit a parse tree produced by the {@code num_binary}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void exitNum_binary(FifthParser.Num_binaryContext ctx);
	/**
	 * Enter a parse tree produced by the {@code num_octal}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void enterNum_octal(FifthParser.Num_octalContext ctx);
	/**
	 * Exit a parse tree produced by the {@code num_octal}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void exitNum_octal(FifthParser.Num_octalContext ctx);
	/**
	 * Enter a parse tree produced by the {@code num_hex}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void enterNum_hex(FifthParser.Num_hexContext ctx);
	/**
	 * Exit a parse tree produced by the {@code num_hex}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void exitNum_hex(FifthParser.Num_hexContext ctx);
	/**
	 * Enter a parse tree produced by the {@code num_imaginary}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void enterNum_imaginary(FifthParser.Num_imaginaryContext ctx);
	/**
	 * Exit a parse tree produced by the {@code num_imaginary}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void exitNum_imaginary(FifthParser.Num_imaginaryContext ctx);
	/**
	 * Enter a parse tree produced by the {@code num_rune}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void enterNum_rune(FifthParser.Num_runeContext ctx);
	/**
	 * Exit a parse tree produced by the {@code num_rune}
	 * labeled alternative in {@link FifthParser#integer}.
	 * @param ctx the parse tree
	 */
	void exitNum_rune(FifthParser.Num_runeContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#operandName}.
	 * @param ctx the parse tree
	 */
	void enterOperandName(FifthParser.OperandNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#operandName}.
	 * @param ctx the parse tree
	 */
	void exitOperandName(FifthParser.OperandNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link FifthParser#qualifiedIdent}.
	 * @param ctx the parse tree
	 */
	void enterQualifiedIdent(FifthParser.QualifiedIdentContext ctx);
	/**
	 * Exit a parse tree produced by {@link FifthParser#qualifiedIdent}.
	 * @param ctx the parse tree
	 */
	void exitQualifiedIdent(FifthParser.QualifiedIdentContext ctx);
}