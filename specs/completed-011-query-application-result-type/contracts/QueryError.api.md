# API Contract: QueryError Type

**Namespace**: Fifth.System  
**Assembly**: fifthlang.system.dll  
**Version**: 1.0.0

## Overview

`QueryError` provides structured diagnostics for failed SPARQL query applications. It categorizes failures into 8 distinct error kinds with actionable messages, source location information, and fix suggestions.

## Type Signature

```csharp
namespace Fifth.System
{
    public sealed class QueryError
    {
        public ErrorKind Kind { get; init; }
        public string Message { get; init; }
        public SourceSpan? SourceSpan { get; init; }
        public string? UnderlyingExceptionType { get; init; }
        public string? Suggestion { get; init; }
    }

    public enum ErrorKind
    {
        SyntaxError,
        ValidationError,
        ExecutionError,
        Timeout,
        Cancellation,
        SecurityWarning,
        ResourceLimit,
        ConcurrencyConflict
    }
}
```

## Properties

### Kind (Required)

**Type**: `ErrorKind` (enum)  
**Description**: Categorizes the error into one of 8 failure modes  
**Constraint**: Must be valid ErrorKind value

**Values**:
| Kind | Semantic Meaning | Typical Cause |
|------|------------------|---------------|
| SyntaxError | SPARQL query text is malformed | Missing semicolons, unclosed brackets, typos |
| ValidationError | Query is syntactically valid but semantically incorrect | Undefined variables, constraint violations |
| ExecutionError | Query execution failed during processing | Function errors, type mismatches in FILTER |
| Timeout | Query exceeded time limit | Complex graph patterns, large datasets |
| Cancellation | User requested cancellation | CancellationToken triggered |
| SecurityWarning | Unsafe query composition detected | Injection patterns, unbalanced braces |
| ResourceLimit | Memory or result size limit exceeded | Too many results, memory exhaustion |
| ConcurrencyConflict | Conflicting concurrent operation | Write/write conflict (future feature) |

---

### Message (Required)

**Type**: `string`  
**Description**: Human-readable error description  
**Constraint**: Non-empty, ≤ 500 characters

**Format Guidelines**:
- Start with error kind context (e.g., "SPARQL syntax error:")
- Include specific location if available (e.g., "at line 5, column 12")
- State the immediate problem (e.g., "unexpected token '}'")
- Avoid overly technical jargon; target developer-friendly language

**Example Messages**:
```csharp
SyntaxError: "SPARQL syntax error at line 3, column 15: unexpected token '}', expected triple pattern"
ValidationError: "Undefined variable ?z in SELECT clause; variable not bound in WHERE clause"
ExecutionError: "Function xsd:dateTime evaluation failed: invalid date format '2023-13-45'"
Timeout: "Query execution exceeded 30 second timeout limit"
Cancellation: "Query execution cancelled by user request"
SecurityWarning: "Unbalanced braces in query text (3 opening, 2 closing); possible injection attempt"
ResourceLimit: "Result set exceeded 500MB memory limit after processing 2.5M rows"
ConcurrencyConflict: "Write conflict detected: concurrent modification to graph <http://example.org/data>"
```

---

### SourceSpan (Optional)

**Type**: `SourceSpan?` (value struct)  
**Description**: Location in query text where error originated  
**Constraint**: If present, Line > 0, Column > 0, Length ≥ 0

**Structure**:
```csharp
public readonly record struct SourceSpan(int Line, int Column, int Length, string? FilePath = null);
```

**Usage**:
- Populated for SyntaxError from ANTLR/dotNetRDF parse exceptions
- May be populated for ValidationError if error is localized (e.g., undefined variable at specific SELECT position)
- Typically null for Timeout, Cancellation, ResourceLimit (not source-specific)

**Example**:
```csharp
new SourceSpan(Line: 5, Column: 23, Length: 7, FilePath: "queries/analytics.5th")
// Points to 7-character token starting at line 5, column 23
```

---

### UnderlyingExceptionType (Optional)

**Type**: `string?`  
**Description**: Original dotNetRDF exception type name for debugging  
**Constraint**: If present, non-empty

**Purpose**: Enables developers to look up dotNetRDF documentation or stack traces

**Examples**:
- `"RdfParseException"` for SyntaxError
- `"SparqlQueryException"` for ValidationError or ExecutionError
- `"RdfQueryTimeoutException"` for Timeout
- `"OutOfMemoryException"` for ResourceLimit

---

### Suggestion (Optional, Target ≥80% Coverage)

**Type**: `string?`  
**Description**: Actionable fix hint to help developer resolve the error  
**Constraint**: If present, ≤ 200 characters

**Guidelines**:
- Be specific: reference the exact issue (e.g., "Add missing semicolon after line 5")
- Provide actionable steps: "Consider adding LIMIT clause to reduce result size"
- Link to documentation where appropriate: "See SPARQL 1.1 spec section 18.2 for ASK syntax"

**Example Suggestions**:
```csharp
SyntaxError: "Check for missing semicolon after triple pattern on line 5"
ValidationError: "Ensure all variables in SELECT clause appear in WHERE graph patterns"
ExecutionError: "Verify function arguments match expected types (expected xsd:integer, got xsd:string)"
Timeout: "Simplify graph pattern, add more specific constraints, or increase timeout limit"
Cancellation: null  // No suggestion needed (user-initiated)
SecurityWarning: "Review query construction logic; consider using bind() API for safer interpolation"
ResourceLimit: "Add LIMIT/OFFSET clauses or stream results iteratively to reduce memory usage"
ConcurrencyConflict: "Retry operation with exponential backoff or use pessimistic locking"
```

