# AST Builder for Fifth Language

AST Builder is a C# .NET 9.0 solution that provides Abstract Syntax Tree (AST) construction capabilities for the Fifth programming language. It includes an ANTLR-based parser, code generation for AST builders and visitors, and a compiler with various language transformations.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap, Build, and Test
```bash
# Prerequisites: .NET 9.0.303 SDK and Java 17+ are required and available
# Verify prerequisites
dotnet --version  # Should show 9.0.303 or compatible
java -version     # Should show Java 17+ for ANTLR

# Initial setup and build (run these commands in sequence)
dotnet restore ast-builder.sln                    # Takes ~70 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
dotnet build ast-builder.sln                      # Takes ~60 seconds. NEVER CANCEL. Set timeout to 120+ seconds.

# Alternative: Use Makefile
make build-all                                     # Takes ~25 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

# Run tests
dotnet test test/ast-tests/ast_tests.csproj        # Takes ~25 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

# Run AST code generator separately
make run-generator                                 # Takes ~5 seconds.
# OR
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```

**CRITICAL BUILD NOTES:**
- **NEVER CANCEL** any build operations - they can take up to 2 minutes
- ANTLR grammar compilation happens automatically during parser project build
- AST code generation runs automatically before compilation via MSBuild targets
- One test may fail (`can_parse_double_literals`) - this is a pre-existing issue, ignore it

### Project Structure
```
src/
├── ast-model/          # Core AST model definitions and type system
├── ast-generated/      # Auto-generated AST builders and visitors  
├── ast_generator/      # Code generator that creates AST infrastructure
├── parser/             # ANTLR-based parser with Fifth.g4 grammar
├── compiler/           # Main compiler with language transformations
└── fifthlang.system/   # Built-in system functions

test/
└── ast-tests/          # XUnit tests with .5th code samples

tools/
└── CodeGenerator.cs    # Additional code generation utilities
```

## Validation

Never under any circumstances mask a failing test with a catch-all try-catch block. It is better to transparently allow tests to fail to properly reflect the state of the code base.

### Always Validate Changes
After making any changes to the codebase:

1. **Build validation:**
   ```bash
   dotnet build ast-builder.sln  # NEVER CANCEL - wait up to 2 minutes
   ```

2. **Test validation:**
   ```bash
   dotnet test test/ast-tests/ast_tests.csproj  # NEVER CANCEL - wait up to 1 minute
   ```

3. **Manual AST functionality test:**
   Create a simple test to verify AST builders work:
   ```csharp
   using ast;
   using ast_generated;
   
   var intLiteral = new Int32LiteralExp { Value = 42 };
   var builder = new Int32LiteralExpBuilder();
   var result = builder.Build();
   // Should complete without errors
   ```

### Expected Build Warnings
The following warnings are normal and can be ignored:
- ANTLR warning: "rule expression contains an assoc terminal option in an unrecognized location"
- Various C# nullable reference warnings throughout the codebase
- Switch expression exhaustiveness warnings in parser

## Common Tasks

### Code Generation
The AST generator is central to this project:
```bash
# Regenerate AST builders and visitors
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

# The generator reads from src/ast-model/AstMetamodel.cs and generates:
# - builders.generated.cs          (Builder pattern classes)
# - visitors.generated.cs          (Visitor pattern classes)  
# - il.builders.generated.cs       (IL-specific builders)
# - typeinference.generated.cs     (Type inference support)
```

### Parser Development
When working with grammar files:
```bash
# Grammar files location: src/parser/grammar/Fifth.g4
# ANTLR compilation happens automatically during build
# Manual ANTLR generation (if needed):
cd src/parser/grammar
java -jar ../tools/antlr-4.8-complete.jar -Dlanguage=CSharp -visitor -listener -o grammar -lib . Fifth.g4
```

### Working with Fifth Language Files
Sample Fifth language files are in:
- `test/ast-tests/CodeSamples/*.5th` - Test code samples
- `src/parser/grammar/test_samples/*.5th` - Parser test samples

Example Fifth syntax:
```fifth
class Person {
    Name: string;
    Height: float;
}

main() => myprint(5 + 6);
myprint(int x) => std.print(x);
```

## Dependencies and Requirements

### System Requirements
- **.NET 9.0 SDK** (version 9.0.303 or compatible)
- **Java 17+** (for ANTLR grammar compilation)
- **ANTLR 4.8** (jar file included at `src/parser/tools/antlr-4.8-complete.jar`)

### Key NuGet Packages
- `Antlr4.Runtime.Standard` - ANTLR runtime for C#
- `RazorLight` - Template engine for code generation
- `System.CommandLine` - CLI parsing for AST generator
- `xunit` - Test framework
- `FluentAssertions` - Test assertions
- `dunet` - Discriminated unions for AST model
- `Vogen` - Value object generation

### Build Timing Guidelines
- **Restore**: ~70 seconds (set timeout to 120+ seconds)
- **Build**: ~60 seconds (set timeout to 120+ seconds) 
- **Test**: ~25 seconds (set timeout to 60+ seconds)
- **Code Generation**: ~5 seconds (set timeout to 30+ seconds)

## Troubleshooting

### Common Issues
1. **"java command not found"** - Install Java 17+ 
2. **ANTLR grammar errors** - Check Fifth.g4 syntax in `src/parser/grammar/`
3. **Missing generated files** - Run `make run-generator` to regenerate AST code
4. **Build timeouts** - Use longer timeout values, builds can legitimately take 1-2 minutes

### Key Files to Watch
- Always check generated files in `src/ast-generated/` after modifying `src/ast-model/AstMetamodel.cs`
- Parser changes require attention to both `src/parser/grammar/Fifth.g4` and `src/parser/AstBuilderVisitor.cs`
- Test failures in grammar parsing usually indicate issues in ANTLR grammar or visitor implementation

### Build Order Dependencies
1. `ast-model` (base AST definitions)
2. `ast_generator` (creates builders/visitors) 
3. `ast-generated` (output of generator, depends on ast-model)
4. `parser` (depends on ast-model, ast-generated, runs ANTLR)
5. `compiler` (depends on all above)
6. `ast-tests` (depends on all above)

Always build the full solution rather than individual projects to ensure proper dependency resolution.
