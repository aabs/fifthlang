# Redesign Diagnostic System for Quality Error Messages and IDE Support

**Labels:** `arch-review`, `diagnostics`, `developer-experience`, `high`  
**Priority:** P1  
**Severity:** HIGH  
**Epic:** Architectural Improvements Q1 2026

## Problem Summary

The diagnostic system is fragmented across multiple mechanisms (exceptions, diagnostic records, string messages) with no source location tracking, no diagnostic codes, and inconsistent error reporting. This prevents high-quality error messages and limits tooling capabilities.

## Current Issues

### Multiple Diagnostic Mechanisms
1. `compiler.Diagnostic` record (simple messages)
2. `ast_model.CompilationException` and 5 other exception types
3. String-based error messages throughout visitors
4. Debug logging in various places
5. Guard validation has its own DiagnosticEmitter

### Missing Critical Features
- No consistent source location (line/column) tracking
- No diagnostic codes for stable error references
- No structured diagnostic data (for quick fixes)
- No diagnostic rendering/formatting infrastructure
- No related information or multi-line diagnostics
- No "did you mean?" suggestions

### Inconsistent Error Reporting
```csharp
// Some phases throw exceptions
throw new TypeCheckingException($"Type mismatch: {expected} vs {actual}");

// Some return null with diagnostics
diagnostics.Add(new Diagnostic(DiagnosticLevel.Error, cex.Message));
return null;

// Some have custom systems
var guardValidator = new GuardCompletenessValidator();
foreach (var diagnostic in guardValidator.Diagnostics)
    diagnostics.Add(diagnostic);
```

## Impact

### Poor Error Messages
- Cannot point to exact error location
- No multi-line diagnostics or related spans
- Cannot provide "did you mean?" suggestions
- Hard to understand complex errors
- Poor error message quality compared to Rust/TypeScript

### Tooling Limitations
- IDE cannot show inline errors at correct location
- Cannot implement quick fixes (need structured diagnostics)
- No way to suppress or filter specific errors
- Cannot generate error code documentation
- Hard to test specific error scenarios

### Maintenance Burden
- Adding new diagnostics requires changes in multiple places
- No central registry of all possible errors
- Diagnostic quality varies across compiler phases
- Hard to maintain consistent formatting

## Requirements

### Unified Diagnostic Model
```csharp
public record Diagnostic
{
    public required DiagnosticId Id { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public required string Message { get; init; }
    public required SourceSpan PrimarySpan { get; init; }
    public ImmutableArray<SourceSpan> SecondarySpans { get; init; }
    public ImmutableArray<Label> Labels { get; init; }
    public ImmutableArray<string> Notes { get; init; }
    public DiagnosticData? Data { get; init; } // For quick fixes
}

public record SourceSpan(
    string FilePath,
    int StartLine,
    int StartCol,
    int EndLine,
    int EndCol
);

public record Label(SourceSpan Span, string Text);

public record DiagnosticId(string Code)
{
    public static DiagnosticId Error(int n) => new($"E{n:D4}");
    public static DiagnosticId Warning(int n) => new($"W{n:D4}");
}
```

### Diagnostic Registry
```csharp
public static class DiagnosticRegistry
{
    // All diagnostics defined in one place
    public static readonly DiagnosticTemplate UndefinedVariable = new(
        Id: DiagnosticId.Error(1001),
        Severity: DiagnosticSeverity.Error,
        MessageTemplate: "Undefined variable '{0}'",
        Category: "Resolution",
        HelpText: "Ensure the variable is declared before use..."
    );
    
    public static readonly DiagnosticTemplate TypeMismatch = new(
        Id: DiagnosticId.Error(1002),
        Severity: DiagnosticSeverity.Error,
        MessageTemplate: "Type mismatch: expected '{0}', found '{1}'",
        Category: "Type Checking"
    );
    
    // ... all other diagnostics catalogued here
}
```

### Source Location Tracking
```csharp
// Add to all AST nodes
public interface IAstNode
{
    SourceLocation Location { get; }
}

// Parser must track locations
public class AstBuilderVisitor : FifthParserBaseVisitor<AstThing>
{
    private SourceLocation GetLocation(ParserRuleContext ctx)
    {
        return new SourceLocation(
            _fileName,
            ctx.Start.Line,
            ctx.Start.Column,
            ctx.Stop.Line,
            ctx.Stop.Column
        );
    }
}
```

