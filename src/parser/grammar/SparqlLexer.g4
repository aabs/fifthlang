lexer grammar SparqlLexer;

// SPARQL 1.2 Lexer grammar derived from W3C BNF: https://www.w3.org/TR/sparql12-query/
// Designed for safe import/embedding within the Fifth language parser. 
// Names are kept distinct to avoid token conflicts.
// IMPORTANT: Do not introduce actions that mutate global state; keep pure tokenization.

// $antlr-format alignTrailingComments true, columnLimit 150, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, minEmptyLines 0, alignSemicolons ownLine
// $antlr-format alignColons trailing, singleLineOverrulesHangingColon true, alignLexerCommands true, alignLabels true, alignTrailers true

// Keywords - Query Forms
SELECT      : [Ss][Ee][Ll][Ee][Cc][Tt];
CONSTRUCT   : [Cc][Oo][Nn][Ss][Tt][Rr][Uu][Cc][Tt];
DESCRIBE    : [Dd][Ee][Ss][Cc][Rr][Ii][Bb][Ee];
ASK         : [Aa][Ss][Kk];

// Keywords - Dataset
FROM        : [Ff][Rr][Oo][Mm];
NAMED       : [Nn][Aa][Mm][Ee][Dd];

// Keywords - Graph Patterns
WHERE       : [Ww][Hh][Ee][Rr][Ee];
GRAPH       : [Gg][Rr][Aa][Pp][Hh];
OPTIONAL    : [Oo][Pp][Tt][Ii][Oo][Nn][Aa][Ll];
UNION       : [Uu][Nn][Ii][Oo][Nn];
FILTER      : [Ff][Ii][Ll][Tt][Ee][Rr];
MINUS       : [Mm][Ii][Nn][Uu][Ss];
BIND        : [Bb][Ii][Nn][Dd];
SERVICE     : [Ss][Ee][Rr][Vv][Ii][Cc][Ee];
SILENT      : [Ss][Ii][Ll][Ee][Nn][Tt];
VALUES      : [Vv][Aa][Ll][Uu][Ee][Ss];

// Keywords - Solution Modifiers
GROUP       : [Gg][Rr][Oo][Uu][Pp];
BY          : [Bb][Yy];
HAVING      : [Hh][Aa][Vv][Ii][Nn][Gg];
ORDER       : [Oo][Rr][Dd][Ee][Rr];
ASC         : [Aa][Ss][Cc];
DESC        : [Dd][Ee][Ss][Cc];
LIMIT       : [Ll][Ii][Mm][Ii][Tt];
OFFSET      : [Oo][Ff][Ff][Ss][Ee][Tt];

// Keywords - Modifiers
DISTINCT    : [Dd][Ii][Ss][Tt][Ii][Nn][Cc][Tt];
REDUCED     : [Rr][Ee][Dd][Uu][Cc][Ee][Dd];
AS          : [Aa][Ss];

// Keywords - Declarations
BASE        : [Bb][Aa][Ss][Ee];
PREFIX      : [Pp][Rr][Ee][Ff][Ii][Xx];

