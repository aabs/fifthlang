/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2013-2014 by Bart Kiers
 * 
 * Adapted for Fifth language integration
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * An IRI lexer grammar based on: http://www.ietf.org/rfc/rfc3987.txt
 * Also see: https://github.com/bkiers/iri-parser
 */

// $antlr-format alignTrailingComments true, columnLimit 150, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, minEmptyLines 0, alignSemicolons ownLine
// $antlr-format alignColons trailing, singleLineOverrulesHangingColon true, alignLexerCommands true, alignLabels true, alignTrailers true

lexer grammar IriLexerFragments;

// IRIREF token - angle-bracketed IRI reference for Fifth language
// This matches the pattern used in Fifth: <http://example.org/path>
// Note: This is a simplified but RFC 3987 compliant version for angle-bracketed IRIs
// IMPORTANT: Requires at least one of ':', '/', or '#' to avoid matching generics like '<int>'
IRIREF:
    '<' (
        IRI_CHAR
        | IRI_RESERVED
        | IRI_UNRESERVED  
        | IRI_PCT_ENCODED
        | IRI_UCSCHAR
    )* (':'| '/' | '#') (
        IRI_CHAR
        | IRI_RESERVED
        | IRI_UNRESERVED  
        | IRI_PCT_ENCODED
        | IRI_UCSCHAR
    )* '>'
;

// Character classes based on RFC 3987

/// ucschar = %xA0-D7FF / %xF900-FDCF / %xFDF0-FFEF
///         / %x10000-1FFFD / %x20000-2FFFD / %x30000-3FFFD
///         / %x40000-4FFFD / %x50000-5FFFD / %x60000-6FFFD
///         / %x70000-7FFFD / %x80000-8FFFD / %x90000-9FFFD
///         / %xA0000-AFFFD / %xB0000-BFFFD / %xC0000-CFFFD
///         / %xD0000-DFFFD / %xE1000-EFFFD
fragment IRI_UCSCHAR:
    '\u00A0' .. '\uD7FF'
    | '\uF900' .. '\uFDCF'
    | '\uFDF0' .. '\uFFEF'
    | '\u{10000}' .. '\u{1FFFD}'
    | '\u{20000}' .. '\u{2FFFD}'
    | '\u{30000}' .. '\u{3FFFD}'
    | '\u{40000}' .. '\u{4FFFD}'
    | '\u{50000}' .. '\u{5FFFD}'
    | '\u{60000}' .. '\u{6FFFD}'
    | '\u{70000}' .. '\u{7FFFD}'
    | '\u{80000}' .. '\u{8FFFD}'
    | '\u{90000}' .. '\u{9FFFD}'
    | '\u{A0000}' .. '\u{AFFFD}'
    | '\u{B0000}' .. '\u{BFFFD}'
    | '\u{C0000}' .. '\u{CFFFD}'
    | '\u{D0000}' .. '\u{DFFFD}'
    | '\u{E1000}' .. '\u{EFFFD}'
;

/// iunreserved = ALPHA / DIGIT / "-" / "." / "_" / "~" / ucschar
fragment IRI_UNRESERVED:
    [a-zA-Z]
    | [0-9]
    | '-'
    | '.'
    | '_'
    | '~'
    | IRI_UCSCHAR
;

/// pct-encoded = "%" HEXDIG HEXDIG
fragment IRI_PCT_ENCODED:
    '%' [0-9a-fA-F] [0-9a-fA-F]
;

/// reserved = gen-delims / sub-delims
/// gen-delims = ":" / "/" / "?" / "#" / "[" / "]" / "@"
/// sub-delims = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
fragment IRI_RESERVED:
    ':'
    | '/'
    | '?'
    | '#'
    | '['
    | ']'
    | '@'
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
;

/// Generic IRI character (for convenience in IRIREF definition)
fragment IRI_CHAR:
    IRI_UNRESERVED
    | IRI_PCT_ENCODED
    | IRI_RESERVED
;
