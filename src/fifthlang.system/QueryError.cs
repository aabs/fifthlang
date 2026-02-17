namespace Fifth.System;

/// <summary>
/// Structured diagnostics for failed SPARQL query applications.
/// Categorizes failures into 8 distinct error kinds with actionable messages.
/// </summary>
public sealed class QueryError
{
    public required ErrorKind Kind { get; init; }
    public required string Message { get; init; }
    public SourceSpan? SourceSpan { get; init; }
    public string? UnderlyingExceptionType { get; init; }
    public string? Suggestion { get; init; }
}

/// <summary>
/// Categorizes query execution failures into distinct failure modes.
/// </summary>
public enum ErrorKind
{
    /// <summary>SPARQL query text is malformed</summary>
    SyntaxError,
    
    /// <summary>Query is syntactically valid but semantically incorrect</summary>
    ValidationError,
    
    /// <summary>Query execution failed during processing</summary>
    ExecutionError,
    
    /// <summary>Query exceeded time limit</summary>
    Timeout,
    
    /// <summary>User requested cancellation</summary>
    Cancellation,
    
    /// <summary>Unsafe query composition detected</summary>
    SecurityWarning,
    
    /// <summary>Memory or result size limit exceeded</summary>
    ResourceLimit,
    
    /// <summary>Conflicting concurrent operation</summary>
    ConcurrencyConflict
}

/// <summary>
/// Location in query text where error originated.
/// </summary>
public readonly record struct SourceSpan(int Line, int Column, int Length, string? FilePath = null);

/// <summary>
/// Exception thrown when query execution fails with structured error details.
/// </summary>
public class QueryExecutionException : Exception
{
    public QueryError Error { get; }
    
    public QueryExecutionException(QueryError error) 
        : base(error.Message)
    {
        Error = error;
    }
    
    public QueryExecutionException(QueryError error, Exception innerException) 
        : base(error.Message, innerException)
    {
        Error = error;
    }
}
