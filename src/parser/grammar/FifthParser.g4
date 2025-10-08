// Restored baseline grammar with permissive tripleLiteral will be re-added below.
parser grammar FifthParser;

options {
	tokenVocab = FifthLexer;
	superClass = FifthParserBase;
}

fifth:
	module_import* alias* (colon_store_decl)* (
		functions += function_declaration
		| classes += class_definition
	)*;

module_import: USE module_name (COMMA module_name)* SEMI;
module_name: IDENTIFIER;
packagename: IDENTIFIER;

alias: ALIAS name = packagename AS iri SEMI;

// ========[FUNC DEFS]========= Ex: Foo(x:int, y:int):int { . . . }
function_declaration:
	name = function_name L_PAREN (
		args += paramdecl (COMMA args += paramdecl)*
	)? R_PAREN COLON result_type = type_name body = function_body;

function_body: block;

function_name: IDENTIFIER;

variable_constraint: OR constraint = expression;

// v2 Parameter declarations
paramdecl:
	var_name COLON type_name (
		variable_constraint
		| destructuring_decl
	)?;

destructuring_decl:
	L_CURLY bindings += destructure_binding (
		COMMA bindings += destructure_binding
	)* R_CURLY;

destructure_binding:
	name = IDENTIFIER COLON propname = IDENTIFIER (
		variable_constraint
		| destructuring_decl
	)?;

// ========[TYPE DEFINITIONS]=========
class_definition:
	CLASS name = IDENTIFIER (EXTENDS superClass = type_name)? (
		IN aliasScope = alias_scope_ref
	)? L_CURLY (
		functions += function_declaration
		| properties += property_declaration
	)* R_CURLY;

property_declaration:
	name = IDENTIFIER COLON type = IDENTIFIER SEMI;

type_name: IDENTIFIER;

// ========[STATEMENTS]=========
block: L_CURLY statement* R_CURLY;

graphAssertionBlock: L_GRAPH statement* R_GRAPH;

declaration: decl = var_decl (ASSIGN init = expression)? SEMI;

statement:
	block
	| graph_assertion_statement
	| if_statement
	| while_statement
	| with_statement // #stmt_with // this is not useful as is
	| assignment_statement
	| return_statement
	| expression_statement
	| declaration
	| colon_store_decl
	| colon_graph_decl;

graph_assertion_statement: graphAssertionBlock SEMI;

assignment_statement:
	lvalue = expression (op = ASSIGN | op = PLUS_ASSIGN | op = MINUS_ASSIGN) rvalue = expression SEMI;

expression_statement: expression? SEMI;

if_statement:
	IF L_PAREN condition = expression R_PAREN ifpart = statement (
		ELSE elsepart = statement
	)?;
return_statement: RETURN expression SEMI;
while_statement:
	WHILE L_PAREN condition = expression R_PAREN looppart = statement;

with_statement: WITH expression statement;

var_decl:
	var_name COLON (
		// Order matters: try more specific signatures before plain identifiers
		list_type_signature
		| array_type_signature
		| generic_type_signature
		| type_name
	);

var_name: IDENTIFIER;

// ========[LISTS AND ARRAYS]=========
list: L_BRACKET body = list_body R_BRACKET;

list_body: list_literal | list_comprehension;

list_literal: expressionList?;

list_comprehension:
	varname = var_name IN source = expression (
		SUCH_THAT constraint = expression
	);

list_type_signature: L_BRACKET type_name R_BRACKET;

array_type_signature:
	type_name L_BRACKET (size = operand)? R_BRACKET;

// ========[GENERIC TYPES]========= Supports syntax like: items: list<int>
generic_type_signature:
	generic_name = IDENTIFIER '<' inner = type_name '>';

// ========[EXPRESSIONS]=========

expressionList:
	expressions += expression (COMMA expressions += expression)*;

expression:
	lhs = expression DOT rhs = expression					# exp_member_access
	| lhs = expression index								# exp_index
	| funcname = IDENTIFIER L_PAREN expressionList? R_PAREN	# exp_funccall
	| expression unary_op = (PLUS_PLUS | MINUS_MINUS)	# exp_unary_postfix
	| unary_op = (
		PLUS
		| MINUS
		| LOGICAL_NOT
		| PLUS_PLUS
		| MINUS_MINUS
	) expression										# exp_unary
	| <assoc = right> lhs = expression POW rhs = expression	# exp_exp
	| lhs = expression mul_op = (
		STAR
		| DIV
		| MOD
		| LSHIFT
		| RSHIFT
		| AMPERSAND
		| STAR_STAR
	) rhs = expression																# exp_mul
	| lhs = expression add_op = (PLUS | MINUS | OR | LOGICAL_XOR) rhs = expression	# exp_add
	| lhs = expression rel_op = (
		EQUALS
		| NOT_EQUALS
		| LESS
		| LESS_OR_EQUALS
		| GREATER
		| GREATER_OR_EQUALS
	) rhs = expression										# exp_rel
	| lhs = expression LOGICAL_AND rhs = expression			# exp_and
	| lhs = expression LOGICAL_OR rhs = expression			# exp_or
	| operand											# exp_operand;

