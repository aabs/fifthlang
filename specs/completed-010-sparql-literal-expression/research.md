# Research: Embedded SPARQL Queries

**Feature**: 001-sparql-literal-expression  
**Date**: 2025-11-13  
**Phase**: Phase 0 - Research & Decision Consolidation

## Overview

This document captures research findings and technical decisions for implementing SPARQL literal expressions in Fifth. The feature introduces inline SPARQL query syntax using `?< ... >` delimiters, mapping to a `Query` system type with safe variable binding.

## Key Technical Decisions

### Decision 1: SPARQL Parser Integration

**Decision**: Use dotNetRDF's `SparqlQueryParser` and `SparqlParameterizedString` for SPARQL validation and parameterization.

**Rationale**:
- dotNetRDF is already a dependency (used for TriG literals and knowledge graphs)
- Provides mature SPARQL 1.1 parser with error reporting
- Built-in parameterization API prevents injection attacks
- `SparqlParameterizedString.SetLiteral()` handles type-safe parameter binding
- Consistent with existing Fifth RDF infrastructure

**Alternatives Considered**:
- **Custom SPARQL parser from ANTLR grammar**: Rejected due to complexity; would duplicate dotNetRDF's extensive SPARQL support and require maintaining parity with W3C spec updates
- **String-based queries without validation**: Rejected due to lack of compile-time error detection and injection vulnerability

**Implementation Pattern**:
```csharp
// Compile-time validation
SparqlParameterizedString queryString = new SparqlParameterizedString();
queryString.CommandText = sparqlTextFromLiteral;
// Add variable bindings discovered during AST analysis
queryString.SetLiteral("variableName", typedValue);
// Validate at compile time
SparqlQueryParser parser = new SparqlQueryParser();
SparqlQuery query = parser.ParseFromString(queryString);
```

### Decision 2: Variable Binding Mechanism

**Decision**: Support two binding mechanisms:
1. **Direct reference** (P1 - required): In-scope Fifth variables referenced by name (e.g., `age`) are bound via `SparqlParameterizedString` using `@varname` syntax
2. **Interpolation** (P2 - optional): `{{expr}}` syntax for computed value injection using string interpolation

**Rationale**:
- Direct reference matches dotNetRDF's parameterization model (safe, type-preserving)
- Interpolation provides escape hatch for computed values (IRIs, complex expressions)
- Two-tier approach allows MVP with P1 only, deferring P2 complexity
- Parameter binding prevents injection by avoiding raw string concatenation

**Alternatives Considered**:
- **Interpolation only**: Rejected; doesn't leverage dotNetRDF's type-safe parameterization
- **Neither (plain text queries)**: Rejected; requires manual escaping and loses type safety

**Implementation Notes**:
- During AST building, scan SPARQL body for identifiers matching in-scope variables
- Create a binding table: `Dictionary<string, (FifthType type, Expression expr)>`
- At lowering/transformation, emit code to construct `SparqlParameterizedString` and call `SetLiteral()` for each binding
- For interpolation (`{{expr}}`), evaluate expression and serialize safely as typed literal or IRI

### Decision 3: AST Node Design

**Decision**: Add `SparqlLiteralExpression` to `AstMetamodel.cs` with these properties:
- `SparqlText: string` - raw SPARQL content
- `Bindings: List<(string Name, Expression Expr)>` - variable references
- `Interpolations: List<(int Position, Expression Expr)>` - interpolation sites
- `SourceSpan: TextSpan` - for diagnostics

**Rationale**:
- Follows existing literal pattern (e.g., `TrigLiteralExpression`)
- Captures all information needed for lowering and diagnostics
- Separates parsing concern (lexer/parser) from semantic analysis (bindings)

**Alternatives Considered**:
- **Store parsed `SparqlQuery` in AST**: Rejected; AST should be serializable and language-agnostic; parsing happens during lowering
- **Single binding mechanism**: Rejected; loses distinction between direct reference and interpolation

### Decision 4: Grammar Token Design

**Decision**: 
- Lexer tokens: `SPARQL_START: '?<'`, `SPARQL_CLOSE_ANGLE: '>'`
- Parser rule: `sparqlLiteral: SPARQL_START sparqlLiteralContent* SPARQL_CLOSE_ANGLE`
- Content handled as flexible token stream, allowing SPARQL keywords and Fifth variable names

