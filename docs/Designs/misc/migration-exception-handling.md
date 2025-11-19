# Migration Notes - Exception Handling Support

## Version: Next Release

### New Reserved Keywords

The following keywords are now reserved and cannot be used as identifiers:

- `try`
- `catch`
- `finally`
- `throw`
- `when` (used in exception filters)

**Impact**: If your code uses any of these as variable names, function names, or type names, you will need to rename them.

**Example**:
```fifth
// Before (will now cause parse errors):
try: int = 5;
catch: string = "value";

// After (renamed identifiers):
tryCount: int = 5;
catchValue: string = "value";
```

### New Features

#### Exception Handling

Fifth now supports C#-style exception handling with try/catch/finally blocks:

```fifth
main(): int {
    result: int = 0;
    
    try {
        result = 10;
    } catch {
        result = 1;  // Catch-all handler
    } finally {
        std.print("cleanup");  // Always executes
    }
    
    return result;
}
```

#### Features Supported:

1. **Try/Finally** - Ensure cleanup code runs
2. **Try/Catch** - Handle exceptions with catch-all blocks
3. **Try/Catch/Finally** - Combined exception handling and cleanup
4. **Throw Expressions** - Use throw in expression contexts (future enhancement)

#### Current Limitations:

- Typed exception catches (e.g., `catch (ex: System.Exception)`) require parser support for qualified type names
- Exception throwing requires instantiation syntax enhancements
- Stack trace preservation for rethrow is supported but requires full exception infrastructure

### Semantic Validation

The compiler now validates:
- **TRY001**: Catch types must derive from System.Exception
- **TRY002**: Filter expressions must be boolean-convertible
- **TRY003**: Unreachable catch clauses are errors
- **TRY004**: Throw operands must be exception types

### Backward Compatibility

- Code not using the new reserved keywords will compile without changes
- No changes to existing control flow semantics
- All existing tests continue to pass