### Diagnostic Builder
```csharp
public class DiagnosticBuilder
{
    public static Diagnostic Build(
        DiagnosticTemplate template,
        SourceSpan primarySpan,
        params object[] args)
    {
        return new Diagnostic
        {
            Id = template.Id,
            Severity = template.Severity,
            Message = string.Format(template.MessageTemplate, args),
            PrimarySpan = primarySpan
        };
    }
    
    // Fluent API for complex diagnostics
    public DiagnosticBuilder WithSecondarySpan(SourceSpan span, string label);
    public DiagnosticBuilder WithNote(string note);
    public DiagnosticBuilder WithHelp(string help);
    public DiagnosticBuilder WithSuggestion(CodeAction action);
}
```

### Diagnostic Rendering
```csharp
public interface IDiagnosticRenderer
{
    string Render(Diagnostic diagnostic);
    string RenderWithSource(Diagnostic diagnostic, string sourceCode);
}

// Console renderer with colors
public class ConsoleRenderer : IDiagnosticRenderer
{
    public string Render(Diagnostic diagnostic)
    {
        // Rust-style error messages:
        // error[E1001]: Undefined variable 'foo'
        //   --> src/main.5th:10:5
        //    |
        // 10 |     print(foo);
        //    |           ^^^ undefined variable
        //    |
        //    = help: Did you mean 'for'?
    }
}

// LSP renderer
public class LSPRenderer : IDiagnosticRenderer
{
    public LSP.Diagnostic Render(Diagnostic diagnostic)
    {
        return new LSP.Diagnostic
        {
            Range = ToLSPRange(diagnostic.PrimarySpan),
            Severity = ToLSPSeverity(diagnostic.Severity),
            Code = diagnostic.Id.Code,
            Message = diagnostic.Message,
            RelatedInformation = diagnostic.SecondarySpans
                .Select(ToRelatedInfo).ToArray()
        };
    }
}
```

## Implementation Plan

### Phase 1: Design & Infrastructure (Weeks 1-2)
1. Design unified diagnostic model
2. Create DiagnosticRegistry class
3. Define all current error codes
4. Set up source location infrastructure

### Phase 2: Parser Integration (Weeks 3-4)
1. Add SourceLocation to AST nodes
2. Update parser to track locations
3. Update code generator to include locations
4. Test location tracking

### Phase 3: Core Migration (Weeks 5-6)
1. Migrate parser errors to new system
2. Migrate transformation phase errors
3. Update exception handling
4. Test error reporting

### Phase 4: Rendering & Tooling (Weeks 7-8)
1. Implement console renderer (Rust-style)
2. Implement LSP renderer
3. Add diagnostic rendering tests
4. Document error codes

## Acceptance Criteria

- [ ] All errors have diagnostic codes (E####, W####)
- [ ] All errors have source locations
- [ ] Console output shows beautiful error messages (like Rust)
- [ ] LSP integration shows errors inline in IDE
- [ ] Error code documentation generated
- [ ] Tests for all diagnostic scenarios
- [ ] Migration guide for adding new diagnostics

## Example Output

### Before (Current)
```
Parse error: Type mismatch
```

### After (Improved)
```
error[E1002]: Type mismatch: expected 'int', found 'string'
  --> src/main.5th:15:18
   |
15 |     let x: int = "hello";
   |                  ^^^^^^^ expected int, found string
   |
   = note: You can convert a string to an int using: int.parse("...")
```

## Error Code Categories

| Range | Category | Example |
|-------|----------|---------|
| E0001-E0999 | Parser/Syntax | E0001: Unexpected token |
| E1000-E1999 | Resolution | E1001: Undefined variable |
| E2000-E2999 | Type System | E2001: Type mismatch |
| E3000-E3999 | Code Generation | E3001: Invalid IL generation |
| W1000-W1999 | Warnings | W1001: Unused variable |

## References

- Architectural Review: `docs/architectural-review-2025.md` - Finding #4
- Rust diagnostics: https://rustc-dev-guide.rust-lang.org/diagnostics.html
- TypeScript diagnostics: https://github.com/microsoft/TypeScript/wiki/Using-the-Compiler-API
- Related Issues: Enables #ISSUE-002 (LSP), improves #ISSUE-001 (Error Recovery)

## Estimated Effort

**8 weeks** (2 months)
- Weeks 1-2: Design and infrastructure
- Weeks 3-4: Parser integration
- Weeks 5-6: Core migration
- Weeks 7-8: Rendering and tooling

## Dependencies

- Issue #001: Error Recovery (for proper error collection)
- Nice to have: #ISSUE-002 LSP (for IDE integration)

## Success Metrics

- 100% of errors have diagnostic codes
- 100% of errors have source locations
- Error messages comparable to Rust/TypeScript quality
- Positive developer feedback on error clarity
- Error documentation auto-generated
