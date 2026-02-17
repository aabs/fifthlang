# Research: Query Application and Result Type

**Feature**: Query Application and Result Type  
**Date**: 2025-11-15  
**Status**: Complete

## Purpose

This document consolidates technical research and design decisions for implementing SPARQL query application to RDF stores using the `<-` operator and a discriminated union `Result` type. All "NEEDS CLARIFICATION" items from the plan's Technical Context section are resolved here.

## Research Tasks Completed

### 1. Discriminated Union Implementation in C# / Fifth Type System

**Question**: How should Result type implement discriminated union semantics in Fifth's C# 14 / .NET 8.0 codebase?

**Decision**: Use pattern matching-friendly class hierarchy with abstract base + sealed derived cases

**Rationale**:
- Fifth already uses dunet library for discriminated unions in AST (per constitution's dependency list)
- C# 14 pattern matching provides exhaustiveness checking with sealed hierarchies
- Aligns with existing Fifth.System types (Query, Store use similar patterns)
- Enables natural syntax: `result switch { TabularResult t => ..., GraphResult g => ..., BooleanResult b => ... }`

**Implementation Pattern**:
```csharp
[Union]
public abstract partial class Result
{
    partial record TabularResult(SparqlResultSet ResultSet) : Result;
    partial record GraphResult(Store GraphStore) : Result;
    partial record BooleanResult(bool Value) : Result;
}
```

**Alternatives Considered**:
- **Option A** (Single type with nullable fields): Rejected - loses compile-time exhaustiveness, requires runtime null checks
- **Option B** (Separate unrelated types): Rejected - no common interface for operators, complicates type inference
- **Option C** (F# style DU): Rejected - not idiomatic in C# 14 codebase

### 2. ANTLR Grammar Token Precedence for `<-` Operator

**Question**: How to add `<-` without conflicts? Existing operators: `->` (function arrow), `<` (less than), `>` (greater than), `=` (assignment), `==` (equality)

**Decision**: Define `<-` as distinct lexer token with explicit character sequence match, positioned before fallback comparison operators in lexer

**Rationale**:
- ANTLR lexer matches longest token first; `<-` will match before separate `<` and `-` tokens
- No conflicts with `->` (different direction)
- No ambiguity with `<` followed by unary minus (context-dependent parsing handled by parser rules)
- Precedent: OCaml, Haskell use `<-` for binding/monad operations without conflicts

**FifthLexer.g4 Addition**:
```antlr
QUERY_APPLICATION_OP : '<-' ;
```
Position in lexer: After function arrow `->`, before comparison operators.

**FifthParser.g4 Addition**:
```antlr
queryApplicationExpr
    : expression QUERY_APPLICATION_OP expression  # QueryApplication
    ;
```
Integration: Add to `primaryExpression` alternatives or appropriate precedence level.

**Alternatives Considered**:
- **Option A** (Infix function `query.applyTo(store)`): Rejected - verbose, doesn't leverage operator ergonomics
- **Option B** (Reverse direction `store -> query`): Rejected - reads unnaturally ("store to query" vs "query from store")
- **Option C** (Symbol `.@` or `<=`): Rejected - `<=` overloads comparison, `.@` is non-standard

### 3. dotNetRDF Exception-to-QueryError.Kind Mapping

**Question**: How to map dotNetRDF's exception types to the 8-value Kind enumeration (SyntaxError, ValidationError, ExecutionError, Timeout, Cancellation, SecurityWarning, ResourceLimit, ConcurrencyConflict)?

**Decision**: Implement explicit mapping table in QueryApplicationExecutor with diagnostic enrichment

**Mapping Table**:
| dotNetRDF Exception | Kind | Notes |
|---------------------|------|-------|
| `RdfParseException` | SyntaxError | SPARQL query text parse failure |
| `SparqlQueryException` (semantic) | ValidationError | Undefined variables, constraint violations |
| `SparqlQueryException` (generic runtime) | ExecutionError | Query processing failure (e.g., function errors) |
| `RdfQueryTimeoutException` | Timeout | Execution limit exceeded |
| `OperationCanceledException` | Cancellation | User-requested cancellation token triggered |
| `OutOfMemoryException`, `InsufficientExecutionStackException` | ResourceLimit | Memory/stack exhaustion |
| (No direct equivalent) | SecurityWarning | Emitted by SparqlSecurityValidator pre-execution |
| (Future) | ConcurrencyConflict | Write/write conflicts when destructive operations added |

**Implementation Strategy**:
```csharp
private static QueryError MapException(Exception ex, string queryText)
{
    return ex switch
    {
        RdfParseException parseEx => new QueryError 
        {
            Kind = ErrorKind.SyntaxError,
            Message = parseEx.Message,
            SourceSpan = ExtractSpan(parseEx, queryText),
            UnderlyingExceptionType = nameof(RdfParseException),
            Suggestion = "Check SPARQL syntax near indicated position"
        },
        // ... other mappings
    };
}
```

**Rationale**:
- dotNetRDF exceptions contain structured info (line/column in RdfParseException)
- Mapping layer enriches diagnostics with SourceSpan and Suggestion fields
- SecurityWarning generated independently during validation pass (not from dotNetRDF)
- ConcurrencyConflict reserved for future coordinator layer

**Alternatives Considered**:
- **Option A** (1:1 mirror dotNetRDF types): Rejected - exposes implementation detail, no abstraction
- **Option B** (Generic "Error" only): Rejected - loses diagnostic precision for tooling

### 4. Streaming/Paging Strategy for Large SELECT Results

**Question**: How to implement FR-013's streaming requirement (100k rows, <1.5× memory, <10% throughput degradation)?

**Decision**: Wrap dotNetRDF's SparqlResultSet with lazy IEnumerable<SparqlResult> iterator, defer full materialization

**Implementation Approach**:
```csharp
public class TabularResult
{
    private readonly SparqlResultSet _resultSet;
    
    public IEnumerable<SparqlResult> Rows => _resultSet; // dotNetRDF already supports iteration
    
    public int RowCount => _resultSet.Count; // May force materialization
    
    // Lazy access by variable name
    public IEnumerable<T> GetColumn<T>(string varName) =>
        _resultSet.Select(r => (T)r[varName].AsValuedNode().AsObject());
}
```

**Rationale**:
- dotNetRDF's SparqlResultSet implements IEnumerable<SparqlResult> natively
- Iteration is lazy if underlying store supports streaming (most do)
- No custom buffer/paging logic needed for MVP
- Performance target achievable via Fifth.System wrapper controlling when to enumerate

**Benchmark Plan** (SC-009 validation):
- Compare Fifth's `result: Result = query <- store` vs direct C# `var results = store.ExecuteQuery(query)`
- Measure peak memory (via dotMemory or Benchmark.NET) and throughput (rows/sec)
- Target dataset: 100k triples, SELECT query returning 100k rows
- Pass criteria: ≤ 1.5× memory, ≤ 10% throughput delta

**Alternatives Considered**:
- **Option A** (Force full materialization): Rejected - violates FR-013 memory constraint
- **Option B** (Custom batch paging API): Deferred - over-engineering for MVP, add if benchmarks fail

### 5. SPARQL Injection Detection Patterns (FR-012)

**Question**: What unsafe patterns must SparqlSecurityValidator detect to satisfy SC-008 (30+ injection attempts rejected)?

**Decision**: Implement regex-based and structural validators detecting token-level injection attacks

**Validation Rules**:
1. **Unbalanced Braces**: `{` count ≠ `}` count (catches incomplete graph patterns)
2. **Prefix Smuggling**: Multiple `PREFIX` declarations for same namespace prefix
3. **Comment Escapes**: Trailing `#` without closing newline (allows appending unauthorized clauses)
4. **Rogue Commands**: Unauthorized keywords if disallowed: `DROP`, `LOAD`, `CREATE`, `CLEAR`, `COPY`, `MOVE`, `ADD`, `INSERT`, `DELETE`
5. **Mid-Token Backslash**: `\` outside string literals or URIs (per TriG parser error from terminal output)
6. **Unstructured Concatenation Warning**: Emit compiler warning when query text constructed via plain string concat/interpolation without safe markers

**Implementation Pseudo-Code**:
```csharp
public class SparqlSecurityValidator
{
    public List<QueryError> Validate(string queryText)
    {
        var errors = new List<QueryError>();
        
        if (CountChar(queryText, '{') != CountChar(queryText, '}'))
            errors.Add(SecurityWarning("Unbalanced graph pattern braces"));
        
        if (HasDuplicatePrefixes(queryText))
            errors.Add(SecurityWarning("Conflicting PREFIX declarations detected"));
        
        if (Regex.IsMatch(queryText, @"#(?!\s*\n).*$", RegexOptions.Multiline))
            errors.Add(SecurityWarning("Trailing comment may hide injected clauses"));
        
        foreach (var keyword in DisallowedKeywords)
            if (Regex.IsMatch(queryText, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
                errors.Add(SecurityWarning($"Unauthorized {keyword} operation detected"));
        
        return errors;
    }
}
```

**Test Matrix** (SC-008 coverage):
- Legitimate queries: multi-line SELECT, escaped string literals, nested OPTIONAL, UNION clauses
- Attack vectors: prefix hijacking, comment-based clause injection, mid-token backslash, unbalanced braces

**Rationale**:
- Complements runtime SPARQL parser (catches syntactic validity)
- Security validator targets semantic threats (injection, unauthorized operations)
- Warning-based approach (not hard failure) aligns with FR-012's "allow with validation" strategy

**Alternatives Considered**:
- **Option A** (Whitelist-only safe patterns): Rejected - too restrictive for legitimate complex queries
- **Option B** (Full ANTLR-based injection grammar): Deferred - complexity overhead for MVP

### 6. Cancellation Token Propagation Strategy

**Question**: How to implement FR-015's optional cancellation support without breaking existing Query/Store APIs?

**Decision**: Add optional CancellationToken parameter to QueryApplicationExecutor, propagate to dotNetRDF's query execution

**API Design**:
```csharp
public class QueryApplicationExecutor
{
    public static Result Execute(Query query, Store store, CancellationToken? cancellationToken = null)
    {
        var token = cancellationToken ?? CancellationToken.None;
        
        // dotNetRDF supports cancellation via IQueryCancellation interface
        var processor = new LeviathanQueryProcessor(store.TripleStore);
        var options = new SparqlQueryEvaluationOptions { Timeout = token };
        
        try
        {
            token.ThrowIfCancellationRequested();
            var results = processor.ProcessQuery(query.SparqlQuery, options);
            return ConvertToResult(results);
        }
        catch (OperationCanceledException)
        {
            return QueryError.Cancellation("Query cancelled by user request");
        }
    }
}
```

**Lowering Strategy**:
- QueryApplicationLoweringRewriter detects if cancellation token variable is in scope
- If present, passes to Execute call; otherwise omits parameter (defaults to None)

**Rationale**:
- Maintains backward compatibility (optional parameter)
- Leverages dotNetRDF's native cancellation mechanisms
- Satisfies SC-011 (< 200ms termination latency) via cooperative checks

**Alternatives Considered**:
- **Option A** (Always require token): Rejected - breaks existing code, violates constitution's compatibility principle
- **Option B** (Global timeout config): Rejected - less flexible than per-query cancellation

### 7. Concurrency Isolation Model (FR-016)

**Question**: How to ensure parallel read query safety without introducing heavy coordination?

**Decision**: Use ReaderWriterLockSlim in Store wrapper, read queries acquire read locks, future writes acquire write locks

**Implementation Sketch**:
```csharp
public class Store
{
    private readonly ITripleStore _tripleStore;
    private readonly ReaderWriterLockSlim _lock = new();
    
    internal Result ExecuteReadQuery(Query query, CancellationToken token)
    {
        _lock.EnterReadLock();
        try
        {
            return QueryApplicationExecutor.Execute(query, this, token);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    // Future: ExecuteWriteQuery acquires _lock.EnterWriteLock()
}
```

**Rationale**:
- Read/read concurrency is safe (multiple readers allowed)
- ReaderWriterLockSlim is lightweight for read-heavy workloads (per SC-012)
- dotNetRDF's in-memory stores are not inherently thread-safe; lock protects mutable state
- Satisfies SC-012 (≤ 5% latency variance) by avoiding coarse-grained global locks

**Benchmark Plan**:
- Spawn 25 parallel tasks executing identical SELECT query
- Measure per-query latency distribution (p50, p95, p99)
- Compare to isolated (single-threaded) baseline
- Pass criteria: ≤ 5% variance

**Alternatives Considered**:
- **Option A** (No coordination, assume immutable): Rejected - unsafe if future writes allowed
- **Option B** (Async semaphore): Deferred - adds complexity without proven benefit

## Summary of Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| Result Type | dunet-based sealed class hierarchy | Idiomatic C# 14, exhaustiveness checking |
| `<-` Token | Distinct lexer token before comparisons | Longest-match rule, no ambiguity |
| Error Mapping | Explicit switch-based exception handler | Diagnostic enrichment with SourceSpan/Suggestion |
| Streaming | Leverage dotNetRDF's IEnumerable iteration | Native support, no custom paging needed |
| Injection Detection | Regex + structural validators (6 rules) | Semantic threat coverage beyond parser |
| Cancellation | Optional CancellationToken parameter | Backward compatible, dotNetRDF native support |
| Concurrency | ReaderWriterLockSlim in Store | Lightweight read-heavy coordination |

## Open Follow-ups (Deferred to Implementation)

- ResourceLimit thresholds: Define memory budget (e.g., 500MB) and max row count trigger
- Pagination API ergonomics: Add if SC-009 benchmarks show streaming inadequate
- Observability hooks: Define logging points (query start/complete, result size, execution time)
- Bind() API design: Future structured binding syntax for FR-012 migration

## Next Phase

Proceed to Phase 1: Create data-model.md defining Result, QueryError, and QueryApplicationExp entities with full field specifications and relationships.
