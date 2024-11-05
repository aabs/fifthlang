parser grammar FifthParser;

options {
    tokenVocab = FifthLexer;
    superClass = FifthParserBase;
}

fifth
    : module_import* alias*
    ( functions+=function_declaration
    | classes += class_definition
    )*
    ;

module_import
    : USE module_name (COMMA module_name)* SEMI
    ;
module_name
    : IDENTIFIER
    ;
packagename
    : IDENTIFIER
    ;

alias
    : ALIAS name=packagename AS uri=absoluteIri SEMI
    ;

// ========[FUNC DEFS]=========
// Ex: Foo(x:int, y:int):int { . . . }
function_declaration
    : name=function_name
        L_PAREN (args+=paramdecl (COMMA args+=paramdecl)*)? R_PAREN
      COLON result_type=type_name
      body=function_body
    ;

function_body
    : block
    ;

function_name
    : IDENTIFIER
    ;

variable_constraint
    : OR constraint=expression
    ;

// v2 Parameter declarations
paramdecl
    : var_name COLON type_name
        ( variable_constraint
        | destructuring_decl )?
    ;

destructuring_decl
    : L_CURLY
        bindings+=destructure_binding
        ( COMMA bindings+=destructure_binding )*
      R_CURLY
    ;

destructure_binding
    : name=IDENTIFIER COLON propname=IDENTIFIER
        ( variable_constraint
        | destructuring_decl )?
    ;

// ========[TYPE DEFINITIONS]=========
class_definition
    : CLASS
      name=IDENTIFIER
      L_CURLY
      ( functions += function_declaration
      | properties += property_declaration
      )*
      R_CURLY
    ;

property_declaration
    : name=IDENTIFIER  COLON type=IDENTIFIER SEMI
    ;

type_name
    :  IDENTIFIER
    ;

absoluteIri
    : iri_scheme=IDENTIFIER
      COLON DIV DIV
      iri_domain+=IDENTIFIER (DOT iri_domain+=IDENTIFIER)*
      (DIV iri_segment+=IDENTIFIER)*
      DIV?
      (HASH IDENTIFIER?)?
      // (QMARK iri_query_param (AMP iri_query_param)*)?
    ;

// ========[STATEMENTS]=========
block
    : '{' statement* '}'
    ;

declaration
    : decl=var_decl (ASSIGN init=expression)? SEMI
    ;

statement
    : block
    | if_statement
    | while_statement
    | with_statement         // #stmt_with // this is not useful as is
    | assignment_statement
    | return_statement
    | expression_statement
    | declaration
    ;

assignment_statement
    : lvalue=expression ASSIGN rvalue=expression SEMI
    ;

expression_statement
    : expression? SEMI
    ;

if_statement
    : IF L_PAREN condition=expression R_PAREN ifpart=statement (ELSE elsepart=statement)?
    ;
return_statement
    : RETURN expression SEMI
    ;
while_statement
    : WHILE L_PAREN condition=expression R_PAREN looppart=statement
    ;

with_statement
    : WITH expression  statement
    ;

var_decl
    :  var_name COLON ( type_name | list_type_signature )
    ;

var_name
    : IDENTIFIER
    ;

// ========[LISTS]=========
list
    : L_BRACKET body=list_body R_BRACKET
    ;

list_body
    : list_literal          #EListLiteral
    | list_comprehension    #EListComprehension
    ;

list_comp_constraint
    : expression // must be of type PrimitiveBoolean
    ;
list_comp_generator
    : varname=var_name GEN value=var_name
    ;

list_literal
    : expressionList
    ;

list_comprehension
    : varname=var_name OR gen=list_comp_generator (COMMA constraints=list_comp_constraint)
    ;

list_type_signature
    : type_name L_BRACKET R_BRACKET
    ;


// ========[EXPRESSIONS]=========

expressionList
    : expression (COMMA expression)*
    ;

expression
    : lhs=expression DOT rhs=expression                                                                                   #exp_member_access
    | lhs=expression POW<assoc=right> rhs=expression                                                                      #exp_exp
    | lhs=expression mul_op=(STAR | DIV | MOD | LSHIFT | RSHIFT | AMPERSAND | STAR_STAR) rhs=expression                   #exp_mul
    | lhs=expression add_op=(PLUS | MINUS | OR | LOGICAL_XOR) rhs=expression                                              #exp_add
    | lhs=expression rel_op=( EQUALS | NOT_EQUALS | LESS | LESS_OR_EQUALS | GREATER | GREATER_OR_EQUALS) rhs=expression   #exp_rel
    | lhs=expression LOGICAL_AND rhs=expression                                                                           #exp_and
    | lhs=expression LOGICAL_OR rhs=expression                                                                            #exp_or
    | expression '(' expressionList? ')'                                                                                  #exp_funccall
    | unary_op=(PLUS | MINUS | LOGICAL_NOT | PLUS_PLUS | MINUS_MINUS ) expression                                         #exp_unary
    | expression unary_op=(PLUS_PLUS | MINUS_MINUS )                                                                      #exp_unary_postfix
    | operand                                                                                                             #exp_operand
 ;

function_call_expression
    : un=function_name L_PAREN expressionList? R_PAREN
    ;

operand
    : literal
    | var_name
    | L_PAREN expression R_PAREN
    | object_instantiation_expression
    ;

object_instantiation_expression
    : NEW type_name
        L_PAREN (args+=paramdecl (COMMA args+=paramdecl)*)? R_PAREN
        (
        L_CURLY
        properties+=initialiser_property_assignment
        (COMMA
            properties+=initialiser_property_assignment
        )*
        R_CURLY
        )?
    ;

initialiser_property_assignment
    : var_name ASSIGN expression
    ;

index
    : L_BRACKET expression R_BRACKET
    ;

slice_
    : L_BRACKET (expression? COLON expression? | expression? COLON expression COLON expression) R_BRACKET
    ;

literal
    : NIL_LIT        #lit_nil
    | integer        #lit_int
    | boolean        #lit_bool
    | string_        #lit_string
    | REAL_LITERAL   #lit_float
    ;

string_
    : INTERPRETED_STRING_LIT   #str_plain
    | INTERPOLATED_STRING_LIT  #str_interpolated
    | RAW_STRING_LIT           #str_raw
    ;

boolean
    : TRUE
    | FALSE
    ;

integer
    : DECIMAL_LIT suffix=(SUF_SHORT|SUF_LONG)?      #num_decimal
    | BINARY_LIT                                    #num_binary
    | OCTAL_LIT                                     #num_octal
    | HEX_LIT                                       #num_hex
    | IMAGINARY_LIT                                 #num_imaginary
    | RUNE_LIT                                      #num_rune
    ;

operandName
    : IDENTIFIER
    ;

qualifiedIdent
    : IDENTIFIER DOT IDENTIFIER
    ;

