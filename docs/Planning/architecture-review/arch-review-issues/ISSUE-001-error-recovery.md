# Parser Needs Error Recovery for IDE Support

**Labels:** `arch-review`, `parser`, `ide-support`, `critical`  
**Priority:** P0  
**Severity:** CRITICAL  
**Epic:** Architectural Improvements Q1-Q2 2026

## Problem Summary

The parser uses ANTLR with a `ThrowingErrorListener` that immediately terminates parsing on the first syntax error. This prevents implementation of IDE features and provides poor developer experience by only showing one error at a time.

## Current Behavior

- Parser throws exception on first syntax error
- Compilation stops immediately
- No partial AST produced
- Cannot show multiple errors
- IDE features impossible to implement

**Example:**
```csharp
// src/parser/ThrowingErrorListener.cs
public override void SyntaxError(...)
{
    throw new ParseException($"line {line}:{charPositionInLine} {msg}");
}
```

## Impact

### Blocks IDE Integration
- Cannot provide real-time syntax highlighting with errors
- Code completion requires partial AST
- "Go to definition" needs AST even with errors
- Inline diagnostics impossible
- Quick fixes cannot be implemented

### Poor Developer Experience  
- Must fix errors sequentially
- No incremental feedback during editing
- Forces waterfall debugging approach
- Cannot see all syntax errors at once

### Prevents LSP Implementation
- LSP requires continuous parsing with error tolerance
- Document synchronization needs partial results
- Cannot implement standard LSP features

## Requirements

### Resilient Parsing
1. Use ANTLR error recovery instead of throwing
2. Implement "panic mode" recovery at statement boundaries
3. Produce partial/error AST nodes for unparseable regions
4. Continue parsing to find all errors
5. Accumulate diagnostics instead of throwing

### Error Node Representation
```csharp
// Add to AstMetamodel.cs
public record ErrorNode(
    string ErrorMessage,
    SourceLocation Location,
    AstThing? PartialAst = null
) : AstThing;
```

### Visitor Pattern Support
- All visitors must handle `ErrorNode`
- Transformations should gracefully skip error regions
- Code generation should not process error nodes
- Symbol table should handle partial information

### Diagnostic Collection
- Replace exception-based errors with diagnostic collection
- Allow parser to accumulate multiple errors
- Return (AST, Diagnostics) tuple from parsing

## Acceptance Criteria

- [ ] Parser produces partial AST even with syntax errors
- [ ] All syntax errors in file are reported (not just first)
- [ ] Error nodes properly represented in AST
- [ ] All existing visitors handle error nodes without crashing
- [ ] Tests verify error recovery at different AST levels
- [ ] Documentation updated with error recovery strategy

## Implementation Notes

### Phase 1: Error Recovery Infrastructure
1. Remove ThrowingErrorListener
2. Implement custom error listener that collects diagnostics
3. Add ErrorNode to AST metamodel
4. Update code generator to regenerate visitors

### Phase 2: Error Recovery Strategy
1. Implement panic-mode recovery in ANTLR grammar
2. Test recovery at statement, expression, and declaration levels
3. Ensure partial ASTs are well-formed

### Phase 3: Visitor Updates
1. Add ErrorNode handling to DefaultRecursiveDescentVisitor
2. Update all custom visitors to handle ErrorNode
3. Test that transformations don't crash on error nodes

### Phase 4: Integration
1. Update Compiler.ParsePhase to handle diagnostics
2. Ensure error nodes don't break later phases
3. Add integration tests for error recovery

## References

- Architectural Review: `docs/architectural-review-2025.md` - Finding #1
- Roslyn's error recovery: https://github.com/dotnet/roslyn/wiki/Resilient-Syntax-Trees
- ANTLR error recovery: https://www.antlr.org/papers/erro.pdf
- Related Issues: Will enable #ISSUE-002 (LSP), #ISSUE-003 (Incremental Compilation)

## Estimated Effort

**8 weeks** (2 months)
- Week 1-2: Design error node representation and recovery strategy
- Week 3-4: Implement ANTLR error recovery
- Week 5-6: Update visitors to handle error nodes
- Week 7-8: Testing, validation, and documentation

## Dependencies

- None (foundational change)

## Enables

- Issue #002: LSP Implementation (requires partial ASTs)
- Issue #004: Diagnostic System (requires error collection)
- Better developer experience for all users
