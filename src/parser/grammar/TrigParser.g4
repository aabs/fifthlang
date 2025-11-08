parser grammar TrigParser;

options {
	tokenVocab = TrigLexer;
}

// TriG 1.2 Parser grammar based on https://www.w3.org/TR/rdf12-trig/trig.bnf Structured to be
// import-safe and free of target-language actions.

trigDoc: (directive | block)* EOF;

block:
	triplesOrGraph
	| wrappedGraph
	| triples2
	| GRAPH labelOrSubject wrappedGraph;

triplesOrGraph:
	labelOrSubject (wrappedGraph | predicateObjectList DOT)
	| reifiedTriple predicateObjectList? DOT;

triples2:
	blankNodePropertyList predicateObjectList? DOT
	| collection predicateObjectList DOT;

wrappedGraph: LBRACE triplesBlock? RBRACE;

triplesBlock: triples (DOT triplesBlock?)?;

labelOrSubject: iri | blankNode;

// Directives

directive:
	prefixID
	| base
	| version
	| sparqlPrefix
	| sparqlBase
	| sparqlVersion;

prefixID: AT_PREFIX PNAME_NS IRIREF DOT;
base: AT_BASE IRIREF DOT;

version: AT_VERSION versionSpecifier DOT;
versionSpecifier:
	STRING_LITERAL_QUOTE
	| STRING_LITERAL_SINGLE_QUOTE;

sparqlPrefix: PREFIX PNAME_NS IRIREF;
sparqlBase: BASE IRIREF;
sparqlVersion: VERSION versionSpecifier;

// Triples / Predicates / Objects

triples:
	subject predicateObjectList
	| blankNodePropertyList predicateObjectList?
	| reifiedTriple predicateObjectList?;

predicateObjectList: verb objectList (SEMI (verb objectList)?)*;

objectList: object annotation (COMMA object annotation)*;

verb: predicate | A;

subject: iri | BlankNode | collection;

predicate: iri;

object:
	iri
	| BlankNode
	| collection
	| blankNodePropertyList
	| literal
	| tripleTerm
	| reifiedTriple;

literal: rdfLiteral | numericLiteral | booleanLiteral;

blankNodePropertyList: LBRACK predicateObjectList RBRACK;

collection: LPAREN object* RPAREN;

numericLiteral: INTEGER | DECIMAL | DOUBLE;

rdfLiteral: string_ (LANG_DIR | HATHAT iri)?;

string_:
	STRING_LITERAL_QUOTE
	| STRING_LITERAL_SINGLE_QUOTE
	| STRING_LITERAL_LONG_SINGLE_QUOTE
	| STRING_LITERAL_LONG_QUOTE;

booleanLiteral: TRUE | FALSE;

iri: IRIREF | prefixedName;

prefixedName: PNAME_LN | PNAME_NS;

blankNode: BLANK_NODE_LABEL | ANON;

// Reification & Triple Terms (RDF-star)

reifier: TILDE (iri | blankNode)?;

reifiedTriple: LT2 rtSubject verb rtObject reifier? GT2;

rtSubject: iri | blankNode | reifiedTriple;

rtObject:
	iri
	| blankNode
	| literal
	| tripleTerm
	| reifiedTriple;

tripleTerm: LT2 LPAREN ttSubject verb ttObject RPAREN GT2;

ttSubject: iri | blankNode;

ttObject: iri | blankNode | literal | tripleTerm;

annotation: (reifier | annotationBlock)*;

annotationBlock: LBRACE_PIPE predicateObjectList PIPE_RBRACE;