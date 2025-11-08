lexer grammar TrigLexer;

// TriG 1.2 Lexer grammar derived from W3C BNF: https://www.w3.org/TR/rdf12-trig/trig.bnf Designed
// for safe import/embedding within the Fifth language parser. Names are kept distinct; overlapping
// tokens (e.g. GRAPH, TRUE/FALSE) deliberately match Fifth definitions for consistent downstream
// handling. IMPORTANT: Do not introduce actions that mutate global state; keep pure tokenization.

// $antlr-format alignTrailingComments true, columnLimit 150, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, minEmptyLines 0, alignSemicolons ownLine
// $antlr-format alignColons trailing, singleLineOverrulesHangingColon true, alignLexerCommands true, alignLabels true, alignTrailers true

AT_PREFIX  : '@prefix';
AT_BASE    : '@base';
AT_VERSION : '@version';
PREFIX     : 'PREFIX';
BASE       : 'BASE';
VERSION    : 'VERSION';
GRAPH      : 'GRAPH';
A          : 'a'; // rdf:type short-form
TRUE       : 'true';
FALSE      : 'false';
TILDE      : '~';
HATHAT     : '^^';

LT2         : '<<';
GT2         : '>>';
LBRACE_PIPE : '{|';
PIPE_RBRACE : '|}';

// Punctuation / delimiters
LBRACE : '{';
RBRACE : '}';
LPAREN : '(';
RPAREN : ')';
LBRACK : '[';
RBRACK : ']';
DOT    : '.';
COMMA  : ',';
SEMI   : ';';

// Numeric literals
INTEGER           : SIGN? DIGIT+;
DECIMAL           : SIGN? (DIGIT* DOT DIGIT+);
DOUBLE            : SIGN? ( DIGIT+ DOT DIGIT* EXPONENT | DOT DIGIT+ EXPONENT | DIGIT+ EXPONENT);
fragment EXPONENT : [eE] SIGN? DIGIT+;
fragment SIGN     : [+-];
fragment DIGIT    : [0-9];

// String literals (four forms) + fragments
STRING_LITERAL_QUOTE             : '"' ( ~["\\\n\r] | ECHAR | UCHAR)* '"';
STRING_LITERAL_SINGLE_QUOTE      : '\'' ( ~['\\\n\r] | ECHAR | UCHAR)* '\'';
STRING_LITERAL_LONG_SINGLE_QUOTE : '\'\'\'' ( ( '\'' | '\'\'')? ( ~['\\] | ECHAR | UCHAR))* '\'\'\'';
STRING_LITERAL_LONG_QUOTE        : '"""' ( ( '"' | '""')? ( ~["\\] | ECHAR | UCHAR))* '"""';

fragment UCHAR : ('\\u' HEX HEX HEX HEX) | ('\\U' HEX HEX HEX HEX HEX HEX HEX HEX);
fragment ECHAR : '\\' ( 't' | 'b' | 'n' | 'r' | 'f' | '"' | '\'' | '\\');
fragment HEX   : [0-9a-fA-F];

// Language / direction tag
LANG_DIR: '@' [a-zA-Z]+ ( '-' [a-zA-Z0-9]+)* ( '--' [a-zA-Z]+)?;

// IRIREF (angle-bracket form)
IRIREF: '<' (~[\u0000-\u0020<>"{}|^`\\] | UCHAR)* '>';

// Prefixed names per Turtle/SPARQL forms
PNAME_NS : PN_PREFIX? ':';
PNAME_LN : PNAME_NS PN_LOCAL;

BLANK_NODE_LABEL : '_:' (PN_CHARS_U | [0-9]) ((PN_CHARS | DOT)* PN_CHARS)?;
ANON             : '[' WS* ']';

fragment PN_PREFIX: PN_CHARS_BASE ( ( PN_CHARS | DOT)* PN_CHARS)?;
fragment PN_LOCAL: (PN_CHARS_U | ':' | [0-9] | PLX) (
        ( PN_CHARS | DOT | ':' | PLX)* ( PN_CHARS | ':' | PLX)
    )?
;
fragment PLX     : PERCENT | PN_LOCAL_ESC;
fragment PERCENT : '%' HEX HEX;
fragment PN_LOCAL_ESC:
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

fragment PN_CHARS_U: PN_CHARS_BASE | '_';
fragment PN_CHARS:
    PN_CHARS_U
    | '-'
    | [0-9]
    | '\u00B7'
    | '\u0300' ..'\u036F'
    | '\u203F' ..'\u2040'
;
fragment PN_CHARS_BASE:
    [A-Z]
    | [a-z]
    | '\u00C0' ..'\u00D6'
    | '\u00D8' ..'\u00F6'
    | '\u00F8' ..'\u02FF'
    | '\u0370' ..'\u037D'
    | '\u037F' ..'\u1FFF'
    | '\u200C' ..'\u200D'
    | '\u2070' ..'\u218F'
    | '\u2C00' ..'\u2FEF'
    | '\u3001' ..'\uD7FF'
    | '\uF900' ..'\uFDCF'
    | '\uFDF0' ..'\uFFFD'
    | '\u{10000}' ..'\u{EFFFF}'
;

// Whitespace & comments
WS      : [ \t\r\n]+   -> skip; // TriG permits standard whitespace
COMMENT : '#' ~[\r\n]* -> skip;

// Ordering: Place LONG forms before SHORT to ensure longest match wins.
// Additional tokens used inside annotations / reified constructs.
HATHAT_WS: WS '^^' -> skip; // (optional helper if needed; kept hidden)