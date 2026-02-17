# Fifth Language Engine

### I. Library-First, Contracts-First
Every feature starts as a focused library under `src/` with a clear, documented purpose and public contract. Libraries must be self-contained, independently buildable, and testable with xUnit. Avoid organizational or “glue-only” libraries. Contracts are expressed through:
- AST metamodels in `src/ast-model/AstMetamodel.cs`
- IL metamodels in `src/ast-model/ILMetamodel.cs`
- Generated builders/visitors in `src/ast-generated/`
- ANTLR grammar split between `src/parser/grammar/FifthParser.g4` and `src/parser/grammar/FifthLexer.g4`
- Public CLIs (e.g., `ast_generator`, `compiler`) with text I/O Constitution

This constitution governs how we build, test, version, and evolve the Fifth language projects in this repository, including the AST model, code generator, parser, compiler, and system libraries. It exists to create a predictable, testable, and observable development flow that GitHub SpecKit and automation can rely on.

This document is self-contained and has no external dependencies. All operational details, build commands, project structure, and workflows are defined herein.

## Project Overview

Fifth Language is a C# .NET 8.0 solution that provides Abstract Syntax Tree (AST) construction capabilities for the Fifth programming language. It includes an ANTLR-based split lexer/parser, code generation for AST builders and visitors, and a multi-pass compiler with various language transformations that perform AST lowering through intermediate representations.

### Architecture Overview

The Fifth language compiler follows a multi-pass pipeline architecture that transforms source code through several intermediate representations:

1. **Lexical Analysis** → **Parsing** → **Parse Tree**
2. **Parse Tree** → **AST Building** → **High-Level AST**
3. **High-Level AST** → **Language Transformations** → **Lowered AST**
4. **Lowered AST** → **IL Transformation** → **IL-Level AST**
5. **IL-Level AST** → **Code Generation** → **PE Assembly**

## Core Principles

### I. Library-First, Contracts-First
Every feature starts as a focused library under `src/` with a clear, documented purpose and public contract. Libraries must be self-contained, independently buildable, and testable with xUnit. Avoid organizational or “glue-only” libraries. Contracts are expressed through:
- AST metamodels in `src/ast-model/`
- Generated builders/visitors in `src/ast-generated/`
- ANTLR grammar in `src/parser/grammar/FifthParser.g4` and `src/parser/grammar/FifthLexer.g4` 
- Public CLIs (e.g., `ast_generator`, `compiler`) with text I/O

### II. CLI and Text I/O Discipline
Each executable surface must provide a CLI entry that communicates via text I/O:
- stdin/args → input; stdout → primary output; stderr → errors/diagnostics
- Support human-readable text; add JSON output where suitable for automation
- Favor deterministic, scriptable commands to enable SpecKit orchestration

### III. Generator-as-Source-of-Truth (Do Not Hand-Edit Generated Code)
The AST generator is authoritative for builders, visitors, IL builders, and type inference support. Never hand-edit files under `src/ast-generated/`. To change generated outputs:
1. Update `src/ast-model/AstMetamodel.cs` for main AST types or `src/ast-model/ILMetamodel.cs` for IL AST types
2. Optionally update templates under `src/ast_generator/Templates/` for code generation changes
3. Regenerate via `just run-generator` (preferred) or `dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated`
4. Build the full solution to validate

### IV. Test-First (Non-Negotiable)
Practice TDD: write/approve tests, see them fail, then implement. Use xUnit + FluentAssertions across:
- `test/ast-tests/` for AST and generator correctness
- `test/syntax-parser-tests/` for grammar parsing
- `test/runtime-integration-tests/` for end-to-end verification
Never mask failing tests with broad try/catch. Let failures surface so CI and SpecKit correctly reflect state.

