grammar Fifth;

call_site:
      var_name                          # exp_callsite_varname
    | function_call                     # exp_callsite_func_call
    | OPENPAREN innerexp=exp CLOSEPAREN # exp_callsite_parenthesised
    ;


fifth
    : module_import* alias*
    ( functions+=function_declaration
    | classes += class_definition
    )*
    ;


function_call
    : function_name OPENPAREN exp (COMMA exp)* CLOSEPAREN
    ;

member_access_expression:
    segments+=call_site (DOT segments+=call_site)*
    ;

module_import
    : USE module_name (COMMA module_name)* SEMICOLON
    ;

module_name
    : IDENTIFIER
    ;

// the name of a package
packagename
    : IDENTIFIER
    ;

// ========[TYPE DEFINITIONS]=========
class_definition
    : CLASS
      name=IDENTIFIER
      OPENBRACE
      ( functions += function_declaration
      | properties += property_declaration
      )*
      CLOSEBRACE
    ;

property_declaration
    : name=IDENTIFIER  COLON type=IDENTIFIER SEMICOLON
    ;


type_initialiser
    : typename=type_name OPENBRACE
        properties+=type_property_init
        (COMMA
            properties+=type_property_init
        )*
      CLOSEBRACE
    ;

type_name
    :  IDENTIFIER
    ;

// Ex: foo.bar = 5 * 23
type_property_init
    : var_name ASSIGN exp
    ;

// ========[FUNC DEFS]=========
// Ex: Foo(x:int, y:int):int { . . . }
function_declaration
    : name=function_name
        OPENPAREN (args+=paramdecl (COMMA args+=paramdecl)*)? CLOSEPAREN
      COLON result_type=function_type
      body=function_body
    ;

function_body
    : block
    ;

function_name
    : identifier_chain
    ;

function_type
    : IDENTIFIER
    ;


variable_constraint
    : BAR constraint=exp
    ;

// v2 Parameter declarations
paramdecl
    : param_name COLON param_type
        ( variable_constraint
        | destructuring_decl )?
    ;

param_name
    : IDENTIFIER
    ;

param_type
    : identifier_chain
    ;

destructuring_decl
    : OPENBRACE
        bindings+=destructure_binding
        ( COMMA bindings+=destructure_binding )*
      CLOSEBRACE
    ;

destructure_binding
    : name=IDENTIFIER COLON propname=IDENTIFIER
        ( variable_constraint
        | destructuring_decl )?
    ;

// ========[STATEMENTS]=========
block
    : OPENBRACE (statement SEMICOLON)* CLOSEBRACE
    ;

statement
    : IF OPENPAREN condition=exp CLOSEPAREN ifpart=block (ELSE elsepart=block)? # stmt_ifelse
    | WHILE OPENPAREN condition=exp CLOSEPAREN looppart=block                   # stmt_while
    | WITH exp  block                                                           # stmt_with // this is not useful as is
    | decl=var_decl (ASSIGN exp)?                                               # stmt_vardecl
    | var_name ASSIGN exp                                                       # stmt_assignment
    | RETURN exp                                                                # stmt_return
    | exp                                                                       # stmt_bareexpression
    ;

var_decl
    :  var_name COLON ( type_name | list_type_signature )
    ;

identifier_chain
    : segments+=IDENTIFIER (DOT segments+=IDENTIFIER)*
    ;

// ========[EXPRESSIONS]=========
explist
    : exp (COMMA exp)*
    ;

exp
    : OPENPAREN type=type_name CLOSEPAREN subexp=exp                # exp_typecast
    | value=truth_value                                             # exp_bool
    | value=INT                                                     # exp_int
    | value=FLOAT                                                   # exp_double
    | value=STRING                                                  # exp_string
    | value=truth_value                                             # exp_boolean
    | value=list                                                    # exp_list
    | op=UNIOP exp                                                  # exp_uniexp
    | left=exp op=BINOP right=exp                                   # exp_binexp
    | var_name                                                      # exp_varname
    | funcname=function_name OPENPAREN (args=explist)? CLOSEPAREN   # exp_funccall
    | OPENPAREN innerexp=exp CLOSEPAREN                             # exp_paren
    | NEW type_initialiser                                          # exp_typecreateinst
    | member_access_expression                                      # exp_memberaccess
    ;

truth_value
    : value=TRUE | value=FALSE
    ;


// ========[KNOWLEDGE GRAPHS]=========
absoluteIri
    : iri_scheme=IDENTIFIER
      COLON DIVIDE DIVIDE
      iri_domain+=IDENTIFIER (DOT iri_domain+=IDENTIFIER)*
      (DIVIDE iri_segment+=IDENTIFIER)*
      DIVIDE?
      (HASH IDENTIFIER?)?
      // (QMARK iri_query_param (AMP iri_query_param)*)?
    ;

alias
    : ALIAS name=packagename AS uri=absoluteIri SEMICOLON
    ;

iri
    : qNameIri | absoluteIri
    ;


iri_query_param
    : name=IDENTIFIER ASSIGN val=IDENTIFIER
    ;

