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

### Documentation & Example Validation

Before running parser or runtime tests, agents MUST ensure that all code samples and test programs in `docs/`, `specs/`, and `test/` use grammar-supported Fifth syntax. This prevents parser-time regressions caused by accidental non-Fifth idioms in documentation or ad-hoc probes.

Checklist for agents (must run every time examples/docs are modified):

1. Sweep for obviously non-Fifth declarations and shorthand forms:
   - Look for `var <name> =` in examples (C#/JS-style). These must be converted to `name: type =` or the appropriate Fifth form.
   - Look for type-first declarations like `graph g =` or `triple t =` in docs and examples. These are invalid in Fifth and must be rewritten as `g: graph =` or `t: triple =` respectively.

   Quick grep examples (run from repo root; fish shell):

   ```fish
   # Find 'var' in .5th and docs
   grep -R --line-number --exclude-dir=.git --include='*.5th' --include='*.md' "\bvar \" . || true

   # Find 'graph <ident> =' patterns in markdown or examples
   grep -R --line-number --exclude-dir=.git --include='*.md' --include='*.5th' "graph [A-Za-z_]\\+\s*=" || true
   ```

   If any hits are found, update the snippet to the correct Fifth syntax. If the snippet is intentionally invalid (used by a negative test), add an explicit negative-test marker comment in the file so the `validate-examples` tool will skip it (see the validator's heuristics).

2. Run the project's example validator and parser-check tools

   ```fish
   # Validate all examples that should parse. This quick-check uses the project's tooling
   scripts/validate-examples.fish

   # If you need to force-include intentionally-invalid examples for debugging
   scripts/validate-examples.fish --include-negatives
   ```

   Fix any parsing errors reported by the validator. If a snippet is intended to be invalid for a test, ensure the validator-skip markers are present.

3. Re-run parser tests and relevant unit/integration tests

   ```fish
   # Parser-focused tests
   dotnet test test/syntax-parser-tests/ -v minimal

   # Then runtime-integration tests for the subset you plan to change
   dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj --filter "FullyQualifiedName~YourTestName" -v minimal
   ```

Notes for agents
- Always prefer updating documentation snippets to the current grammar rather than changing the validator or tests to accept legacy forms.
- If you intentionally change the language surface syntax, update the grammar files under `src/parser/grammar/`, `AstBuilderVisitor`, and add new parser tests explaining the rationale. Also update the constitution/specs to record the change.

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

### Grammar Compliance Checklist for Agents

When adding or updating example code, test programs, or documentation snippets that are intended to be parsed by the compiler or used as integration tests, follow this checklist:

1. Validate parsing locally:
   - Build the solution and run the parser/syntax tests (e.g., `dotnet test test/syntax-parser-tests/`) or a targeted parser-check. Ensure the sample parses without errors and the `AstBuilderVisitor` can build the high-level AST.
2. Use grammar-supported forms only:
   - Do NOT use legacy shorthand forms (for example, the older guard shorthand using `when`) in samples that will be parsed by the compiler. Use the parameter-constraint form `param: Type | <expr>` and block function bodies where the parser requires them.
3. Ensure test integration:
   - If a sample is referenced by integration tests, add `CopyToOutputDirectory` metadata in the test project's `.csproj` so the sample is available at test runtime (see `test/runtime-integration-tests/runtime-integration-tests.csproj`).
4. Run integration-checks before committing:
   - Run the relevant integration tests that consume the sample (e.g., `dotnet test test/runtime-integration-tests/ --filter GuardValidation`) to ensure the sample behaves as expected end-to-end.
5. Update spec/constitution if intentionally introducing new surface syntax:
   - If you believe a new surface syntax is required (instead of fixing the sample), update `src/parser/grammar/*` and `src/parser/AstBuilderVisitor.cs`, add parser tests, update the constitution's grammar rules, and include a rationale in the PR.

Following this checklist prevents parser-time flakiness and keeps integration tests deterministic.

## Knowledge Graphs (Agent Notes)
- Canonical store declarations only: `name : store = sparql_store(<iri>);` or `store default = sparql_store(<iri>);`
- Graph assertion blocks `<{ ... }>`:
   - Statement-form saves to default store.
   - Expression-form yields an `IGraph`.
- Validate quickly:
   - `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj`
   - `dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj -v minimal --filter FullyQualifiedName~GraphAssertionBlock_`
- Reference: `docs/knowledge-graphs.md`

CI notes:

- This repository includes a CI step `Validate .5th samples (parser-check)` that runs the `src/tools/validate-examples` tool to ensure all `.5th` examples across `docs/`, `specs/`, `src/parser/grammar/test_samples/`, and `test/` parse with the current grammar. Agents should run `scripts/validate-examples.fish` locally before committing to catch parser-time regressions early.

- The `validate-examples` tool now skips intentionally-invalid (negative) tests when validating samples. It uses directory- and content-based heuristics to exclude files under `*/Invalid/*`, files with `invalid` in the filename, or files that include an explicit negative-test comment marker. To force validation of negative tests (for debugging), run the tool with `--include-negatives`.