**CRITICAL: Compilation is NOT Sufficient**
A language feature is NOT complete until it is proven to work end-to-end with comprehensive passing tests that:
1. Use actual Fifth language syntax (not just C# interop calls)
2. Execute successfully at runtime (not just compile)
3. Validate that results are accessible and correct
4. Exercise all major code paths and result types

For features involving TriG literals (`<{...}>`), SPARQL literals (`?<...>`), or other language constructs:
- Tests MUST use these constructs directly in Fifth code
- Tests MUST run to completion and return expected exit codes
- Tests MUST prove data flows through the entire pipeline
- Manual C# validation is acceptable as supplementary evidence but does NOT replace Fifth language tests

Any feature with only compilation tests or failing runtime tests is considered INCOMPLETE. Continue iterating until all tests pass.

### V. Reproducible Builds & Toolchain Discipline
Tooling is pinned and enforced for reproducibility:
- .NET SDK 8.0.x per `global.json` (currently 8.0.118)
- Java 17+ for ANTLR; ANTLR 4.8 runtime with the jar at `src/parser/tools/antlr-4.8-complete.jar`
- Build order: ast-model → ast_generator → ast-generated → parser → code_generator → compiler → tests
- Critical rule: NEVER CANCEL restore/build/test/generation tasks. Allow up to 1–2 minutes for completion as documented.

### VI. Simplicity, Minimal Surface, and Safety
Prefer the simplest design that works. Avoid incidental complexity and non-required abstractions (YAGNI). Make targeted, minimal changes that respect existing structure and APIs. Do not add catch-all error handling that hides defects. Changes that increase complexity must be justified in the PR.

### VII. Multi-Pass Compilation & AST Lowering Philosophy
The compiler implements a clean separation of concerns through multiple compilation phases:

#### Phase 1: Lexical Analysis & Parsing
- **Lexer** (`FifthLexer.g4`): Tokenizes source code into lexical tokens
- **Parser** (`FifthParser.g4`): Builds parse tree from tokens according to grammar rules
- **AST Builder Visitor** (`AstBuilderVisitor.cs`): Transforms parse tree into high-level AST

#### Phase 2: Language Analysis & Transformation
Multiple passes through the AST apply increasingly sophisticated transformations:

1. **TreeLinkageVisitor**: Establishes parent-child relationships in AST
2. **BuiltinInjectorVisitor**: Injects built-in function definitions
3. **ClassCtorInserter**: Adds default constructors where needed
4. **SymbolTableBuilderVisitor**: Builds symbol tables for scoping
5. **PropertyToFieldExpander**: Expands property syntax to underlying field access
6. **OverloadGatheringVisitor**: Groups function overloads together
7. **OverloadTransformingVisitor**: Transforms overloaded functions into guard/subclause pattern
8. **DestructuringVisitor**: Resolves property references in destructuring patterns
9. **DestructuringLoweringRewriter**: Lowers destructuring to variable declarations with constraint collection
10. **TypeAnnotationVisitor**: Performs type inference and annotation

#### Phase 3: IL Transformation
- **AstToIlTransformationVisitor**: Transforms high-level AST to IL-level AST
- IL-level AST uses simpler, lower-level constructs suitable for code generation

#### Phase 4: Code Generation
#### Phase 3: IL Transformation (legacy)
- **AstToIlTransformationVisitor**: Transforms high-level AST to IL-level AST
- IL-level AST uses simpler, lower-level constructs suitable for code generation

> Note (Roslyn migration): The project is transitioning from the legacy IL pipeline (AstToIlTransformationVisitor → ILEmissionVisitor → PEEmitter) to a Roslyn-based backend. During migration the legacy pipeline remains available behind a feature flag; the long-term goal is to remove IL-level lowering and PEEmitter code once equivalence is established and preservation candidates are handled.

#### Phase 4: Code Generation (Roslyn-backed)
- New component: `LoweredAstToRoslynTranslator` — accepts the Lowered AST and emits C# syntax trees for Roslyn compilation and PE/PDB emission.
- Roslyn-based emission replaces direct IL emission for most constructs and produces Portable PDBs with #line mapping back to `.5th` sources.
- Roslyn-generated PDBs MUST include full line-and-column sequence points for all emitted statements and expressions to preserve developer debugging experience. Implementations should use Roslyn's SequencePoint APIs / #line pragmas and EmbeddedText to preserve source fidelity.
- Legacy emitters (ILEmissionVisitor/PEEmitter) will be deprecated and removed only after passing a documented canary and acceptance plan.

### VIII. AST Design & Transformation Strategy
When designing language features and solving problems:

**Prefer AST Transformations over Code Generation Complexity**
- Implement syntactic sugar and high-level constructs in the main AST model
- Use language transformation visitors to lower high-level constructs to simpler forms
- Keep the IL AST model simple and close to actual IL instructions
- Reserve IL-level transformations for optimizations and final code generation

**Two-Level AST Design**
- **Main AST** (`AstMetamodel.cs`): Rich, high-level constructs that mirror source language features
- **IL AST** (`ILMetamodel.cs`): Simple, low-level constructs that map directly to IL instructions

**Transformation Guidelines**
- Each transformation visitor should have a single, well-defined responsibility
- Transformations should be order-dependent; later passes can depend on earlier ones
- Prefer multiple simple passes over complex single passes
- Document dependencies between transformation passes
- Each pass should preserve AST validity and type safety

**Choosing the Right Transformation Pattern**

The AST generator provides three main patterns for AST traversal and transformation:

1. **`BaseAstVisitor`** (read-only analysis)
   - Use for: Symbol table building, diagnostics, validation, metrics collection
   - Characteristics: Enter/leave pattern, no return values, side effects only
   - Cannot modify AST

2. **`DefaultRecursiveDescentVisitor`** (type-preserving transformations)
   - Use for: Simple AST modifications, structure-preserving rewrites
   - Characteristics: Type-safe returns (BinaryExp → BinaryExp), recursive descent
   - Cannot change node types or hoist statements

3. **`DefaultAstRewriter`** ⭐ **PREFERRED for new lowering passes**
   - Use for: Statement-level desugaring, cross-type rewrites, expression hoisting
   - Characteristics: Returns `RewriteResult` with node + prologue, category-level flexibility
   - Enables: Temporary variables, pre-computation statements, cross-type transformations
   - Example: Transform `BinaryExp` → `FuncCallExp` with hoisted temp declaration

**When to Use DefaultAstRewriter**:
- Introducing temporary variables or pre-computation
- Breaking down high-level constructs into multiple statements
- Transforming expression types (e.g., operators to function calls)
- Any lowering that requires statement insertion before the current statement

See `src/ast_generator/README.md` for comprehensive visitor/rewriter pattern guide.

### IX. Parser & Grammar Integrity
The grammar is split into lexer and parser components:
- **FifthLexer.g4**: Defines tokens, keywords, literals, and lexical structure
- **FifthParser.g4**: Defines syntactic rules and grammar structure

Changes to either grammar file require:
- Corresponding tests in `src/parser/grammar/test_samples/*.5th`
- Updates to `AstBuilderVisitor.cs` for new syntax constructs
- Regeneration occurs automatically during build via ANTLR
- Expected warnings (like `assoc` option location) can be ignored if documented as benign

Additional rule: Grammar compliance for example/source files

- All example programs, quickstart snippets, test fixtures, and specification samples that are intended to be fed into the parser or compiler MUST conform to the current parser grammar. Samples that use legacy or non-grammar shorthand (for example, older guard shorthand such as `when`) must be converted to the canonical grammar-supported form before being added to the repository or referenced by integration tests.

- Canonical guard example (before → after):

      // before (legacy shorthand — NOT VALID for parser-based tests)
      myprint(int x) when x == 0 => std.print(x);

      // after (grammar-compliant parameter constraint)
      myprint(int x | x == 0) { std.print(x); }

- Definition: "Grammar-compliant" means the sample can be successfully parsed by the current `FifthParser.g4` and successfully transformed to the high-level AST by `AstBuilderVisitor` without syntax errors.

- Enforcement (implemented): CI includes a step "Validate .5th samples (parser-check)" that runs the repo's example validator to ensure all `.5th` examples across `docs/`, `specs/`, `src/parser/grammar/test_samples/`, and `test/` parse with the current grammar. Locally, run `scripts/validate-examples.fish` (fish shell) to catch issues before pushing.
   - Parser-focused tests: `dotnet test test/syntax-parser-tests/ -v minimal`
   - Integration tests for affected areas as needed: `dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj --filter "FullyQualifiedName~YourTestName" -v minimal`

- Rationale: Ensures integration tests exercise the real compiler pipeline and prevents parser-time flakiness caused by deprecated surface forms.

### X. Observability & Diagnostics
Text I/O is the primary observability mechanism. Emit clear, actionable diagnostics to stderr. Prefer structured messages (line/column, file paths) for parsers/compilers. When logging is needed, use standard .NET logging abstractions; avoid custom frameworks. Output must be deterministic and stable to support automated parsing by SpecKit.

### XI. Versioning & Backward Compatibility
Use Semantic Versioning (MAJOR.MINOR.PATCH). Breaking changes require:
- A migration note in the PR
- Updated tests reflecting new behavior
- A minor/major version bump as appropriate
Generated code changes follow metamodel versioning. Deprecations must be documented and tested.

## Project Structure

```
src/
├── ast-model/          # Core AST model definitions and type system
│   ├── AstMetamodel.cs     # Main high-level AST definitions
│   ├── ILMetamodel.cs      # IL-level AST definitions
│   └── TypeSystem/         # Type system components
├── ast-generated/      # Auto-generated AST builders and visitors  
│   ├── builders.generated.cs       # Builder pattern classes
│   ├── visitors.generated.cs       # Visitor pattern classes
│   ├── rewriter.generated.cs       # Rewriter pattern for lowering (NEW)
│   ├── il.builders.generated.cs    # IL-specific builders
│   ├── il.rewriter.generated.cs    # IL rewriter pattern (NEW)
│   └── typeinference.generated.cs  # Type inference support
├── ast_generator/      # Code generator that creates AST infrastructure
│   ├── Program.cs              # CLI entry point
│   ├── Templates/              # Razor templates for code generation
│   └── RazorLight*.cs          # Template processors
├── parser/             # ANTLR-based parser with split grammar
│   ├── grammar/
│   │   ├── FifthLexer.g4       # Lexical analysis grammar
│   │   ├── FifthParser.g4      # Syntactic analysis grammar
│   │   └── test_samples/*.5th  # Parser test samples
│   ├── tools/
│   │   └── antlr-4.8-complete.jar  # ANTLR tool
│   └── AstBuilderVisitor.cs    # Parse tree to AST transformation
├── compiler/           # Main compiler with language transformations
│   ├── Compiler.cs             # Main compilation orchestrator
│   ├── ParserManager.cs        # Parser and transformation coordinator
│   ├── CompilerOptions.cs      # CLI option parsing
│   └── LanguageTransformations/    # AST transformation passes
│       ├── TreeLinkageVisitor.cs
│       ├── OverloadTransformingVisitor.cs
│       ├── DestructuringVisitor.cs
│       └── ... (other transformation passes)
├── code_generator/     # IL code generator and emission pipeline
│   ├── ILCodeGenerator.cs      # Orchestrates IL generation
│   ├── AstToIlTransformationVisitor.cs  # AST → IL AST transformation
│   ├── ILEmissionVisitor.cs    # IL AST → CIL instructions
│   └── PEEmitter.cs            # PE assembly emission
└── fifthlang.system/   # Built-in system functions
    ├── BuiltinFunctions.cs
    └── KnowledgeGraphs.cs

test/
├── ast-tests/          # xUnit tests with .5th code samples
├── syntax-parser-tests/  # Grammar and parsing tests
└── runtime-integration-tests/  # End-to-end verification tests
```

## Build System & Commands

### Prerequisites
- **.NET 8.0 SDK** (global.json pins 8.0.118)
- **Java 17+** (for ANTLR grammar compilation)
- **ANTLR 4.8** (jar file included at `src/parser/tools/antlr-4.8-complete.jar`)

### Verification Commands
```bash
# Verify prerequisites
dotnet --version  # Should show 8.0.x
java -version     # Should show Java 17+
```

### Bootstrap, Build, and Test Commands
```bash
# Initial setup and build (run these commands in sequence)
dotnet restore fifthlang.sln                      # Takes ~70 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
dotnet build fifthlang.sln                        # Takes ~60 seconds. NEVER CANCEL. Set timeout to 120+ seconds.

# Optional: Use Just
just build-all # Takes ~25 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

# Run tests
dotnet test test/ast-tests/ast_tests.csproj        # Takes ~25 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
# Or run all tests across the solution
dotnet test fifthlang.sln

# Run AST code generator separately
just run-generator                                                         # Takes ~5 seconds.
# OR
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```

### Critical Build Notes
- **NEVER CANCEL** any build operations - they can take up to 2 minutes
- ANTLR grammar compilation happens automatically during parser project build
- AST code generation runs automatically before compilation via MSBuild targets

### Build Timing Guidelines
- **Restore**: ~70 seconds (set timeout to 120+ seconds)
- **Build**: ~60 seconds (set timeout to 120+ seconds) 
- **Test**: ~25 seconds (set timeout to 60+ seconds)
- **Code Generation**: ~5 seconds (set timeout to 30+ seconds)

### Build Order Dependencies
1. `ast-model` (base AST definitions)
2. `ast_generator` (creates builders/visitors) 
3. `ast-generated` (output of generator, depends on ast-model)
4. `parser` (depends on ast-model, ast-generated, runs ANTLR)
5. `code_generator` (legacy IL pipeline — will be phased out as Roslyn backend matures)
6. `compiler` (depends on all above; after migration, compiler will integrate `LoweredAstToRoslynTranslator` and may depend less on legacy `code_generator` internals)
7. `tests` (depends on all above)

Always build the full solution rather than individual projects to ensure proper dependency resolution.

## Dependencies and Requirements

### Key NuGet Packages
- `Antlr4.Runtime.Standard` - ANTLR runtime for C#
- `RazorLight` - Template engine for code generation
- `System.CommandLine` - CLI parsing for AST generator
- `xUnit` - Test framework
- `FluentAssertions` - Test assertions
- `dunet` - Discriminated unions for AST model
- `Vogen` - Value object generation

### Fifth Language Syntax Examples
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

### Knowledge Graphs (Canonical Syntax)
- Canonical store declaration forms:
   - `name: store = sparql_store(<http://example.org/store>);`
   - `store default = sparql_store(<http://example.org/store>);` (sets default)
- Literal support in object position includes: strings, booleans, chars, all signed/unsigned integers, float, double, and precise `decimal`.
- Graph operations are performed via supported APIs and expressions; there is no special surface syntax for inline graph assertion blocks.
- Tests: runtime coverage under `test/runtime-integration-tests` and smoke tests under `test/kg-smoke-tests` validate end-to-end behavior of RDF features (e.g., triple literals, store interactions).

## Engineering Constraints & Standards

### Toolchain & Environment
- C# 12 Language Version (or the latest supported by .NET 8 SDK).
- .NET 8.0 SDK required; Java 17+ required for ANTLR operations
- For build timing, set timeouts per documented guidance; do not prematurely interrupt tasks

### Code Generation
- All generated code resides in `src/ast-generated/`
- The generator reads from `src/ast-model/AstMetamodel.cs` and `src/ast-model/ILMetamodel.cs`
- Generated files include:
  - `builders.generated.cs` (Builder pattern classes)
  - `visitors.generated.cs` (Visitor pattern classes)
  - `rewriter.generated.cs` (Rewriter pattern for lowering - NEW)
  - `il.builders.generated.cs` (IL-specific builders)
  - `il.rewriter.generated.cs` (IL rewriter pattern - NEW)
  - `typeinference.generated.cs` (Type inference support)
- Update metamodels/templates only; regenerate instead of manual edits
- PRs modifying generated folders must include the upstream metamodel/template changes and the regeneration command used
- See `src/ast_generator/README.md` for guidance on choosing the right visitor/rewriter pattern

### Testing Requirements
- Unit tests: xUnit with FluentAssertions
- Parser tests: grammar samples in `src/parser/grammar/test_samples/*.5th`
- Integration tests: `test/runtime-integration-tests/`
- Add tests first; ensure failing state is visible before implementing fixes
- Prefer property-based testing over testing of single-point scenarios
- Testing should aim to verify all corner cases
- Tests should avoid testing of internal implementation details, and should aim not to embed dependencies on concrete implementations where possible

### Manual AST Smoke Test
To quickly validate AST builders after changes:
```csharp
using ast;
using ast_generated;

var intLiteral = new Int32LiteralExp { Value = 42 };
var builder = new Int32LiteralExpBuilder();
var result = builder.Build();
```
This should complete without errors.

### Expected Build Warnings (Acceptable)
- ANTLR: "rule expression contains an assoc terminal option in an unrecognized location"
- C# nullable reference warnings across the codebase
- Switch expression exhaustiveness warnings in parser

### Performance & Reliability
- Favor predictable, deterministic behavior over micro-optimizations
- Use integration tests to guard against regressions in code generation, parsing, and compilation
- Large builds are expected; never truncate or cancel build steps

### Security & Safety
- Avoid executing arbitrary code during generation or parsing
- Validate inputs; clearly separate user inputs from internal templates
- Do not introduce network calls or file system side-effects without explicit review

### Temporary Tooling & Scratch Artifacts
- Repository cleanliness is mandatory. Temporary debugging helpers, IL dumps, or scratch `.5th` programs must never be committed. Use local temp directories outside the repo when experimenting.
- `scripts/` is reserved for durable automation only. Promote a helper into `src/tools/` (or the appropriate project) once it has docs, tests, and a CLI contract; otherwise delete it after use.
- Prefer compiler flags such as `--keep-temp` to emit transient artifacts, but delete the generated `build_debug_il/` (or equivalent) output before committing. `.gitignore` covers common scratch patterns like `tmp_*.5th`, and those files must stay untracked.
- Document any sanctioned debugging workflow in `docs/debugging.md`. Update this constitution if a temporary workflow becomes part of the supported toolchain to capture build/test expectations.

## Troubleshooting

### Common Issues
1. **"java command not found"** - Install Java 17+ 
2. **ANTLR grammar errors** - Check grammar syntax in `src/parser/grammar/FifthLexer.g4` and `src/parser/grammar/FifthParser.g4`
3. **Missing generated files** - Run `just run-generator` to regenerate AST code
4. **Build timeouts** - Use longer timeout values, builds can legitimately take 1-2 minutes

### Key Files to Watch
- Always check generated files in `src/ast-generated/` after modifying `src/ast-model/AstMetamodel.cs` or `src/ast-model/ILMetamodel.cs`
- Parser changes require attention to both grammar files and `src/parser/AstBuilderVisitor.cs`
- Test failures in grammar parsing usually indicate issues in ANTLR grammar or visitor implementation
- Language transformation changes require updates to the transformation pipeline in `ParserManager.cs`

## Development Workflow & Quality Gates

### Standard Developer Loop
1. Define or refine the SpecKit task/spec
2. TDD: add/adjust tests under the appropriate test project
3. For AST changes: edit `AstMetamodel.cs` or `ILMetamodel.cs` → regenerate → build
4. For grammar changes: edit `FifthLexer.g4` or `FifthParser.g4` → update `AstBuilderVisitor.cs` → build
5. For transformation changes: add/modify visitors in `LanguageTransformations/` → update pipeline in `ParserManager.cs`
6. Build the full solution: `dotnet build fifthlang.sln`
7. Run focused tests first (e.g., `test/ast-tests/`), then broader suites
8. Update documentation if behavior changes
9. Prepare a PR with clear scope, rationale, and test evidence

### Always Validate Changes
After making any changes to the codebase:

1. **Build validation:**
   ```bash
   dotnet build fifthlang.sln  # NEVER CANCEL - wait up to 2 minutes
   ```

2. **Test validation:**
   ```bash
   dotnet test test/ast-tests/ast_tests.csproj  # NEVER CANCEL - wait up to 1 minute
   ```

3. **Manual AST functionality test:** (Use the smoke test above)

### PR Checklist (SpecKit Gate)
- Builds succeed for the full solution (no cancellations)
- New/updated tests added; all suites pass locally
- No hand-edits in `src/ast-generated/`; regeneration steps included
- Grammar changes have corresponding parser and visitor updates
- Transformation changes are properly integrated into the pipeline
- Public contracts and CLI help text are updated when behavior changes
- Rationale for any complexity increases is documented

### Review Standards
- Favor smallest viable change; keep diffs focused
- Ensure changes adhere to core principles (Sections I–XI)
- Confirm reproducibility by re-running documented commands
- Verify deterministic outputs and diagnostics for automation use
- Validate that AST transformations maintain proper lowering semantics

## SpecKit Integration

### Validations
- Run restore/build/test commands exactly as documented; flag early termination as failure
- Detect edits under `src/ast-generated/` without corresponding metamodel/template change → fail
- Ensure PR diffs include test changes when behavior changes
- Verify deterministic CLI outputs (no timestamps/non-deterministic ordering) where practical
- Validate transformation pipeline integrity when language transformations change

### Artifacts & Interfaces
- Executables: `src/ast_generator`, `src/compiler`, `src/code_generator`
- Inputs: `.5th` samples under `test/` and `src/parser/grammar/test_samples/`
- Outputs: stdout text, optional JSON streams for machine parsing
- Metamodels: `src/ast-model/AstMetamodel.cs`, `src/ast-model/ILMetamodel.cs`
- Grammars: `src/parser/grammar/FifthLexer.g4`, `src/parser/grammar/FifthParser.g4`

### Files to Watch
- Metamodel: `src/ast-model/AstMetamodel.cs`, `src/ast-model/ILMetamodel.cs`
- Templates: `src/ast_generator/Templates/`
- Generated: `src/ast-generated/` (read-only by policy)
- Grammars: `src/parser/grammar/FifthLexer.g4`, `src/parser/grammar/FifthParser.g4`
- Transformations: `src/compiler/LanguageTransformations/`
- Pipeline: `src/compiler/ParserManager.cs`
- Build configs: `global.json`, solution/project files

## Governance

This constitution supersedes ad-hoc practices for this repository. All PRs and reviews must verify compliance with the principles, constraints, and gates above. Amendments to this constitution require:
- A design note or PR section describing the change and its impact
- A migration plan for any breaking process or contract changes
- Updates to build/test commands if workflows change

SpecKit is considered a first-class consumer. Specifications and tasks must be kept in sync with code and tests, and their automation must not be broken by non-deterministic outputs or undocumented changes.

**Version**: 2.0.0 | **Ratified**: 2025-09-14 | **Last Amended**: 2025-09-14

**Major Changes in v2.0.0:**
- Incorporated all valuable content from AGENTS.md to eliminate external dependencies
- Updated parser/lexer references to reflect split grammar (FifthLexer.g4 + FifthParser.g4)
- Added comprehensive description of multi-pass compiler pipeline
- Detailed language transformations and their roles in AST lowering
- Clarified two-level AST design (main AST vs IL AST)
- Updated testing framework references (xUnit instead of TUnit)
- Enhanced project structure documentation
- Added transformation strategy guidelines
