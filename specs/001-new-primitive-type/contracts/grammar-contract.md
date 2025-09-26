# Grammar Contract: Triple Literal & Operators

## Lexer Changes
- Add keyword: `TRIPLE : 'triple';` inserted among other primitive/keyword tokens (ensure no collision)
- No change to IRIREF token; rely on parser differentiation.

## Parser Additions
```
tripleLiteral
  : '<' tripleSubject COMMA triplePredicate COMMA tripleObject '>'
  ;

tripleSubject
  : iri
  | var_name              # must type-check to IRI
  ;

triplePredicate
  : iri
  | var_name              # must type-check to IRI
  ;

tripleObject
  : iri
  | literal               # restricted to primitive literals (string/number/boolean)
  | var_name              # expression resolving to allowed object type
  | list                  # single-level list literal triggers expansion (semantic phase); nested lists invalid → TRPL006
  ;
```

## Integration Points
- Extend `literal:` rule with alternative `| tripleLiteral # lit_triple` OR introduce separate `operand` branch if ambiguity arises.
- Ensure `<{` still maps to `graphAssertionBlock` (parser lookahead distinguishes `{`).

## Ambiguity Avoidance
- Triple literal requires exactly two commas; parser predicate may reject if count != 2.

## Errors
- Arity violation: handled by parse error (fallback) or post-parse validation raising TRPL001.
- Nested list in object position: raise TRPL006 during semantic validation (list walker rejects nested list elements).
- Unresolved prefixed name: existing unresolved-prefix diagnostic (no new implicit resolution per FR-023).

## Mutating Operators
- Grammar already supports `PLUS_ASSIGN` and could add `MINUS_ASSIGN` if not present; use same tokens `+=` / `-=` on lvalue graph variable with rvalue triple or triple literal.
