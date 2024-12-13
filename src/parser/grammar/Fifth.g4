grammar Fifth;

options {
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
      (SUCH_THAT IDENTIFIER?)?
      // (QMARK iri_query_param (AMP iri_query_param)*)?
    ;

// ========[STATEMENTS]=========
block
    : L_CURLY statement* R_CURLY
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
    :  var_name COLON ( type_name | list_type_signature | array_type_signature )
    ;

var_name
    : IDENTIFIER
    ;

// ========[LISTS AND ARRAYS]=========
list
    : L_BRACKET body=list_body R_BRACKET
    ;

list_body
    : list_literal
    | list_comprehension
    ;

list_literal
    : expressionList
    ;

list_comprehension
    : varname=var_name IN source=expression (SUCH_THAT constraint=expression)
    ;

list_type_signature
    : L_BRACKET type_name R_BRACKET
    ;

array_type_signature
    :type_name  L_BRACKET size=operand R_BRACKET
    ;


// ========[EXPRESSIONS]=========

expressionList
    : expressions+=expression (COMMA expressions+=expression)*
    ;

expression
    : lhs=expression DOT rhs=expression                                                                                   #exp_member_access
    | lhs=expression POW<assoc=right> rhs=expression                                                                      #exp_exp
    | lhs=expression mul_op=(STAR | DIV | MOD | LSHIFT | RSHIFT | AMPERSAND | STAR_STAR) rhs=expression                   #exp_mul
    | lhs=expression add_op=(PLUS | MINUS | OR | LOGICAL_XOR) rhs=expression                                              #exp_add
    | lhs=expression rel_op=( EQUALS | NOT_EQUALS | LESS | LESS_OR_EQUALS | GREATER | GREATER_OR_EQUALS) rhs=expression   #exp_rel
    | lhs=expression LOGICAL_AND rhs=expression                                                                           #exp_and
    | lhs=expression LOGICAL_OR rhs=expression                                                                            #exp_or
    | expression L_PAREN expressionList? R_PAREN                                                                                  #exp_funccall
    | unary_op=(PLUS | MINUS | LOGICAL_NOT | PLUS_PLUS | MINUS_MINUS ) expression                                         #exp_unary
    | expression unary_op=(PLUS_PLUS | MINUS_MINUS )                                                                      #exp_unary_postfix
    | operand                                                                                                             #exp_operand
 ;

function_call_expression
    : un=function_name L_PAREN expressionList? R_PAREN
    ;

operand
    : literal
    | list
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




/*
 * A Fifth grammar for ANTLR 4 derived from the Go Grammar Specification
 * https://raw.githubusercontent.com/antlr/grammars-v4/refs/heads/master/golang/GoLexer.g4
 */

// $antlr-format alignTrailingComments true, columnLimit 150, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, minEmptyLines 0, alignSemicolons ownLine
// $antlr-format alignColons trailing, singleLineOverrulesHangingColon true, alignLexerCommands true, alignLabels true, alignTrailers true

// Keywords

ALIAS       : 'alias';
AS          : 'as';
BREAK       : 'break' /*-> mode(NLSEMI)*/;
CASE        : 'case';
CLASS       : 'class';
CONST       : 'const';
CONTINUE    : 'continue' /*-> mode(NLSEMI)*/;
DEFAULT     : 'default';
DEFER       : 'defer';
ELSE        : 'else';
FALLTHROUGH : 'fallthrough' /*-> mode(NLSEMI)*/;
FOR         : 'for';
FUNC        : 'func';
GO          : 'go';
GOTO        : 'goto';
IF          : 'if';
IMPORT      : 'import';
IN          : 'in';
INTERFACE   : 'interface';
MAP         : 'map';
NEW         : 'new';
PACKAGE     : 'package';
RANGE       : 'range';
RETURN      : 'return' /*-> mode(NLSEMI)*/;
SELECT      : 'select';
STRUCT      : 'struct';
SWITCH      : 'switch';
TYPE        : 'type';
USE         : 'use';
VAR         : 'var';
WHILE       : 'while';
WITH        : 'with';
TRUE        : 'true';
FALSE       : 'false';

NIL_LIT: 'null' /*-> mode(NLSEMI)*/;

IDENTIFIER: LETTER (LETTER | UNICODE_DIGIT)* /*-> mode(NLSEMI)*/;

// Punctuation

L_PAREN        : '(';
R_PAREN        : ')' /*-> mode(NLSEMI)*/;
L_CURLY        : '{';
R_CURLY        : '}' /*-> mode(NLSEMI)*/;
L_BRACKET      : '[';
R_BRACKET      : ']' /*-> mode(NLSEMI)*/;
ASSIGN         : '=';
COMMA          : ',';
SEMI           : ';';
COLON          : ':';
DOT            : '.';
PLUS_PLUS      : '++' /*-> mode(NLSEMI)*/;
MINUS_MINUS    : '--' /*-> mode(NLSEMI)*/;
STAR_STAR      : '**';
DECLARE_ASSIGN : ':=';
ELLIPSIS       : '...';
GEN            : '<-' ;
UNDERSCORE     : '_';
// Logical

LOGICAL_NOT  : '!';
LOGICAL_OR  : '||';
LOGICAL_AND : '&&';
LOGICAL_NAND: '!&';
LOGICAL_NOR: '!|';
LOGICAL_XOR: '~';

// Relation operators

EQUALS            : '==';
NOT_EQUALS        : '!=';
LESS              : '<';
LESS_OR_EQUALS    : '<=';
GREATER           : '>';
GREATER_OR_EQUALS : '>=';

// Arithmetic operators

OR         : '|';
DIV        : '/';
MOD        : '%';
LSHIFT     : '<<';
RSHIFT     : '>>';
POW        : '^';

// Unary operators



// Mixed operators

PLUS      : '+';
MINUS     : '-';
STAR      : '*';

AMPERSAND : '&';
SUCH_THAT : '#';
CONCAT    : '<>';

SUF_SHORT: [sS];
SUF_DECIMAL: [cC];
SUF_DOUBLE:  [dD];
SUF_LONG: [lL];

// Number literals

DECIMAL_LIT : ('0' | [1-9] ('_'? [0-9])*)   /*-> mode(NLSEMI)*/;
BINARY_LIT  : '0' [bB] ('_'? BIN_DIGIT)+    /*-> mode(NLSEMI)*/;
OCTAL_LIT   : '0' [oO]? ('_'? OCTAL_DIGIT)+ /*-> mode(NLSEMI)*/;
HEX_LIT     : '0' [xX] ('_'? HEX_DIGIT)+    /*-> mode(NLSEMI)*/;

REAL_LITERAL:
    ([0-9] ('_'* [0-9])*)? '.' [0-9] ('_'* [0-9])* ExponentPart? [FfDdMm]?
    | [0-9] ('_'* [0-9])* ([FfDdMm] | ExponentPart [FfDdMm]?)
;
fragment ExponentPart      : [eE] ('+' | '-')? [0-9] ('_'* [0-9])*;

FLOAT_LIT: (DECIMAL_FLOAT_LIT | HEX_FLOAT_LIT) /*-> mode(NLSEMI)*/;

DECIMAL_FLOAT_LIT: DECIMALS ('.' DECIMALS? EXPONENT? | EXPONENT) | '.' DECIMALS EXPONENT?;

HEX_FLOAT_LIT: '0' [xX] HEX_MANTISSA HEX_EXPONENT;

fragment HEX_MANTISSA:
    ('_'? HEX_DIGIT)+ ('.' ( '_'? HEX_DIGIT)*)?
    | '.' HEX_DIGIT ('_'? HEX_DIGIT)*
;

fragment HEX_EXPONENT: [pP] [+-]? DECIMALS;

IMAGINARY_LIT: (DECIMAL_LIT | BINARY_LIT | OCTAL_LIT | HEX_LIT | FLOAT_LIT) 'i' /*-> mode(NLSEMI)*/;

// Rune literals

fragment RUNE: '\'' (UNICODE_VALUE | BYTE_VALUE) '\''; //: '\'' (~[\n\\] | ESCAPED_VALUE) '\'';

RUNE_LIT: RUNE /*-> mode(NLSEMI)*/;

BYTE_VALUE: OCTAL_BYTE_VALUE | HEX_BYTE_VALUE;

OCTAL_BYTE_VALUE: '\\' OCTAL_DIGIT OCTAL_DIGIT OCTAL_DIGIT;

HEX_BYTE_VALUE: '\\' 'x' HEX_DIGIT HEX_DIGIT;

LITTLE_U_VALUE: '\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT;

BIG_U_VALUE:
    '\\' 'U' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
;

// String literals

RAW_STRING_LIT         : '`' ~'`'* '`'                     /*-> mode(NLSEMI)*/;
INTERPRETED_STRING_LIT : '"' (~["\\] | ESCAPED_VALUE)* '"' /*-> mode(NLSEMI)*/;
INTERPOLATED_STRING_LIT : '$' INTERPRETED_STRING_LIT /*-> mode(NLSEMI)*/;

// Hidden tokens

WS           : [ \t]+        -> channel(HIDDEN);
COMMENT      : '/*' .*? '*/' -> channel(HIDDEN);
TERMINATOR   : [\r\n]+       -> channel(HIDDEN);
LINE_COMMENT : '//' ~[\r\n]* -> channel(HIDDEN);

fragment UNICODE_VALUE: ~[\r\n'] | LITTLE_U_VALUE | BIG_U_VALUE | ESCAPED_VALUE;

// Fragments

fragment ESCAPED_VALUE:
    '\\' (
        'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
        | 'U' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
        | [abfnrtv\\'"]
        | OCTAL_DIGIT OCTAL_DIGIT OCTAL_DIGIT
        | 'x' HEX_DIGIT HEX_DIGIT
    )
;

fragment DECIMALS: [0-9] ('_'? [0-9])*;

fragment OCTAL_DIGIT: [0-7];

fragment HEX_DIGIT: [0-9a-fA-F];

fragment BIN_DIGIT: [01];

fragment EXPONENT: [eE] [+-]? DECIMALS;

fragment LETTER: UNICODE_LETTER | '_';

//[\p{Nd}] matches a digit zero through nine in any script except ideographic scripts
fragment UNICODE_DIGIT: [\p{Nd}];
//[\p{L}] matches any kind of letter from any language
fragment UNICODE_LETTER: [\p{L}];

// Treat whitespace as normal
WS_NLSEMI: [ \t]+ -> channel(HIDDEN);
// Ignore any comments that only span one line
COMMENT_NLSEMI      : '/*' ~[\r\n]*? '*/' -> channel(HIDDEN);
LINE_COMMENT_NLSEMI : '//' ~[\r\n]*       -> channel(HIDDEN);
// Emit an EOS token for any newlines, semicolon, multiline comments or the EOF and
//return to normal lexing
EOS: ([\r\n]+ | ';' | '/*' .*? '*/' | EOF) -> mode(DEFAULT_MODE);
// Did not find an EOS, so go back to normal lexing
//OTHER: -> mode(DEFAULT_MODE), channel(HIDDEN);
