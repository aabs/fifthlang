parser grammar SparqlParser;

options {
    tokenVocab = SparqlLexer;
}

// SPARQL 1.2 Parser grammar derived from W3C BNF: https://www.w3.org/TR/sparql12-query/
// Designed for safe import/embedding within the Fifth language parser.
// Top-level rule name is 'queryUnit' to avoid conflicts with other grammars.

// $antlr-format alignTrailingComments true, columnLimit 150, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, minEmptyLines 0, alignSemicolons ownLine
// $antlr-format alignColons trailing, singleLineOverrulesHangingColon true, alignLexerCommands true, alignLabels true, alignTrailers true

// [1] QueryUnit
queryUnit : query EOF;

// [2] Query
query : prologue ( selectQuery | constructQuery | describeQuery | askQuery ) valuesClause;

// [3] Prologue
prologue : ( baseDecl | prefixDecl )*;

// [4] BaseDecl
baseDecl : BASE iriRef;

// [5] PrefixDecl
prefixDecl : PREFIX PNAME_NS iriRef;

// [6] SelectQuery
selectQuery : selectClause datasetClause* whereClause solutionModifier;

// [7] SubSelect
subSelect : selectClause whereClause solutionModifier valuesClause;

// [8] SelectClause
selectClause : SELECT ( DISTINCT | REDUCED )? 
               ( ( varOrProjection )+ | STAR );

varOrProjection : var 
                | LPAREN expression AS var RPAREN;

// [9] ConstructQuery
constructQuery : CONSTRUCT ( constructTemplate datasetClause* whereClause solutionModifier 
                           | datasetClause* WHERE LBRACE triplesTemplate? RBRACE solutionModifier );

// [10] DescribeQuery
describeQuery : DESCRIBE ( varOrIri+ | STAR ) datasetClause* whereClause? solutionModifier;

// [11] AskQuery
askQuery : ASK datasetClause* whereClause solutionModifier;

// [12] DatasetClause
datasetClause : FROM ( defaultGraphClause | namedGraphClause );

// [13] DefaultGraphClause
defaultGraphClause : sourceSelector;

// [14] NamedGraphClause
namedGraphClause : NAMED sourceSelector;

// [15] SourceSelector
sourceSelector : iri;

// [16] WhereClause
whereClause : WHERE? groupGraphPattern;

// [17] SolutionModifier
solutionModifier : groupClause? havingClause? orderClause? limitOffsetClauses?;

// [18] GroupClause
groupClause : GROUP BY groupCondition+;

// [19] GroupCondition
groupCondition : builtInCall 
               | functionCall 
               | LPAREN expression ( AS var )? RPAREN 
               | var;

// [20] HavingClause
havingClause : HAVING havingCondition+;

// [21] HavingCondition
havingCondition : constraint;

// [22] OrderClause
orderClause : ORDER BY orderCondition+;

// [23] OrderCondition
orderCondition : ( ASC | DESC ) brackettedExpression 
               | constraint 
               | var;

// [24] LimitOffsetClauses
limitOffsetClauses : limitClause offsetClause? 
                   | offsetClause limitClause?;

// [25] LimitClause
limitClause : LIMIT INTEGER;

// [26] OffsetClause
offsetClause : OFFSET INTEGER;

// [27] ValuesClause
valuesClause : ( VALUES dataBlock )?;

// [28] GroupGraphPattern
groupGraphPattern : LBRACE ( subSelect | groupGraphPatternSub ) RBRACE;

// [29] GroupGraphPatternSub
groupGraphPatternSub : triplesBlock? ( graphPatternNotTriples DOT? triplesBlock? )*;

// [30] TriplesBlock
triplesBlock : triplesSameSubjectPath ( DOT triplesBlock? )?;

// [31] GraphPatternNotTriples
graphPatternNotTriples : groupOrUnionGraphPattern 
                       | optionalGraphPattern 
                       | minusGraphPattern 
                       | graphGraphPattern 
                       | serviceGraphPattern 
                       | filter 
                       | bind 
                       | inlineData;

// [32] OptionalGraphPattern
optionalGraphPattern : OPTIONAL groupGraphPattern;

// [33] GraphGraphPattern
graphGraphPattern : GRAPH varOrIri groupGraphPattern;

// [34] ServiceGraphPattern
serviceGraphPattern : SERVICE SILENT? varOrIri groupGraphPattern;

// [35] Bind
bind : BIND LPAREN expression AS var RPAREN;

// [36] InlineData
inlineData : VALUES dataBlock;