---

## Factory Methods

QueryError instances are typically created via factory methods in `QueryApplicationExecutor`:

```csharp
public static class QueryErrorFactory
{
    public static QueryError FromException(Exception ex, string queryText)
    {
        return ex switch
        {
            RdfParseException parseEx => new QueryError
            {
                Kind = ErrorKind.SyntaxError,
                Message = $"SPARQL syntax error at line {parseEx.Line}, column {parseEx.Column}: {parseEx.Message}",
                SourceSpan = new SourceSpan(parseEx.Line, parseEx.Column, 1),
                UnderlyingExceptionType = nameof(RdfParseException),
                Suggestion = "Check SPARQL syntax near indicated position"
            },
            
            SparqlQueryException queryEx when queryEx.Message.Contains("Undefined variable") => new QueryError
            {
                Kind = ErrorKind.ValidationError,
                Message = queryEx.Message,
                UnderlyingExceptionType = nameof(SparqlQueryException),
                Suggestion = "Ensure all projected variables are bound in WHERE clause"
            },
            
            RdfQueryTimeoutException timeoutEx => new QueryError
            {
                Kind = ErrorKind.Timeout,
                Message = $"Query execution exceeded {timeoutEx.Timeout} second timeout",
                UnderlyingExceptionType = nameof(RdfQueryTimeoutException),
                Suggestion = "Simplify graph pattern or increase timeout limit"
            },
            
            OperationCanceledException => new QueryError
            {
                Kind = ErrorKind.Cancellation,
                Message = "Query execution cancelled by user request",
                UnderlyingExceptionType = nameof(OperationCanceledException),
                Suggestion = null
            },
            
            OutOfMemoryException memEx => new QueryError
            {
                Kind = ErrorKind.ResourceLimit,
                Message = "Query result exceeded available memory",
                UnderlyingExceptionType = nameof(OutOfMemoryException),
                Suggestion = "Add LIMIT clause or enable result streaming"
            },
            
            _ => new QueryError
            {
                Kind = ErrorKind.ExecutionError,
                Message = $"Query execution failed: {ex.Message}",
                UnderlyingExceptionType = ex.GetType().Name,
                Suggestion = "Review query logic and data constraints"
            }
        };
    }
    
    public static QueryError SecurityWarning(string message, string suggestion)
    {
        return new QueryError
        {
            Kind = ErrorKind.SecurityWarning,
            Message = message,
            Suggestion = suggestion
        };
    }
}
```

---

## Usage in Fifth Code

Errors are communicated via exceptions in runtime execution:

```csharp
try {
    result: Result = query <- store;
    // Process result...
} catch (QueryExecutionException ex) {
    // ex.Error contains QueryError details
    ex.Error.Kind switch {
        ErrorKind.SyntaxError => std.print("Fix query syntax: " + ex.Error.Message),
        ErrorKind.Timeout => std.print("Query too slow, consider optimization"),
        ErrorKind.SecurityWarning => std.print("SECURITY ALERT: " + ex.Error.Suggestion),
        _ => std.print("Query failed: " + ex.Error.Message)
    }
}
```

---

## Serialization

QueryError supports JSON serialization for logging/diagnostics:

```json
{
  "kind": "SyntaxError",
  "message": "SPARQL syntax error at line 3, column 15: unexpected token '}'",
  "sourceSpan": {
    "line": 3,
    "column": 15,
    "length": 1,
    "filePath": "queries/analytics.5th"
  },
  "underlyingExceptionType": "RdfParseException",
  "suggestion": "Check for missing semicolon after triple pattern"
}
```

---

## Success Criteria Mapping

| Success Criterion | QueryError Field | Target |
|-------------------|------------------|--------|
| SC-010 (100% Kind population) | Kind | All errors have Kind value |
| SC-010 (≥80% Suggestion coverage) | Suggestion | ≥80% of errors include Suggestion |
| SC-013 (100% Kind enum coverage) | Kind | Tests cover all 8 ErrorKind values |

---

## Testing Strategy

### Unit Tests
- Verify all 8 ErrorKind values mappable from dotNetRDF exceptions
- Validate SourceSpan extraction from parse exceptions
- Ensure Suggestion field populated for common error scenarios
- Test JSON serialization round-trip

### Integration Tests
- Trigger each ErrorKind via real query execution
- Verify Suggestion quality (actionable, specific, ≤200 chars)
- Confirm SourceSpan accuracy for syntax errors

### Coverage Requirements (SC-013)
- At least 1 test case per ErrorKind value
- Positive tests: valid queries succeed
- Negative tests: known-bad queries produce expected ErrorKind

---

## Best Practices

1. **Always populate Kind and Message**: Required fields must never be null/empty
2. **Provide Suggestions liberally**: Target ≥80% coverage per SC-010
3. **Extract SourceSpan when available**: Improves IDE integration and error reporting
4. **Log UnderlyingExceptionType**: Aids debugging and dotNetRDF issue tracking
5. **Avoid leaking sensitive data**: Sanitize error messages in production (e.g., redact user data from query text)

---

## Security Considerations

- Message field may contain portions of user-provided query text; sanitize before logging
- SourceSpan FilePath may reveal internal file structure; redact in public-facing logs
- SecurityWarning kind must not be ignored; treat as potential attack indicator

---

## Future Enhancements

- Error code enumeration (e.g., `E001-E008` for automated tooling)
- Multi-language Message localization
- Rich diagnostics (QuickFix actions for IDEs)
- Error telemetry/analytics integration