**Rationale**:
- `?<` is unambiguous and visually distinct from existing syntax
- Follows TriG literal precedent (`<{ ... }>`)
- Avoids conflict with existing operators (`?` for conditional, `<` for comparison)
- Allows mixed SPARQL and Fifth identifier tokens within literal body

**Alternatives Considered**:
- **`sparql"..."`**: Rejected; conflicts with potential string literal prefix syntax
- **`@"..."`**: Rejected; conflicts with C# verbatim string syntax familiarity
- **`query { ... }`**: Rejected; `{}` already used for blocks, would require complex disambiguation

**Lexer Mode Strategy**:
- Use ANTLR lexer mode to switch context inside `?< ... >`
- Within mode, recognize SPARQL keywords, Fifth identifiers, and interpolation markers
- Exit mode on `>` token

### Decision 5: Type System Integration

**Decision**: Map `SparqlLiteralExpression` to system type `Fifth.System.Query`, user-facing as `Query`.

**Rationale**:
- Consistent with existing system types (e.g., `Triple`, `Graph`, `Store`)
- Provides compile-time type checking
- Enables method overloading and operator definitions for query execution
- Follows TriG literal precedent (`TrigLiteralExpression` → `Graph`)

**Implementation**:
- Add `Query.cs` to `src/fifthlang.system/` wrapping `VDS.RDF.Query.SparqlQuery`
- Type annotation visitor maps `SparqlLiteralExpression` → `Query`
- Runtime representation: instance of `VDS.RDF.Query.SparqlQuery`

**Alternatives Considered**:
- **Expose `VDS.RDF.Query.SparqlQuery` directly**: Rejected; leaks third-party types into Fifth language surface
- **String type with validation**: Rejected; loses type safety and enables incorrect usage

### Decision 6: Transformation Pipeline Integration

**Decision**: Add two new transformation passes:
1. **SparqlVariableBindingVisitor** (after SymbolTableBuilderVisitor): Resolves variable references in SPARQL literals against in-scope Fifth variables
2. **SparqlLoweringRewriter** (after TypeAnnotationVisitor): Constructs lowered AST nodes that emit query initialization code

**Rationale**:
- Follows multi-pass architecture
- Separates concerns: variable resolution vs code generation
- Leverages symbol table built by earlier pass
- Allows diagnostics at appropriate compilation phase

**Pipeline Position**:
```
TreeLinkageVisitor
  ↓
SymbolTableBuilderVisitor
  ↓
[NEW] SparqlVariableBindingVisitor ← resolve bindings
  ↓
TypeAnnotationVisitor ← type `SparqlLiteralExpression` → `Query`
  ↓
[NEW] SparqlLoweringRewriter ← emit query construction
  ↓
AstToIlTransformation / RoslynBackend
```

**Alternatives Considered**:
- **Single-pass resolution during AST building**: Rejected; too early (symbol table not yet available)
- **Inline during type checking**: Rejected; violates single-responsibility principle

### Decision 7: Diagnostics Strategy

**Decision**: Emit diagnostics at multiple phases:
- **Parse time**: Lexer/parser errors for malformed literal delimiters
- **Binding resolution**: Unknown variable names in SPARQL body
- **SPARQL validation**: Syntax errors within SPARQL text (via dotNetRDF parser)
- **Type checking**: Type mismatches for bound variables

**Rationale**:
- Early failure for syntax issues
- Clear error messages with source location (line/column within literal)
- Leverages dotNetRDF's error reporting for SPARQL validation

**Error Message Examples**:
```
error: Unknown variable 'age' referenced in SPARQL literal at line 5, column 42
error: Invalid SPARQL syntax: Expected 'WHERE' but found 'WEHRE' at line 3, column 15
error: Cannot bind variable 'name' of type 'int' as SPARQL literal (expected: string, IRI, or typed literal)
```

### Decision 8: Security & Injection Prevention

**Decision**: Enforce zero-concatenation policy:
- All variable values passed via `SparqlParameterizedString.SetLiteral()`
- Interpolation values serialized as typed literals (never raw text splicing)
- Compile-time validation prevents structural injection

**Rationale**:
- Follows established security best practices (parameterized queries)
- dotNetRDF handles escaping and type serialization
- Compile-time enforcement via code review and tests