function_call_expression:
	un = function_name L_PAREN expressionList? R_PAREN;

operand:
	tripleExpression
	| literal
	| list
	| var_name
	| L_PAREN expression R_PAREN
	| graphAssertionBlock
	| object_instantiation_expression;

// Treat triples as primary (non-left-recursive) expressions Semantic predicate: ensure lookahead
// matches '<' followed by identifier and comma (for variable refs) or '<' IDENTIFIER ':' (for prefixed IRIs)
// This allows both <var1, var2, var3> and <ex:s, ex:p, ex:o> forms
tripleExpression:
	{ InputStream.LA(1) == LESS && InputStream.LA(2) == IDENTIFIER && (InputStream.LA(3) == COMMA || InputStream.LA(3) == COLON)
		}? (tripleLiteral | malformedTripleLiteral);

object_instantiation_expression:
	NEW type_name (
		L_PAREN (args += paramdecl (COMMA args += paramdecl)*)? R_PAREN
	)? (
		L_CURLY properties += initialiser_property_assignment (
			COMMA properties += initialiser_property_assignment
		)* R_CURLY
	)?;

initialiser_property_assignment: var_name ASSIGN expression;

index: L_BRACKET expression R_BRACKET;

// Primitive literals extracted to allow extension with tripleLiteral
primitiveLiteral:
	NIL_LIT			# lit_nil
	| integer		# lit_int
	| boolean		# lit_bool
	| string_		# lit_string
	| REAL_LITERAL	# lit_float;
// ===[ TRIPLE LITERALS ]=== Valid triple literal (subject, predicate, object)
tripleLiteral:
	'<' tripleSubject = tripleIriRef COMMA triplePredicate = tripleIriRef COMMA tripleObject =
		tripleObjectTerm '>' # triple_literal;

// Malformed variants captured for structured diagnostics (TRPL001)
malformedTripleLiteral:
	// Order: shorter/specific malformed patterns first, greedy tooMany last Missing object: only
	// two components
	'<' tripleIriRef COMMA tripleIriRef '>' # triple_malformed_missingObject
	// Trailing comma: has 3rd object component, then an extra comma before '>'
	| '<' tripleIriRef COMMA tripleIriRef COMMA tripleObjectTerm COMMA '>' #
		triple_malformed_trailingComma
	| '<' tripleIriRef COMMA tripleIriRef COMMA tripleIriRef COMMA tripleIriRef (
		COMMA tripleIriRef
	)* '>' # triple_malformed_tooMany;

tripleObjectTerm: tripleIriRef | primitiveLiteral | list;

// Allow either full IRI, prefixed alias form, or bare var reference (alias prefix resolution later)
prefixedIri: IDENTIFIER COLON IDENTIFIER;
// Allow both prefixed IRIs and expressions (variables, function calls, etc.) in triple components
tripleIriRef: prefixedIri | expression;

literal: primitiveLiteral;

string_:
	INTERPRETED_STRING_LIT		# str_plain
	| INTERPOLATED_STRING_LIT	# str_interpolated
	| RAW_STRING_LIT			# str_raw;

boolean: TRUE | FALSE;

integer:
	DECIMAL_LIT suffix = (SUF_SHORT | SUF_LONG)?	# num_decimal
	| BINARY_LIT									# num_binary
	| OCTAL_LIT										# num_octal
	| HEX_LIT										# num_hex
	| IMAGINARY_LIT									# num_imaginary
	| RUNE_LIT										# num_rune;

operandName: IDENTIFIER;

qualifiedIdent: IDENTIFIER DOT IDENTIFIER;

// =====[KNOWLEDGE MANAGEMENT]=========
iri: IRIREF;

graphDeclaration:
	GRAPH name = IDENTIFIER (IN aliasScope = alias_scope_ref)? ASSIGN L_CURLY assignment_statement*
		R_CURLY;

// Colon form graph variable: g : graph in <scope?> = <{ ... }>;
colon_graph_decl:
	name = IDENTIFIER COLON GRAPH (
		IN aliasScope = alias_scope_ref
	)? ASSIGN graphAssertionBlock SEMI;

// Prefer simple identifier first to avoid mispredicting IRI when both are viable
alias_scope_ref: IDENTIFIER | iri;

// Colon-form variant: name : store = sparql_store(<iri>);
colon_store_decl:
	store_name = IDENTIFIER COLON STORE ASSIGN SPARQL L_PAREN iri R_PAREN SEMI;