// Keywords - Built-in Functions (subset - most common ones)
STR         : [Ss][Tt][Rr];
LANG        : [Ll][Aa][Nn][Gg];
LANGMATCHES : [Ll][Aa][Nn][Gg][Mm][Aa][Tt][Cc][Hh][Ee][Ss];
DATATYPE    : [Dd][Aa][Tt][Aa][Tt][Yy][Pp][Ee];
BOUND       : [Bb][Oo][Uu][Nn][Dd];
IRI         : [Ii][Rr][Ii];
URI         : [Uu][Rr][Ii];
BNODE       : [Bb][Nn][Oo][Dd][Ee];
RAND        : [Rr][Aa][Nn][Dd];
ABS         : [Aa][Bb][Ss];
CEIL        : [Cc][Ee][Ii][Ll];
FLOOR       : [Ff][Ll][Oo][Oo][Rr];
ROUND       : [Rr][Oo][Uu][Nn][Dd];
CONCAT      : [Cc][Oo][Nn][Cc][Aa][Tt];
STRLEN      : [Ss][Tt][Rr][Ll][Ee][Nn];
UCASE       : [Uu][Cc][Aa][Ss][Ee];
LCASE       : [Ll][Cc][Aa][Ss][Ee];
ENCODE_FOR_URI : [Ee][Nn][Cc][Oo][Dd][Ee]'_'[Ff][Oo][Rr]'_'[Uu][Rr][Ii];
CONTAINS    : [Cc][Oo][Nn][Tt][Aa][Ii][Nn][Ss];
STRSTARTS   : [Ss][Tt][Rr][Ss][Tt][Aa][Rr][Tt][Ss];
STRENDS     : [Ss][Tt][Rr][Ee][Nn][Dd][Ss];
STRBEFORE   : [Ss][Tt][Rr][Bb][Ee][Ff][Oo][Rr][Ee];
STRAFTER    : [Ss][Tt][Rr][Aa][Ff][Tt][Ee][Rr];
YEAR        : [Yy][Ee][Aa][Rr];
MONTH       : [Mm][Oo][Nn][Tt][Hh];
DAY         : [Dd][Aa][Yy];
HOURS       : [Hh][Oo][Uu][Rr][Ss];
MINUTES     : [Mm][Ii][Nn][Uu][Tt][Ee][Ss];
SECONDS     : [Ss][Ee][Cc][Oo][Nn][Dd][Ss];
TIMEZONE    : [Tt][Ii][Mm][Ee][Zz][Oo][Nn][Ee];
TZ          : [Tt][Zz];
NOW         : [Nn][Oo][Ww];
UUID        : [Uu][Uu][Ii][Dd];
STRUUID     : [Ss][Tt][Rr][Uu][Uu][Ii][Dd];
MD5         : [Mm][Dd]'5';
SHA1        : [Ss][Hh][Aa]'1';
SHA256      : [Ss][Hh][Aa]'256';
SHA384      : [Ss][Hh][Aa]'384';
SHA512      : [Ss][Hh][Aa]'512';
COALESCE    : [Cc][Oo][Aa][Ll][Ee][Ss][Cc][Ee];
IF          : [Ii][Ff];
STRLANG     : [Ss][Tt][Rr][Ll][Aa][Nn][Gg];
STRDT       : [Ss][Tt][Rr][Dd][Tt];
SAMETERM    : [Ss][Aa][Mm][Ee][Tt][Ee][Rr][Mm];
ISIRI       : [Ii][Ss][Ii][Rr][Ii];
ISURI       : [Ii][Ss][Uu][Rr][Ii];
ISBLANK     : [Ii][Ss][Bb][Ll][Aa][Nn][Kk];
ISLITERAL   : [Ii][Ss][Ll][Ii][Tt][Ee][Rr][Aa][Ll];
ISNUMERIC   : [Ii][Ss][Nn][Uu][Mm][Ee][Rr][Ii][Cc];
ISTRIPLE    : [Ii][Ss][Tt][Rr][Ii][Pp][Ll][Ee];
REGEX       : [Rr][Ee][Gg][Ee][Xx];
SUBSTR      : [Ss][Uu][Bb][Ss][Tt][Rr];
REPLACE     : [Rr][Ee][Pp][Ll][Aa][Cc][Ee];
EXISTS      : [Ee][Xx][Ii][Ss][Tt][Ss];
NOT         : [Nn][Oo][Tt];
ADJUST      : [Aa][Dd][Jj][Uu][Ss][Tt];
TRIPLE      : [Tt][Rr][Ii][Pp][Ll][Ee];
SUBJECT     : [Ss][Uu][Bb][Jj][Ee][Cc][Tt];
PREDICATE   : [Pp][Rr][Ee][Dd][Ii][Cc][Aa][Tt][Ee];
OBJECT      : [Oo][Bb][Jj][Ee][Cc][Tt];

// Keywords - Aggregates
COUNT       : [Cc][Oo][Uu][Nn][Tt];
SUM         : [Ss][Uu][Mm];
MIN         : [Mm][Ii][Nn];
MAX         : [Mm][Aa][Xx];
AVG         : [Aa][Vv][Gg];
SAMPLE      : [Ss][Aa][Mm][Pp][Ll][Ee];
GROUP_CONCAT : [Gg][Rr][Oo][Uu][Pp]'_'[Cc][Oo][Nn][Cc][Aa][Tt];
SEPARATOR   : [Ss][Ee][Pp][Aa][Rr][Aa][Tt][Oo][Rr];

// Keywords - Logical
IN          : [Ii][Nn];

// Special keyword for RDF type
A           : 'a';

// Literals
TRUE        : [Tt][Rr][Uu][Ee];
FALSE       : [Ff][Aa][Ll][Ss][Ee];
UNDEF       : [Uu][Nn][Dd][Ee][Ff];

// Operators
OR          : '||';
AND         : '&&';
EQ          : '=';
NE          : '!=';
LT          : '<';
GT          : '>';
LE          : '<=';
GE          : '>=';
NOT_SIGN    : '!';
PLUS        : '+';
MINUS_SIGN  : '-';
STAR        : '*';
SLASH       : '/';
CARET       : '^';
HATHAT      : '^^';