qNameIri
    : prefix=IDENTIFIER? COLON fragname=IDENTIFIER
    ;


list
    : OPENBRACK body=list_body CLOSEBRACK
    ;

list_body
    : list_literal          #EListLiteral
    | list_comprehension    #EListComprehension
    ;

list_comp_constraint
    : exp // must be of type PrimitiveBoolean
    ;
list_comp_generator
    : varname=var_name GEN value=var_name
    ;

list_literal
    : explist
    ;

list_comprehension
    : varname=var_name BAR gen=list_comp_generator (COMMA constraints=list_comp_constraint)
    ;

list_type_signature
    : type_name OPENBRACK CLOSEBRACK
    ;



var_name
    : IDENTIFIER
    ;




// ========[RESERVED WORDS]=========
ALIAS: 'alias';
AS: 'as';
CLASS: 'class' ;
ELSE: 'else';
FALSE: 'false' ;
IF: 'if';
LIST: 'list' ;
NEW: 'new';
RETURN: 'return';
USE: 'use';
TRUE: 'true' ;
WHILE: 'while';
WITH: 'with';



// ========[OPERATORS AND PUNCTUATION]=========

OP_LogicalNot: '!';
OP_ArithmeticAdd: '+';
OP_StringConcatenate: '+';
OP_ArithmeticNegative: '-';
OP_ArithmeticSubtract: '-';
OP_ArithmeticMultiply: '*';
OP_ArithmeticDivide: '/';
OP_ArithmeticRem: '#';
OP_ArithmeticMod: '%';
OP_ArithmeticPow: '**';
OP_BitwiseAnd: '&';
OP_BitwiseOr: '|';
OP_LogicalAnd: '&&';
OP_LogicalOr: '||';
OP_LogicalNand: '!&';
OP_LogicalNor: '!|';
OP_LogicalXor: '^';
OP_Equal: '==';
OP_NotEqual: '!=';
OP_LessThan: '<';
OP_GreaterThan: '>';
OP_LessThanOrEqual: '<=';
OP_GreaterThanOrEqual: '>=';
OP_BitwiseLeftShift: '<<';
OP_BitwiseRightShift: '>>';
// OP_TernaryCondition: ''; // ?:

UNIOP: 
  OP_LogicalNot
| OP_ArithmeticSubtract
;

BINOP: 
  OP_ArithmeticAdd
| OP_StringConcatenate
| OP_ArithmeticNegative
| OP_ArithmeticSubtract
| OP_ArithmeticMultiply
| OP_ArithmeticDivide
| OP_ArithmeticRem
| OP_ArithmeticMod
| OP_ArithmeticPow
| OP_BitwiseAnd
| OP_BitwiseOr
| OP_LogicalAnd
| OP_LogicalOr
| OP_LogicalNand
| OP_LogicalNor
| OP_LogicalXor
| OP_Equal
| OP_NotEqual
| OP_LessThan
| OP_GreaterThan
| OP_LessThanOrEqual
| OP_GreaterThanOrEqual
| OP_BitwiseLeftShift
| OP_BitwiseRightShift
;

LAMBDASEP: '=>';
HASH: '#';
DOT: '.';
GEN: '<-' ; // list comprehension generator
COLON: ':';
SEMICOLON: ';' ;
COMMA: ',';
ASSIGN: '=';    // equal
// AMP: '&';    // intersection
// AND: '&&';    // intersection
// BAR: '|';     // union
// DIVIDE: '/';     // division
// EQ: '==';    // equal
// GEQ: '>='   ; // greater-or-equal
// GT: '>'   ;  // greater-than
// LEQ: '<='   ; // less-or-equal
// LT: '<'    ; // less-than
// MINUS: '-';     // subtraction
// NEQ: '!='    ;// not-equal
// NOT: '!';     // negation
// OR: '||';     // union
// PERCENT: '%';     // modulo
// PLUS: '+';     // addition
// POWER: '^' ;
// TIMES: '*';     // multiplication
CLOSEBRACE: '}';
CLOSEBRACK: ']';
CLOSEPAREN: ')';
OPENBRACE: '{';
OPENBRACK: '[';
OPENPAREN: '(';
QMARK: '?';
UNDERSCORE: '_' ;

// CHARACTER CLASSES ETC
// IRI_PARAM_VALUE: (LETTER|DIGIT|URI_PUNCT)+;
IDENTIFIER: (LETTER|UNDERSCORE) (LETTER|DIGIT|UNDERSCORE)*;
fragment LETTER: [a-zA-Z];
// fragment URI_PUNCT: [%\-_+];
STRING:'"' (~["])* '"'  | '\'' (~['])* '\'';
fragment EXP: [eE] [+\-]? INT;
fragment DIGIT: [0-9];
fragment HEXDIGIT: [0-9a-fA-F];
fragment POSITIVEDIGIT: [1-9];
INT: '0' | '-'? POSITIVEDIGIT DIGIT*;
FLOAT: '-'? INT ('.' DIGIT+ ) EXP?;
WS: [ \t\n\r]+ -> skip;
