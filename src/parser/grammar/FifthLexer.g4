lexer grammar FifthLexer;

/*
 * A Fifth lexer grammar for ANTLR 4 derived from the Go Grammar Specification
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
EXTENDS     : 'extends';
FALLTHROUGH : 'fallthrough' /*-> mode(NLSEMI)*/;
FOR         : 'for';
FUNC        : 'func';
GO          : 'go';
GOTO        : 'goto';
GRAPH       : 'graph';
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
SPARQL      : 'sparql_store';
STORE       : 'store';
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
L_GRAPH        : '<{';
R_GRAPH        : '}>' /*-> mode(NLSEMI)*/;
L_BRACKET      : '[';
R_BRACKET      : ']' /*-> mode(NLSEMI)*/;
ASSIGN         : '=';
COMMA          : ',';
SEMI           : ';';
COLON          : ':';
DOT            : '.';
PLUS_PLUS      : '++' /*-> mode(NLSEMI)*/;
MINUS_MINUS    : '--' /*-> mode(NLSEMI)*/;
PLUS_ASSIGN    : '+=';
STAR_STAR      : '**';
DECLARE_ASSIGN : ':=';
ELLIPSIS       : '...';
GEN            : '<-';
UNDERSCORE     : '_';
// Logical

LOGICAL_NOT  : '!';
LOGICAL_OR   : '||';
LOGICAL_AND  : '&&';
LOGICAL_NAND : '!&';
LOGICAL_NOR  : '!|';
LOGICAL_XOR  : '~';

// Relation operators

EQUALS            : '==';
NOT_EQUALS        : '!=';
LESS              : '<';
LESS_OR_EQUALS    : '<=';
GREATER           : '>';
GREATER_OR_EQUALS : '>=';

// Arithmetic operators

OR     : '|';
DIV    : '/';
MOD    : '%';
LSHIFT : '<<';
RSHIFT : '>>';
POW    : '^';

// Unary operators

// Mixed operators

PLUS  : '+';
MINUS : '-';
STAR  : '*';

AMPERSAND : '&';
SUCH_THAT : '#';
CONCAT    : '<>';

// IRI Reference - must come after L_GRAPH, GEN, CONCAT for proper precedence
// Required fragments moved from IRIMode for IRIREF
fragment PN_CHARS_BASE:
    'A' .. 'Z'
    | 'a' .. 'z'
    | '\u00C0' .. '\u00D6'
    | '\u00D8' .. '\u00F6'
    | '\u00F8' .. '\u02FF'
    | '\u0370' .. '\u037D'
    | '\u037F' .. '\u1FFF'
    | '\u200C' .. '\u200D'
    | '\u2070' .. '\u218F'
    | '\u2C00' .. '\u2FEF'
    | '\u3001' .. '\uD7FF'
    | '\uF900' .. '\uFDCF'
    | '\uFDF0' .. '\uFFFD'
;

fragment PN_CHARS_U: PN_CHARS_BASE | '_';

fragment PN_CHARS: PN_CHARS_U | '-' | [0-9] | '\u00B7' | [\u0300-\u036F] | [\u203F-\u2040];

fragment UCHAR:
    '\\u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
    | '\\U' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
;

// Require at least one of ':' '/' or '#' inside to avoid matching generics like '<int>'
IRIREF:
    '<' (PN_CHARS | '.' | ':' | '/' | '\\' | '#' | '@' | '%' | '&' | UCHAR)* (':' | '/' | '#') (
        PN_CHARS
        | '.'
        | ':'
        | '/'
        | '\\'
        | '#'
        | '@'
        | '%'
        | '&'
        | UCHAR
    )* '>'
;

SUF_SHORT   : [sS];
SUF_DECIMAL : [cC];
SUF_DOUBLE  : [dD];
SUF_LONG    : [lL];

// Number literals

DECIMAL_LIT : ('0' | [1-9] ('_'? [0-9])*) /*-> mode(NLSEMI)*/;
BINARY_LIT  : '0' [bB] ('_'? BIN_DIGIT)+ /*-> mode(NLSEMI)*/;
OCTAL_LIT   : '0' [oO]? ('_'? OCTAL_DIGIT)+ /*-> mode(NLSEMI)*/;
HEX_LIT     : '0' [xX] ('_'? HEX_DIGIT)+ /*-> mode(NLSEMI)*/;

REAL_LITERAL:
    ([0-9] ('_'* [0-9])*)? '.' [0-9] ('_'* [0-9])* ExponentPart? [FfDdMm]?
    | [0-9] ('_'* [0-9])* ([FfDdMm] | ExponentPart [FfDdMm]?)
;
fragment ExponentPart: [eE] ('+' | '-')? [0-9] ('_'* [0-9])*;

FLOAT_LIT: (DECIMAL_FLOAT_LIT | HEX_FLOAT_LIT) /*-> mode(NLSEMI)*/;

DECIMAL_FLOAT_LIT: DECIMALS ('.' DECIMALS? EXPONENT? | EXPONENT) | '.' DECIMALS EXPONENT?;

HEX_FLOAT_LIT: '0' [xX] HEX_MANTISSA HEX_EXPONENT;

fragment HEX_MANTISSA:
    ('_'? HEX_DIGIT)+ ('.' ( '_'? HEX_DIGIT)*)?
    | '.' HEX_DIGIT ('_'? HEX_DIGIT)*
;

fragment HEX_EXPONENT: [pP] [+-]? DECIMALS;

IMAGINARY_LIT: (DECIMAL_LIT | BINARY_LIT | OCTAL_LIT | HEX_LIT | FLOAT_LIT) 'i' /*-> mode(NLSEMI)*/
;

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

RAW_STRING_LIT          : '`' ~'`'* '`' /*-> mode(NLSEMI)*/;
INTERPRETED_STRING_LIT  : '"' (~["\\] | ESCAPED_VALUE)* '"' /*-> mode(NLSEMI)*/;
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

mode IRIMode;

PrefixedName : PNAME_LN | PNAME_NS;
PNAME_LN     : PNAME_NS PN_LOCAL;
PN_LOCAL: (PN_CHARS_U | ':' | [0-9] | PLX) ((PN_CHARS | '.' | ':' | PLX)* (PN_CHARS | ':' | PLX))?
;
PLX      : PERCENT | PN_LOCAL_ESC;
PERCENT  : '%' HEX_DIGIT HEX_DIGIT;
PNAME_NS : PN_PREFIX? ':';

PN_PREFIX: PN_CHARS_BASE ((PN_CHARS | '.')* PN_CHARS)?;
PN_LOCAL_ESC:
    '\\' (
        '_'
        | '~'
        | '.'
        | '-'
        | '!'
        | '$'
        | '&'
        | '\''
        | '('
        | ')'
        | '*'
        | '+'
        | ','
        | ';'
        | '='
        | '/'
        | '?'
        | '#'
        | '@'
        | '%'
    )
;