// [37] DataBlock
dataBlock : inlineDataOneVar | inlineDataFull;

// [38] InlineDataOneVar
inlineDataOneVar : var LBRACE dataBlockValue* RBRACE;

// [39] InlineDataFull
inlineDataFull : ( NIL | LPAREN var* RPAREN ) LBRACE ( LPAREN dataBlockValue* RPAREN | NIL )* RBRACE;

// [40] DataBlockValue
dataBlockValue : iri 
               | rdfLiteral 
               | numericLiteral 
               | booleanLiteral 
               | UNDEF;

// [41] MinusGraphPattern
minusGraphPattern : MINUS groupGraphPattern;

// [42] GroupOrUnionGraphPattern
groupOrUnionGraphPattern : groupGraphPattern ( UNION groupGraphPattern )*;

// [43] Filter
filter : FILTER constraint;

// [44] Constraint
constraint : brackettedExpression 
           | builtInCall 
           | functionCall;

// [45] FunctionCall
functionCall : iri argList;

// [46] ArgList
argList : NIL 
        | LPAREN DISTINCT? expression ( COMMA expression )* RPAREN;

// [47] ExpressionList
expressionList : NIL 
               | LPAREN expression ( COMMA expression )* RPAREN;

// [48] ConstructTemplate
constructTemplate : LBRACE constructTriples? RBRACE;

// [49] ConstructTriples
constructTriples : triplesSameSubject ( DOT constructTriples? )?;

// [50] TriplesSameSubject
triplesSameSubject : varOrTerm propertyListNotEmpty 
                   | triplesNode propertyList;

// [51] PropertyList
propertyList : propertyListNotEmpty?;

// [52] PropertyListNotEmpty
propertyListNotEmpty : verb objectList ( reifier | annotation )* ( SEMI ( verb objectList ( reifier | annotation )* )? )*;

// [53] Verb
verb : varOrIri | A;

// [54] ObjectList
objectList : object ( COMMA object )*;

// [55] Object
object : graphNode;

// [56] TriplesSameSubjectPath
triplesSameSubjectPath : varOrTerm propertyListPath 
                       | triplesNodePath propertyListPath;

// [57] PropertyListPath
propertyListPath : propertyListPathNotEmpty?;

// [58] PropertyListPathNotEmpty
propertyListPathNotEmpty : ( verbPath | verbSimple ) objectListPath ( reifier | annotation )*
                          ( SEMI ( ( verbPath | verbSimple ) objectListPath ( reifier | annotation )* )? )*;

// [59] VerbPath
verbPath : path;

// [60] VerbSimple
verbSimple : var;

// [61] ObjectListPath
objectListPath : objectPath ( COMMA objectPath )*;

// [62] ObjectPath
objectPath : graphNodePath;

// [63] Path
path : pathAlternative;

// [64] PathAlternative
pathAlternative : pathSequence ( PIPE pathSequence )*;

// [65] PathSequence
pathSequence : pathEltOrInverse ( SLASH pathEltOrInverse )*;

// [66] PathElt
pathElt : pathPrimary pathMod?;

// [67] PathEltOrInverse
pathEltOrInverse : pathElt | CARET pathElt;

// [68] PathMod
pathMod : QUESTION | STAR | PLUS;

// [69] PathPrimary
pathPrimary : iri 
            | A 
            | NOT_SIGN pathNegatedPropertySet 
            | LPAREN path RPAREN;

// [70] PathNegatedPropertySet
pathNegatedPropertySet : pathOneInPropertySet 
                       | LPAREN ( pathOneInPropertySet ( PIPE pathOneInPropertySet )* )? RPAREN;

// [71] PathOneInPropertySet
pathOneInPropertySet : iri 
                     | A 
                     | CARET ( iri | A );

// [72] Integer (handled by token)
integer : INTEGER;

// [73] TriplesNode
triplesNode : collection | blankNodePropertyList;

// [74] BlankNodePropertyList
blankNodePropertyList : LBRACK propertyListNotEmpty RBRACK;

// [75] TriplesNodePath
triplesNodePath : collectionPath | blankNodePropertyListPath;

// [76] BlankNodePropertyListPath
blankNodePropertyListPath : LBRACK propertyListPathNotEmpty RBRACK;

// [77] Collection
collection : LPAREN graphNode+ RPAREN;

// [78] CollectionPath
collectionPath : LPAREN graphNodePath+ RPAREN;

// [79] GraphNode
graphNode : varOrTerm | triplesNode;

// [80] GraphNodePath
graphNodePath : varOrTerm | triplesNodePath;

