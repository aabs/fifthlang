# Fifth Language Development Agent Instructions

This file provides operational guidance for GitHub Copilot and automated agents working on the Fifth language codebase. 

**Primary Reference**: All architectural decisions, principles, and comprehensive documentation are in `/specs/.specify/memory/constitution.md`. Always consult the constitution first for authoritative guidance on project structure, build processes, and development philosophy.

This file contains focused operational commands and agent-specific workflow instructions that complement the constitution.

## Quick Start Commands

### Prerequisites Verification
```bash
# Verify prerequisites (as detailed in constitution)
dotnet --version  # Should show 8.0.x (global.json uses 8.0.118)
java -version     # Should show Java 17+ for ANTLR
```

### Essential Build Commands
```bash
# Initial setup and build (run these commands in sequence)
dotnet restore fifthlang.sln                      # Takes ~70 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
dotnet build fifthlang.sln                        # Takes ~60 seconds. NEVER CANCEL. Set timeout to 120+ seconds.

# Alternative: Use Makefile
make build-all                                     # Takes ~25 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

# Run tests
dotnet test test/ast-tests/ast_tests.csproj        # Takes ~25 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
# Or run all tests across the solution
dotnet test fifthlang.sln

# Run AST code generator separately
make run-generator                                 # Takes ~5 seconds.
# OR
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```

## Project Structure Reference

See the constitution (`/specs/.specify/memory/constitution.md`) for the complete project structure diagram and component descriptions. Key operational points:

- `src/ast-model/` - Edit `AstMetamodel.cs` or `ILMetamodel.cs` to modify AST definitions
- `src/ast-generated/` - **NEVER edit manually**; regenerate via `make run-generator`
- `src/parser/grammar/` - `FifthLexer.g4` + `FifthParser.g4` (split grammar)
- `src/compiler/LanguageTransformations/` - AST transformation passes
- `test/` - TUnit tests with FluentAssertions

## Agent Workflow Guidelines

### Critical Build Rules
- **NEVER CANCEL** any build operations - they can take up to 2 minutes
- ANTLR grammar compilation happens automatically during parser project build
- AST code generation runs automatically before compilation via MSBuild targets
- **NEVER edit files in `src/ast-generated/` manually**

### Validation Protocol
After making changes, always run in this order:

1. **Build validation:**
   ```bash
   dotnet build fifthlang.sln  # NEVER CANCEL - wait up to 2 minutes
   ```

2. **Test validation:**
   ```bash
   dotnet test test/ast-tests/ast_tests.csproj  # NEVER CANCEL - wait up to 1 minute
   ```

3. **AST smoke test:**
   ```csharp
   using ast;
   using ast_generated;
   
   var intLiteral = new Int32LiteralExp { Value = 42 };
   var builder = new Int32LiteralExpBuilder();
   var result = builder.Build();
   // Should complete without errors
   ```

## Common Development Tasks

### AST Code Generation
```bash
# Regenerate AST builders and visitors after metamodel changes
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
# OR
make run-generator
```

### Grammar Development
- Edit `src/parser/grammar/FifthLexer.g4` (tokens, keywords, literals)
- Edit `src/parser/grammar/FifthParser.g4` (syntactic rules)
- Update `src/parser/AstBuilderVisitor.cs` for new syntax constructs
- ANTLR compilation happens automatically during build

### Language Transformations
- Add/modify visitors in `src/compiler/LanguageTransformations/`
- Update transformation pipeline in `src/compiler/ParserManager.cs`
- See constitution for complete transformation phase descriptions

## Expected Build Warnings (Safe to Ignore)
- ANTLR: "rule expression contains an assoc terminal option in an unrecognized location"
- Various C# nullable reference warnings throughout the codebase
- Switch expression exhaustiveness warnings in parser

## Agent-Specific Notes

### File Editing Rules
- **NEVER** hand-edit files in `src/ast-generated/`
- To modify AST: edit `src/ast-model/AstMetamodel.cs` or `src/ast-model/ILMetamodel.cs`, then regenerate
- Grammar changes: update both `FifthLexer.g4` AND `FifthParser.g4` as needed
- Always update corresponding `AstBuilderVisitor.cs` for grammar changes

### Testing Philosophy
- Practice TDD: write/approve tests, see them fail, then implement
- Use TUnit + FluentAssertions
- Never mask failing tests with broad try/catch blocks
- Let failures surface to properly reflect codebase state

### Multi-Pass Compilation Context
When adding language features, prefer AST transformations over code generation complexity:
1. Implement syntactic sugar in main AST model (`AstMetamodel.cs`)
2. Use transformation visitors to lower to simpler forms
3. Keep IL AST model (`ILMetamodel.cs`) simple and close to IL instructions
4. Reserve IL-level transformations for optimizations

See constitution for complete transformation pipeline details.
