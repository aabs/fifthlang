# AST Builder for Fifth Language

AST Builder is a C# .NET 8.0 solution that provides Abstract Syntax Tree (AST) construction capabilities for the Fifth programming language. It includes an ANTLR-based parser, code generation for AST builders and visitors, and a compiler with various language transformations.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap, Build, and Test
```bash
# Prerequisites: .NET 8.0 SDK and Java 17+ are required and available
# Verify prerequisites
dotnet --version  # Should show 8.0.x (global.json uses 8.0.118)
java -version     # Should show Java 17+ for ANTLR

# Initial setup and build (run these commands in sequence)
dotnet restore fifthlang.sln                      # Takes ~70 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
dotnet build fifthlang.sln                        # Takes ~60 seconds. NEVER CANCEL. Set timeout to 120+ seconds.

# Optional: Use just
just build-all                                     # Takes ~25 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

# Run tests (default: full suite for regressions)
dotnet test fifthlang.sln                           # Default for regression checks. NEVER CANCEL. Set timeout to 5+ minutes.
# Optional fast smoke (subset)
dotnet test test/ast-tests/ast_tests.csproj        # Quick subset when iterating locally.

# Run AST code generator separately
# Prefer just for quick tasks
just run-generator                                 # Takes ~5 seconds.
# OR
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```

**CRITICAL BUILD NOTES:**
- **NEVER CANCEL** any build operations - they can take up to 2 minutes
- ANTLR grammar compilation happens automatically during parser project build
- AST code generation runs automatically before compilation via MSBuild targets

### Project Structure
```
src/
├── ast-model/          # Core AST model definitions and type system
├── ast-generated/      # Auto-generated AST builders and visitors  
├── ast_generator/      # Code generator that creates AST infrastructure
├── code_generator/     # IL code generator and emission pipeline
├── parser/             # ANTLR-based parser with split grammar
├── compiler/           # Main compiler with language transformations
└── fifthlang.system/   # Built-in system functions

test/
└── ast-tests/          # TUnit tests with .5th code samples
└── runtime-integration-tests/          # TUnit tests for end-to-end verification
```

## Validation

Never under any circumstances mask a failing test with a catch-all try-catch block. It is better to transparently allow tests to fail to properly reflect the state of the code base.

### Always Validate Changes
After making any changes to the codebase:

1. **Build validation:**
   ```bash
      dotnet build fifthlang.sln  # NEVER CANCEL - wait up to 2 minutes
   ```

2. **Test validation (full suite for regressions):**
   ```bash
   dotnet test fifthlang.sln  # Default regression gate – runs all tests
   ```

   Optional fast smoke while iterating locally:
   ```bash
   dotnet test test/ast-tests/ast_tests.csproj  # Quick subset; follow with full suite before commit/PR
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
# - rewriter.generated.cs          (Rewriter pattern for lowering - NEW)
# - il.builders.generated.cs       (IL-specific builders)
# - il.rewriter.generated.cs       (IL rewriter pattern - NEW)
# - typeinference.generated.cs     (Type inference support)
```

### Choosing the Right Visitor/Rewriter Pattern

When implementing AST transformations, choose the appropriate pattern based on your needs:

**Use `BaseAstVisitor`** (read-only) when:
- ✅ Analyzing the AST without modifications (e.g., symbol table building, diagnostics)
- ✅ Collecting information or metrics
- ✅ Validation passes

**Use `DefaultRecursiveDescentVisitor`** when:
- ✅ Modifying AST with type-preserving rewrites (BinaryExp → BinaryExp)
- ✅ Simple transformations that don't change node types
- ✅ No statement hoisting needed

**Use `DefaultAstRewriter`** (⭐ PREFERRED for new lowering passes) when:
- ✅ **Statement-level lowering** - introducing temporary variables, pre-computation
- ✅ **Cross-type rewrites** - transforming node types (BinaryExp → FuncCallExp)
- ✅ **Desugaring operations** - breaking down high-level constructs
- ✅ **Expression hoisting** - pulling sub-expressions into statements
- ✅ Any transformation requiring statement insertion before the current statement

**Example: Using DefaultAstRewriter for Statement Hoisting**
```csharp
public class MyLoweringRewriter : DefaultAstRewriter
{
    public override RewriteResult VisitBinaryExp(BinaryExp ctx)
    {
        var lhs = Rewrite(ctx.LHS);
        var rhs = Rewrite(ctx.RHS);
        var prologue = new List<Statement>();
        prologue.AddRange(lhs.Prologue);
        prologue.AddRange(rhs.Prologue);
        
        // Hoist to temp variable
        var tmpDecl = new VarDeclStatement { /* ... */ };
        prologue.Add(tmpDecl);
        
        // Return reference to temp (cross-type rewrite!)
        return new RewriteResult(new VarRefExp { /* ... */ }, prologue);
    }
}
```

**See also**: `src/ast_generator/README.md` for detailed visitor/rewriter pattern guide.


### Parser Development
When working with grammar files:
```bash
# Grammar files location: src/parser/grammar/FifthLexer.g4 and FifthParser.g4
# ANTLR compilation happens automatically during build
# Manual ANTLR generation (if needed):
cd src/parser/grammar
java -jar ../tools/antlr-4.8-complete.jar -Dlanguage=CSharp -visitor -listener -o grammar -lib . FifthParser.g4
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

Knowledge Graphs quickstart:
```fifth
// Canonical store declaration (default store)
store default = sparql_store(<http://example.org/store>);

main(): int {
   // Statement-form graph block saves to the default store
   <{
      <http://example.org/s> <http://example.org/p> 42;
   }>;
   return 0;
}
```

## Dependencies and Requirements

### System Requirements
- **.NET 8.0 SDK** (global.json pins 8.0.118)
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
- **Test (AST subset)**: ~25 seconds (set timeout to 60+ seconds)
- **Test (full suite)**: varies by machine; set timeout to 5+ minutes in CI
- **Code Generation**: ~5 seconds (set timeout to 30+ seconds)

## Troubleshooting

### Common Issues
1. **"java command not found"** - Install Java 17+ 
2. **ANTLR grammar errors** - Check Fifth.g4 syntax in `src/parser/grammar/`
3. **Missing generated files** - Run `just run-generator` to regenerate AST code
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

## Active Technologies
- C# (compiler implementation), Fifth language surface; .NET SDK 8.0.x (global.json pins 8.0.118) + Antlr4.Runtime.Standard, RazorLight, System.CommandLine, TUnit, FluentAssertions, dunet, Vogen; Roslyn (for IL or backend equivalence tests) (005-implementation-of-try)
- C# 14 (per constitution) + ANTLR 4.8 runtime; internal AST generator; TUnit + FluentAssertions (001-gab-removal)
- C# 14, .NET SDK 8.0.118 (per global.json) + dotNetRDF (`VDS.RDF.*`), Fifth.System library (001-system-kg-types)
- N/A (in-memory objects and SPARQL stores via library) (001-system-kg-types)

## Recent Changes
- 005-implementation-of-try: Added C# (compiler implementation), Fifth language surface; .NET SDK 8.0.x (global.json pins 8.0.118) + Antlr4.Runtime.Standard, RazorLight, System.CommandLine, TUnit, FluentAssertions, dunet, Vogen; Roslyn (for IL or backend equivalence tests)