// [81] VarOrTerm
varOrTerm : var | graphTerm;

// [82] VarOrIri
varOrIri : var | iri;

// [83] Var
var : VAR1 | VAR2;

// [84] GraphTerm
graphTerm : iri 
          | rdfLiteral 
          | numericLiteral 
          | booleanLiteral 
          | blankNode 
          | NIL 
          | tripleTerm;

// [85] TripleTerm (SPARQL 1.2 feature)
tripleTerm : LT2 dataValueTerm dataValueTerm dataValueTerm GT2;

// [86] DataValueTerm
dataValueTerm : iri 
              | rdfLiteral 
              | numericLiteral 
              | booleanLiteral 
              | var
              | tripleTerm
              | dataCollection
              | dataBlankNodePropertyList;

// Data collection (for use in triple terms)
dataCollection : LPAREN dataValueTerm* RPAREN;

// Data blank node property list (for use in triple terms)
dataBlankNodePropertyList : LBRACK dataPropertyListNotEmpty RBRACK;

// Property list for data terms
dataPropertyListNotEmpty : verb dataObjectList ( SEMI ( verb dataObjectList )? )*;

// Object list for data terms
dataObjectList : dataValueTerm ( COMMA dataValueTerm )*;

// [87] Expression
expression : conditionalOrExpression;

// [88] ConditionalOrExpression
conditionalOrExpression : conditionalAndExpression ( OR conditionalAndExpression )*;

// [89] ConditionalAndExpression
conditionalAndExpression : valueLogical ( AND valueLogical )*;

// [90] ValueLogical
valueLogical : relationalExpression;

// [91] RelationalExpression
relationalExpression : numericExpression 
                       ( EQ numericExpression 
                       | NE numericExpression 
                       | LT numericExpression 
                       | GT numericExpression 
                       | LE numericExpression 
                       | GE numericExpression 
                       | IN expressionList 
                       | NOT IN expressionList 
                       )?;

// [92] NumericExpression
numericExpression : additiveExpression;

// [93] AdditiveExpression
additiveExpression : multiplicativeExpression 
                    ( ( PLUS | MINUS_SIGN ) multiplicativeExpression 
                    | ( numericLiteralPositive | numericLiteralNegative ) ( ( STAR unaryExpression ) | ( SLASH unaryExpression ) )* 
                    )*;

// [94] MultiplicativeExpression
multiplicativeExpression : unaryExpression ( ( STAR | SLASH ) unaryExpression )*;

// [95] UnaryExpression
unaryExpression : NOT_SIGN primaryExpression 
                | PLUS primaryExpression 
                | MINUS_SIGN primaryExpression 
                | primaryExpression;

// [96] PrimaryExpression
primaryExpression : brackettedExpression 
                  | builtInCall 
                  | iriOrFunction 
                  | rdfLiteral 
                  | numericLiteral 
                  | booleanLiteral 
                  | var 
                  | tripleTerm;

// [97] BrackettedExpression
brackettedExpression : LPAREN expression RPAREN;

