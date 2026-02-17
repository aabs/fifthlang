# Runtime Integration Tests

This test project provides comprehensive runtime integration testing for the Fifth programming language compiler. The tests verify that Fifth language programs can be compiled to executable files and run correctly.

## Purpose

These tests validate the end-to-end compilation and execution pipeline:
1. **Compilation**: Fifth source code → IL metamodel → PE executable
2. **Execution**: PE executable runs as a separate process
3. **Validation**: Exit codes and output match expected behavior

## Current Status

⚠️ **Important Note**: The current PE emission implementation generates a hardcoded "Hello from Fifth!" program regardless of the input Fifth language code. Therefore, these tests are structured to:

1. **Verify Compilation Success**: Ensure Fifth language features can be parsed and compiled without errors
2. **Document Expected Behavior**: Include TODO comments describing what the exit codes and output should be when PE emission is fully implemented
3. **Provide Test Infrastructure**: Establish a comprehensive test framework ready for when PE emission processes actual Fifth IL

## Test Categories

### BasicRuntimeTests
- Arithmetic operations (+, -, *, /, %)
- Variable declarations and assignments
- Boolean expressions
- Nested expressions

### ControlFlowRuntimeTests  
- If/else statements
- While loops
- Complex boolean conditions
- Nested control structures

### FunctionRuntimeTests
- Function definitions and calls
- Recursive functions
- Parameter overloading with constraints
- Local variable scoping

### ClassRuntimeTests
- Class definitions and instantiation
- Property access and modification
- Object-oriented programming features
- Class-based method calls

### CollectionRuntimeTests
- Array declarations and operations
- List manipulations (if implemented)
- Array indexing and iteration
- Nested collections

### DestructuringRuntimeTests
- Simple destructuring patterns
- Conditional destructuring with guards
- Nested destructuring
- Array destructuring

### BuiltInRuntimeTests
- Standard library functions
- String operations and concatenation
- I/O operations
- Math functions
- Error handling (try/catch)

## Test Structure

Each test follows this pattern:

```csharp
[Test]
public async Task FeatureName_ShouldExpectedBehavior()
{
    // Arrange
    var sourceCode = """
        main(): int {
            // Fifth language code
            return 42;
        }
        """;

    // Act
    var executablePath = await CompileSourceAsync(sourceCode);
    
    // Assert
    File.Exists(executablePath).Should().BeTrue("Executable should be generated");
    
    // TODO: When PE emission is fixed, expect exit code 42
}
```

## Infrastructure Classes

### RuntimeTestBase
- Base class providing common compilation and execution functionality
- Automatic cleanup of generated files and directories
- Cross-platform executable running (handles Windows .exe vs Linux dotnet execution)
- Runtime configuration file generation for .NET executables

### ExecutionResult
- Captures process exit code, stdout, stderr, and execution time
- Used for validating program behavior when PE emission is working

## Test Programs

The `TestPrograms/` directory contains Fifth language source files organized by feature:
- `Basic/` - Simple arithmetic and expressions
- `ControlFlow/` - If statements, loops
- `Functions/` - Function definitions, recursion, overloading
- `Classes/` - Object-oriented programming
- `Collections/` - Arrays and lists
- `BuiltIns/` - Standard library usage

## Running Tests

```bash
# Run all runtime integration tests
dotnet test test/runtime-integration-tests/

# Run specific test category
dotnet test test/runtime-integration-tests/ --filter "BasicRuntimeTests"

# Run specific test
dotnet test test/runtime-integration-tests/ --filter "SimpleReturnInt_ShouldGenerateExecutable"
```

## Future Updates

When PE emission is updated to process actual Fifth language IL:

1. **Update Assertions**: Change tests to validate actual exit codes and output
2. **Remove Workarounds**: Remove try/catch blocks that skip unimplemented features  
3. **Add Execution Validation**: Include tests that run executables and verify their behavior
4. **Expand Coverage**: Add more comprehensive test scenarios

## Cross-Platform Compatibility

Tests automatically handle platform differences:
- **Windows**: Runs `.exe` files directly
- **Linux/macOS**: Uses `dotnet` to run executables
- **Runtime Config**: Generates `.runtimeconfig.json` files for .NET runtime compatibility

## Contributing

When adding new tests:
1. Follow existing naming conventions
2. Include TODO comments for expected behavior
3. Use try/catch for features that may not be implemented yet
4. Ensure proper cleanup of generated files
5. Add both inline source code tests and external `.5th` file tests

### VS Code Dev Kit Tests
- See `docs/vscode-devkit-tests.md` for enabling test discovery/runs in the Dev Kit Testing UI.