**Validation**:
- Add security-focused tests attempting injection
- Code scan to verify no string concatenation in lowering code
- Success criterion SC-004 validates this requirement

## Implementation Sequence

### Phase 1: Grammar & AST Foundation (P1)
1. Update `FifthLexer.g4`: Add `SPARQL_START`, `SPARQL_CLOSE_ANGLE` tokens and lexer mode
2. Update `FifthParser.g4`: Add `sparqlLiteral` rule to `literal` alternatives
3. Update `AstMetamodel.cs`: Add `SparqlLiteralExpression` with properties above
4. Regenerate: `just run-generator`
5. Update `AstBuilderVisitor.cs`: Implement `VisitSparqlLiteral()` to construct AST node
6. Add parser tests: Valid/invalid syntax samples in `test_samples/sparql-literal.5th`

### Phase 2: System Type & Binding Resolution (P1)
1. Create `src/fifthlang.system/Query.cs`: Wrapper for `VDS.RDF.Query.SparqlQuery`
2. Implement `SparqlVariableBindingVisitor`: Scan literals for identifier references, resolve against symbol table
3. Add unit tests: Variable resolution scenarios (in-scope, out-of-scope, shadowing)

### Phase 3: Type Checking & Lowering (P1)
1. Update `TypeAnnotationVisitor`: Map `SparqlLiteralExpression` → `Query` type
2. Implement `SparqlLoweringRewriter`: Generate IL/Roslyn code for query construction
3. Add SPARQL validation: Call dotNetRDF parser, emit diagnostics on failure
4. Add AST tests: Type checking and lowering correctness

### Phase 4: Integration & Testing (P1)
1. Add runtime integration tests: End-to-end compilation and query object creation
2. Security tests: Injection attempt scenarios (all should fail at compile time)
3. Performance tests: Large literals, many bindings
4. Update documentation: `docs/knowledge-graphs.md`, quickstart samples

### Phase 5: Interpolation Support (P2 - Optional)
1. Extend lexer: Recognize `{{` and `}}` within SPARQL literal mode
2. Update `SparqlLiteralExpression`: Add interpolation tracking
3. Extend lowering: Evaluate interpolation expressions, serialize safely
4. Add tests: Interpolation scenarios, type conversions

## Open Questions & Future Work

### Deferred to Implementation
- **Lexer mode details**: Exact token recognition strategy within `?< ... >` (resolved during grammar implementation)
- **Error recovery**: How to handle partial SPARQL syntax for IDE support (deferred to LSP work)
- **Performance optimization**: Caching parsed queries, lazy validation (profile-guided)

### Future Enhancements
- **Query execution operators**: `query.execute(store)`, `store.query(...)` (depends on runtime design)
- **Query composition**: Combining/transforming queries (requires query algebra)
- **IDE support**: Syntax highlighting, completion inside literals (LSP integration)
- **Debugging**: Breakpoints/stepping within SPARQL execution (debugger protocol)

## References

- **dotNetRDF Documentation**: https://github.com/dotnetrdf/dotnetrdf/wiki
- **SPARQL 1.1 Spec**: https://www.w3.org/TR/sparql11-query/
- **Fifth TriG Literal Implementation**: `specs/completed-009-trig-literal-expression/`
- **ANTLR Lexer Modes**: https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md#lexer-rule-actions
- **Constitution Multi-Pass Compilation**: `.specify/memory/constitution.md` Section VII

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| SPARQL grammar conflicts with Fifth tokens | Low | Medium | Use lexer mode; test thoroughly |
| dotNetRDF parser exceptions at compile time | Medium | Low | Wrap in try/catch, emit diagnostic |
| Variable binding type mismatches | Medium | Medium | Validate during type checking phase |
| Performance degradation (large literals) | Low | Low | Set size limit (1MB), add diagnostic |
| Injection vulnerabilities | Low | High | Enforce parameterization, security tests |

## Success Metrics Mapping

This research supports all success criteria from spec.md:
- **SC-001**: dotNetRDF validation ensures valid samples parse
- **SC-002**: Parser/validator errors produce diagnostics
- **SC-003**: Parameterization pattern handles diverse types
- **SC-004**: Zero-concatenation policy enforced by design