// [98] BuiltInCall - comprehensive list of built-in functions
builtInCall : aggregate
            | STR LPAREN expression RPAREN
            | LANG LPAREN expression RPAREN
            | LANGMATCHES LPAREN expression COMMA expression RPAREN
            | DATATYPE LPAREN expression RPAREN
            | BOUND LPAREN var RPAREN
            | IRI LPAREN expression RPAREN
            | URI LPAREN expression RPAREN
            | BNODE ( LPAREN expression RPAREN | NIL )
            | RAND NIL
            | ABS LPAREN expression RPAREN
            | CEIL LPAREN expression RPAREN
            | FLOOR LPAREN expression RPAREN
            | ROUND LPAREN expression RPAREN
            | CONCAT expressionList
            | substringExpression
            | STRLEN LPAREN expression RPAREN
            | strReplaceExpression
            | UCASE LPAREN expression RPAREN
            | LCASE LPAREN expression RPAREN
            | ENCODE_FOR_URI LPAREN expression RPAREN
            | CONTAINS LPAREN expression COMMA expression RPAREN
            | STRSTARTS LPAREN expression COMMA expression RPAREN
            | STRENDS LPAREN expression COMMA expression RPAREN
            | STRBEFORE LPAREN expression COMMA expression RPAREN
            | STRAFTER LPAREN expression COMMA expression RPAREN
            | YEAR LPAREN expression RPAREN
            | MONTH LPAREN expression RPAREN
            | DAY LPAREN expression RPAREN
            | HOURS LPAREN expression RPAREN
            | MINUTES LPAREN expression RPAREN
            | SECONDS LPAREN expression RPAREN
            | TIMEZONE LPAREN expression RPAREN
            | TZ LPAREN expression RPAREN
            | NOW NIL
            | UUID NIL
            | STRUUID NIL
            | MD5 LPAREN expression RPAREN
            | SHA1 LPAREN expression RPAREN
            | SHA256 LPAREN expression RPAREN
            | SHA384 LPAREN expression RPAREN
            | SHA512 LPAREN expression RPAREN
            | COALESCE expressionList
            | IF LPAREN expression COMMA expression COMMA expression RPAREN
            | STRLANG LPAREN expression COMMA expression RPAREN
            | STRDT LPAREN expression COMMA expression RPAREN
            | SAMETERM LPAREN expression COMMA expression RPAREN
            | ISIRI LPAREN expression RPAREN
            | ISURI LPAREN expression RPAREN
            | ISBLANK LPAREN expression RPAREN
            | ISLITERAL LPAREN expression RPAREN
            | ISNUMERIC LPAREN expression RPAREN
            | ISTRIPLE LPAREN expression RPAREN
            | regexExpression
            | existsFunc
            | notExistsFunc
            | ADJUST LPAREN expression COMMA expression RPAREN
            | TRIPLE LPAREN expression COMMA expression COMMA expression RPAREN
            | SUBJECT LPAREN expression RPAREN
            | PREDICATE LPAREN expression RPAREN
            | OBJECT LPAREN expression RPAREN;

// [99] RegexExpression
regexExpression : REGEX LPAREN expression COMMA expression ( COMMA expression )? RPAREN;

// [100] SubstringExpression
substringExpression : SUBSTR LPAREN expression COMMA expression ( COMMA expression )? RPAREN;

// [101] StrReplaceExpression
strReplaceExpression : REPLACE LPAREN expression COMMA expression COMMA expression ( COMMA expression )? RPAREN;

// [102] ExistsFunc
existsFunc : EXISTS groupGraphPattern;

// [103] NotExistsFunc
notExistsFunc : NOT EXISTS groupGraphPattern;

// [104] Aggregate
aggregate : COUNT LPAREN DISTINCT? ( STAR | expression ) RPAREN
          | SUM LPAREN DISTINCT? expression RPAREN
          | MIN LPAREN DISTINCT? expression RPAREN
          | MAX LPAREN DISTINCT? expression RPAREN
          | AVG LPAREN DISTINCT? expression RPAREN
          | SAMPLE LPAREN DISTINCT? expression RPAREN
          | GROUP_CONCAT LPAREN DISTINCT? expression ( SEMI SEPARATOR EQ string )? RPAREN;

// [105] iriOrFunction
iriOrFunction : iri argList?;

// [106] RDFLiteral
rdfLiteral : string ( LANGTAG | ( HATHAT iri ) )?;

// [107] NumericLiteral
numericLiteral : numericLiteralUnsigned 
               | numericLiteralPositive 
               | numericLiteralNegative;

// [108] NumericLiteralUnsigned
numericLiteralUnsigned : INTEGER | DECIMAL | DOUBLE;

// [109] NumericLiteralPositive
numericLiteralPositive : INTEGER_POSITIVE | DECIMAL_POSITIVE | DOUBLE_POSITIVE;

// [110] NumericLiteralNegative
numericLiteralNegative : INTEGER_NEGATIVE | DECIMAL_NEGATIVE | DOUBLE_NEGATIVE;

// [111] BooleanLiteral
booleanLiteral : TRUE | FALSE;

// [112] String
string : STRING_LITERAL1 
       | STRING_LITERAL2 
       | STRING_LITERAL_LONG1 
       | STRING_LITERAL_LONG2;

// [113] iri
iri : iriRef | prefixedName;

// [114] PrefixedName
prefixedName : PNAME_LN | PNAME_NS;

// [115] BlankNode
blankNode : BLANK_NODE_LABEL | ANON;

// Terminal references
iriRef : IRIREF;

// TriplesTemplate (used in ConstructQuery)
triplesTemplate : triplesSameSubject ( DOT triplesTemplate? )?;

// Reifier syntax (SPARQL 1.2)
// Reifiers assign an identifier to a triple for referencing in annotations
reifier : TILDE varOrIri;

// Annotation syntax (SPARQL 1.2)
// Annotations allow associating metadata with a triple pattern
annotation : LBRACE_PIPE propertyListNotEmpty PIPE_RBRACE;