// Punctuation
LPAREN      : '(';
RPAREN      : ')';
LBRACE      : '{';
RBRACE      : '}';
LBRACK      : '[';
RBRACK      : ']';
SEMI        : ';';
COMMA       : ',';
DOT         : '.';
PIPE        : '|';
QUESTION    : '?';
DOLLAR      : '$';

// Triple terms (SPARQL 1.2 feature)
LT2         : '<<';
GT2         : '>>';

// Numeric literals
INTEGER_POSITIVE : PLUS INTEGER;
DECIMAL_POSITIVE : PLUS DECIMAL;
DOUBLE_POSITIVE  : PLUS DOUBLE;
INTEGER_NEGATIVE : MINUS_SIGN INTEGER;
DECIMAL_NEGATIVE : MINUS_SIGN DECIMAL;
DOUBLE_NEGATIVE  : MINUS_SIGN DOUBLE;

INTEGER     : DIGIT+;
DECIMAL     : DIGIT* DOT DIGIT+;
DOUBLE      : DIGIT+ DOT DIGIT* EXPONENT 
            | DOT DIGIT+ EXPONENT 
            | DIGIT+ EXPONENT;

fragment EXPONENT : [eE] [+-]? DIGIT+;
fragment DIGIT    : [0-9];

// String literals
STRING_LITERAL1         : '\'' ( ~['\\\r\n] | ECHAR )* '\'';
STRING_LITERAL2         : '"' ( ~["\\\r\n] | ECHAR )* '"';
STRING_LITERAL_LONG1    : '\'\'\'' ( ( '\'' | '\'\'' )? ( ~['\\] | ECHAR ) )* '\'\'\'';
STRING_LITERAL_LONG2    : '"""' ( ( '"' | '""' )? ( ~["\\] | ECHAR ) )* '"""';

fragment ECHAR : '\\' [tbnrf\\"'];

// IRI references
IRIREF      : '<' ( ~[\u0000-\u0020<>"{}|^`\\] | UCHAR )* '>';

// Prefixed names
PNAME_NS    : PN_PREFIX? ':';
PNAME_LN    : PNAME_NS PN_LOCAL;

// Blank nodes
BLANK_NODE_LABEL : '_:' ( PN_CHARS_U | DIGIT ) ( ( PN_CHARS | DOT )* PN_CHARS )?;
ANON        : '[' WS* ']';

// Variables
VAR1        : QUESTION VARNAME;
VAR2        : DOLLAR VARNAME;

// Language tag
LANGTAG     : '@' [a-zA-Z]+ ( '-' [a-zA-Z0-9]+ )*;

// NIL (empty list)
NIL         : LPAREN WS* RPAREN;

// Unicode escapes
fragment UCHAR : '\\u' HEX HEX HEX HEX 
               | '\\U' HEX HEX HEX HEX HEX HEX HEX HEX;

// Character classes for names
fragment PN_CHARS_BASE : [A-Z] | [a-z] 
                       | [\u00C0-\u00D6] | [\u00D8-\u00F6] | [\u00F8-\u02FF]
                       | [\u0370-\u037D] | [\u037F-\u1FFF] | [\u200C-\u200D]
                       | [\u2070-\u218F] | [\u2C00-\u2FEF] | [\u3001-\uD7FF]
                       | [\uF900-\uFDCF] | [\uFDF0-\uFFFD];

fragment PN_CHARS_U : PN_CHARS_BASE | '_';

fragment PN_CHARS   : PN_CHARS_U | '-' | DIGIT | '\u00B7' 
                    | [\u0300-\u036F] | [\u203F-\u2040];

fragment PN_PREFIX  : PN_CHARS_BASE ( ( PN_CHARS | DOT )* PN_CHARS )?;

fragment PN_LOCAL   : ( PN_CHARS_U | ':' | DIGIT | PLX ) 
                      ( ( PN_CHARS | DOT | ':' | PLX )* ( PN_CHARS | ':' | PLX ) )?;

fragment PLX        : PERCENT | PN_LOCAL_ESC;

fragment PERCENT    : '%' HEX HEX;

fragment HEX        : [0-9] | [A-F] | [a-f];

fragment PN_LOCAL_ESC : '\\' ( '_' | '~' | '.' | '-' | '!' | '$' | '&' | '\'' 
                      | '(' | ')' | '*' | '+' | ',' | ';' | '=' 
                      | '/' | '?' | '#' | '@' | '%' );

fragment VARNAME    : ( PN_CHARS_U | DIGIT ) 
                      ( PN_CHARS_U | DIGIT | '\u00B7' | [\u0300-\u036F] | [\u203F-\u2040] )*;

// Whitespace and comments
WS          : [ \t\r\n]+ -> channel(HIDDEN);
COMMENT     : '#' ~[\r\n]* -> channel(HIDDEN);

fragment WS_CHAR : [ \t\r\n];
