# Engineering Constitution
## Governance
- ARCH-001: Architecture guidance for the Fifth compiler must be concrete and measurable. Every rule in this document must include a compliance check an agent can perform.
## Dependency
- ARCH-002: Project references follow this strict DAG:
  
  ```text
  ast-model -> ast_generator -> ast-generated -> parser -> compiler -> tests
                                                 ^
                                            fifthlang.system
  ```
  
  No `.csproj` under `src/` may contain a `<ProjectReference>` pointing backward in this ordering.
  
  Verify: inspect project references under `src/` and reject any backward edge relative to this DAG.
- BUILD-008: The effective build order is:
  
  ```text
  ast-model -> ast_generator -> ast-generated -> parser -> compiler -> tests
  ```
  
  Always build the full solution rather than individual projects so dependency ordering is resolved correctly.
- PIPE-003: Transformation passes are order-dependent, and later passes may rely on invariants established by earlier passes.
## Generation
- ARCH-003: Files under `src/ast-generated/` are output, not source. Any diff modifying `src/ast-generated/` must also change `src/ast-model/AstMetamodel.cs` or `src/ast_generator/Templates/`.
  
  Verify: if `git diff --name-only` includes `src/ast-generated/`, it must also include `src/ast-model/` or `src/ast_generator/Templates/`.
- BUILD-006: After metamodel changes, regenerate AST output with:
  
  ```bash
  dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
  ```
- BUILD-011: AST code generation runs automatically before compilation via MSBuild targets. Manual generation is primarily for focused regeneration workflows.
- CODE-010: Never hand-edit files in `src/ast-generated/`.
- CODE-011: To modify the AST, edit the metamodels in `src/ast-model/` and then regenerate the generated output.
- GEN-001: The AST generator is authoritative for all files under `src/ast-generated/`. These files must never be hand-edited.
- GEN-002: The primary generated files are:
  
  - `builders.generated.cs` for builder pattern classes
  - `visitors.generated.cs` for visitor pattern classes
  - `rewriter.generated.cs` for lowering-oriented rewriters
  - `typeinference.generated.cs` for type inference support
- PR-003: Do not hand-edit `src/ast-generated/`. If the metamodel changes, include the required regeneration steps in the pull request.
- PR-015: Generated code changes must follow metamodel versioning rules.
## Ast
- ARCH-004: All AST node types, fields, and inheritance are defined in `src/ast-model/AstMetamodel.cs`. No hand-written class outside `ast-model` may subclass `AstThing` or introduce new AST node types.
  
  Verify: search `.cs` files outside `src/ast-model/` and `src/ast-generated/` for classes inheriting `AstThing`, `Expression`, `Statement`, or `TypeRef`. Any match is non-compliant.
- TEST-012: Use this quick smoke test after AST builder changes:
  
  ```csharp
  using ast;
  using ast_generated;
  
  var intLiteral = new Int32LiteralExp { Value = 42 };
  var builder = new Int32LiteralExpBuilder();
  var result = builder.Build();
  ```
  
  The builder construction should complete without errors.
## Backend
- ARCH-005: `LoweredAstToRoslynTranslator` is the sole bridge to Roslyn. No phase or visitor under `src/compiler/LanguageTransformations/` or `src/compiler/Pipeline/Phases/` may reference `Microsoft.CodeAnalysis`. Roslyn types must not leak into the AST model or transformation layer.
  
  Verify: `using Microsoft.CodeAnalysis` must not appear in `src/compiler/LanguageTransformations/`, `src/compiler/Pipeline/Phases/`, `src/ast-model/`, or `src/ast-generated/`.
## Pipeline
- ARCH-006: Every compiler phase implements `ICompilerPhase` and declares:
  
  - `DependsOn`: capability strings required from earlier phases.
  - `ProvidedCapabilities`: capability strings this phase makes available.
  
  `TransformationPipeline.RegisterPhase` enforces at registration time that every `DependsOn` entry is already provided. A phase that reads AST state from another phase without declaring the dependency is non-compliant.
  
  Verify: for each phase under `src/compiler/Pipeline/Phases/`, confirm every visitor or rewriter it instantiates operates only on AST state guaranteed by its declared `DependsOn`.
- ARCH-007: Each `ICompilerPhase` performs exactly one category of work:
  
  - Structural linking
  - Symbol resolution
  - Validation and diagnostics
  - Lowering and desugaring
  - Type annotation
  
  A compound phase combining sub-steps must document each in its XML summary and must not mix unrelated concerns.
  
  Verify: the `Transform` method should only instantiate visitors or rewriters serving its declared category. Distinct visitor types spanning multiple categories without XML-summary justification are non-compliant.
- ARCH-008: A phase receives `AstThing` and returns a new or mutated `AstThing` via `PhaseResult`. No phase may retain a reference to the input AST and mutate it after returning. The pipeline owns the AST reference between phases.
  
  Verify: phase `Transform` methods must not store the input `ast` parameter in instance or static fields. `PhaseResult.TransformedAst` is the only valid output path.
- ARCH-011: The phase sequence in `TransformationPipeline.CreateDefault()` is the canonical compilation order. Phases must not be conditionally reordered at runtime. Skipping via `PipelineOptions.SkipPhases` is permitted, but reordering is not.
  
  Verify: `CreateDefault()` contains only `RegisterPhase` calls in a fixed sequence with no conditional logic such as `if`, loops, or configuration-driven ordering.
- ARCH-012: Phases must not communicate through static mutable state, singletons, or thread-local storage. All inter-phase data flows through the AST or `PhaseContext`. The only exception is `DebugHelpers.DebugEnabled` as a read-only diagnostic flag.
  
  Verify: phase classes under `src/compiler/Pipeline/Phases/` must not declare `static` mutable fields. Visitors or rewriters they instantiate must not read or write `static` mutable fields other than `DebugHelpers`.
- OVR-002: The canonical compiler flow is:
  
  1. Lexical analysis and parsing into an ANTLR parse tree
  2. Parse-tree transformation into a high-level AST through `AstBuilderVisitor.cs`
  3. High-level AST lowering through multiple language-transformation passes
  4. Roslyn code generation from the lowered AST into a PE assembly
- PIPE-001: The compiler applies these passes sequentially in `ParserManager.cs`:
  
  1. `TreeLinkageVisitor` for parent-child relationships
  2. `BuiltinInjectorVisitor` for built-in function definitions
  3. `ClassCtorInserter` for default constructors
  4. `SymbolTableBuilderVisitor` for scoping symbol tables
  5. `PropertyToFieldExpander` for property syntax expansion
  6. `OverloadGatheringVisitor` for overload grouping
  7. `OverloadTransformingVisitor` for guard and subclause transformation
  8. `DestructuringVisitor` for destructuring property references
  9. `DestructuringLoweringRewriter` for lowering destructuring into variable declarations
  10. `TypeAnnotationVisitor` for type inference and annotation
- PR-005: Transformation changes must be integrated correctly into the pipeline in `ParserManager.cs`.
## Lowering
- ARCH-009: Transformation phases lower high-level AST constructs toward simpler forms consumable by `LoweredAstToRoslynTranslator`. No phase may introduce a higher-level construct than what it received.
  
  Verify: for any rewriter phase, output node types must be equal to or simpler than input node types, where simpler means closer to what `LoweredAstToRoslynTranslator.TranslateStatement` or `TranslateExpression` directly handle.
- PR-011: Validate that AST transformations maintain correct lowering semantics through the pipeline.
## Diagnostics
- ARCH-010: Phases report errors and warnings exclusively through `PhaseResult.Diagnostics` or `PhaseContext.Diagnostics`. Direct `Console.Error` writes are permitted only when `DebugHelpers.DebugEnabled` is true. No phase may write to `Console.Out`.
  
  Verify: `Console.Error.WriteLine` in phase `Transform` methods must be guarded by `DebugHelpers.DebugEnabled`. `Console.WriteLine` or `Console.Out` calls are non-compliant.
- BUILD-012: The following warnings are expected and safe to ignore unless they change unexpectedly:
  
  - ANTLR `assoc` option warnings
  - C# nullable reference warnings
  - Switch exhaustiveness warnings
## Parser
- ARCH-013: All parseable Fifth syntax is defined in `FifthLexer.g4` for tokens and `FifthParser.g4` for rules. No code outside the ANTLR grammar files may define new syntax. `AstBuilderVisitor` translates parse trees to AST but must not accept token sequences the grammar rejects.
  
  Verify: every `Visit*` method suffix in `AstBuilderVisitor.cs` must match a named rule in `FifthParser.g4`.
- ARCH-014: Every named parser rule in `FifthParser.g4` that produces a semantic construct must have a corresponding `Visit*` method in `AstBuilderVisitor.cs`, and vice versa.
  
  Verify: extract rule names from `FifthParser.g4` using lines matching `ruleName :`. Extract `Visit*` method names from `AstBuilderVisitor.cs`. The sets must align, allowing for ANTLR alternation labels such as `#labeledAlt`.
- BUILD-010: ANTLR grammar compilation happens automatically during the parser project build. Do not add redundant manual generation steps to the normal workflow.
- CODE-012: When grammar behavior changes, update both `FifthLexer.g4` and `FifthParser.g4` as needed.
- CODE-013: Always update `AstBuilderVisitor.cs` when grammar changes alter the parse tree or surface syntax.
- PR-004: Any grammar change must have corresponding updates in both the parser grammar and the AST builder visitor.
- PR-017: When adding or updating `.5th` examples or test programs:
  
  1. Validate parsing locally with parser or syntax tests
  2. Use grammar-supported forms only and avoid legacy shorthand
  3. Add `CopyToOutputDirectory` metadata in the test `.csproj` when an integration test consumes the sample
  4. Run the relevant integration tests before committing
  5. Update the grammar files and constitution if the change intentionally introduces new surface syntax
## Visitor
- ARCH-015: Use the visitor and rewriter base classes according to the operation being performed:
  
  | Operation | Base class | When to use |
  |---|---|---|
  | Read-only analysis | `BaseAstVisitor` | Never modifies AST |
  | Type-preserving AST modification | `DefaultRecursiveDescentVisitor` | Same node types in and out |
  | Cross-type rewrites, statement hoisting, desugaring | `DefaultAstRewriter` | Returns `RewriteResult` with prologue or changes node types |
  
  A visitor returning `RewriteResult` with non-empty prologue must extend `DefaultAstRewriter`. A visitor that only collects information must not extend `DefaultAstRewriter` or `DefaultRecursiveDescentVisitor`.
  
  Verify: for each visitor or rewriter under `src/compiler/LanguageTransformations/`, confirm the base class matches the operation kind. A `BaseAstVisitor` subclass mutating AST nodes, or a `DefaultAstRewriter` that never returns prologue and never changes node types, is non-compliant.
- GEN-005: Use `BaseAstVisitor` for read-only analysis such as symbol tables, diagnostics, and validation. This pattern must not modify the AST.
- GEN-006: Use `DefaultRecursiveDescentVisitor` for type-preserving AST modifications. This pattern must not change node types or hoist statements.
- GEN-007: Use `DefaultAstRewriter` for statement-level desugaring, cross-type rewrites, and expression hoisting. This is the preferred pattern for new lowering passes because it returns `RewriteResult` with a node and prologue.
  
  Choose this pattern when introducing temporary variables, breaking down high-level constructs, transforming expression types, or performing any lowering that requires statement insertion.
## Prerequisites
- BUILD-001: The local environment must include:
  
  - .NET 10.0 SDK pinned by `global.json` to `10.0.100`
  - Java 17 or newer for ANTLR grammar compilation
  - The ANTLR runtime package and the jar at `src/parser/tools/antlr-4.13.2-complete.jar`
## Commands
- BUILD-002: Use the solution restore command as the standard restore entry point:
  
  ```bash
  dotnet restore fifthlang.sln
  ```
  
  This operation typically takes about 70 seconds. Do not cancel it. Use a timeout of at least 120 seconds when automation controls execution time.
- BUILD-003: Use the solution build command as the standard build entry point:
  
  ```bash
  dotnet build fifthlang.sln
  ```
  
  This operation typically takes about 60 seconds. Do not cancel it. Use a timeout of at least 120 seconds when automation controls execution time.
- TEST-007: The default regression command is:
  
  ```bash
  dotnet test fifthlang.sln
  ```
- TEST-008: Use this quick smoke command while iterating:
  
  ```bash
  dotnet test test/ast-tests/ast_tests.csproj
  ```
- TEST-009: Use this focused parser command when grammar behavior changes:
  
  ```bash
  dotnet test test/syntax-parser-tests/ -v minimal
  ```
- TEST-010: Use filtered runtime integration tests for focused investigation:
  
  ```bash
  dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj --filter "FullyQualifiedName~YourTestName" -v minimal
  ```
- TEST-011: Validate knowledge-graph changes with:
  
  ```bash
  dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj
  ```
## Testing
- BUILD-004: Use the solution test command as the default regression gate:
  
  ```bash
  dotnet test fifthlang.sln
  ```
  
  Do not cancel this run. Use a timeout of at least 5 minutes when automation controls execution time.
- BUILD-005: Use this command for a quick smoke-test subset while iterating locally:
  
  ```bash
  dotnet test test/ast-tests/ast_tests.csproj
  ```
- BUILD-014: These targeted commands are available for narrower local validation:
  
  ```bash
  just test-ast
  just test-runtime
  just test-syntax
  just test-all-roslyn
  ```
  
  Use them to iterate locally, but retain the full solution test run as the regression gate.
- PR-002: Pull requests that change behavior must add or update tests, and all relevant suites must pass locally.
## Verification
- BUILD-007: Confirm the toolchain before debugging restore or build failures:
  
  ```bash
  dotnet --version
  java -version
  ```
  
  The .NET command should report `10.0.x`, and Java should report version 17 or newer.
## Workflow
- BUILD-009: Do not cancel restore, build, test, or generation operations. The documented timings in this repository are normal and expected.
- GEN-003: To change generated AST output:
  
  1. Edit `src/ast-model/AstMetamodel.cs`
  2. Update templates under `src/ast_generator/Templates/` when template behavior must change
  3. Regenerate with `just run-generator`
  4. Build the full solution with `dotnet build fifthlang.sln`
- GRAM-002: When grammar behavior changes:
  
  1. Edit `FifthLexer.g4` for tokens and keywords and `FifthParser.g4` for syntax rules as needed
  2. Update `AstBuilderVisitor.cs` for the new syntax constructs
  3. Add test samples under `src/parser/grammar/test_samples/*.5th`
  4. Rely on the normal build to run ANTLR compilation automatically
  5. Run parser tests with `dotnet test test/syntax-parser-tests/ -v minimal`
  6. Run the full regression suite with `dotnet test fifthlang.sln`
- PIPE-008: To add a new transformation:
  
  1. Create the visitor or rewriter in `src/compiler/LanguageTransformations/`
  2. Choose the correct base pattern using the code-generation steering guidance
  3. Register the transformation in `src/compiler/ParserManager.cs`
  4. Add tests in `test/ast-tests/` or `test/runtime-integration-tests/`
  5. Build the solution and run the full test suite
## Validation
- BUILD-013: After any change, validate in this order:
  
  1. `dotnet build fifthlang.sln`
  2. `dotnet test fifthlang.sln`
  3. Verify runtime behavior
  
  Compilation alone is not sufficient validation.
- GRAM-003: All `.5th` files in `docs/`, `specs/`, `test/`, and `src/parser/grammar/test_samples/` must parse with the current grammar. CI enforces this with the `Validate .5th samples (parser-check)` step.
  
  Run `just validate-examples` locally before committing.
- GRAM-008: Intentionally invalid files are excluded from example validation by these heuristics:
  
  - directory matches under `*/Invalid/*`
  - filenames containing `invalid`
  - an explicit negative-test comment marker in the file
  
  For debugging, force validation of negative examples with:
  
  ```bash
  dotnet run --project src/tools/validate-examples/validate-examples.csproj -- --include-negatives
  ```
- PR-001: Before opening or updating a pull request:
  
  1. Build the full solution with `dotnet build fifthlang.sln` and do not cancel the run
  2. Run the full test suite with `dotnet test fifthlang.sln`
  3. Validate grammar examples with `just validate-examples`
## Design
- CODE-001: Every feature should start as a focused library under `src/` with a clear public contract.
- CODE-002: Prefer the simplest design that works. Do not introduce incidental complexity or abstractions that are not required.
- GEN-004: `AstMetamodel.cs` defines rich, high-level constructs that mirror source language features. These constructs are lowered through transformation passes before Roslyn code generation.
- PIPE-002: Each visitor or rewriter in the transformation pipeline must have one well-defined responsibility.
- PIPE-004: Prefer several simple, comprehensible passes over a single pass that mixes unrelated transformation logic.
- PIPE-007: Prefer expressing language adaptation as AST transformations rather than pushing additional complexity into code generation.
- TEST-004: Prefer property-based testing over single-point scenarios, and aim to verify corner cases rather than only happy paths.
- TEST-005: Avoid testing internal implementation details and avoid depending on concrete implementations where looser behavioral validation is possible.
## Maintainability
- CODE-003: Make targeted, minimal changes that respect existing structure and public APIs.
- PR-007: When a change increases complexity, document the rationale in the pull request.
## Quality
- CODE-004: Do not add catch-all error handling that hides defects. Any change that increases complexity must be justified explicitly.
- PR-008: Favor the smallest viable change and keep diffs focused.
- PR-009: Confirm reproducibility by re-running the documented commands rather than relying on assumptions.
- PR-010: Verify that generated outputs and diagnostics remain deterministic.
## Platform
- CODE-005: Target C# 14, or the latest language version supported by the .NET 10 SDK, and target .NET 10.0.
## Versioning
- CODE-006: Use Semantic Versioning in `MAJOR.MINOR.PATCH` form for all packages.
- PR-014: Apply a minor or major version bump when the change warrants it.
## Cli
- CODE-007: Use stdin and arguments for input, stdout for primary output, and stderr for errors and diagnostics.
- CODE-008: Support human-readable text by default and add JSON output where it materially improves automation.
- CODE-009: Favor deterministic, scriptable commands. Output must be stable and must not depend on timestamps or non-deterministic ordering.
## Repository
- CODE-014: Do not commit temporary debugging helpers, IL dumps, or scratch `.5th` programs.
- CODE-015: The `scripts/` directory is reserved for durable automation only.
- CODE-016: Do not commit `tmp_*.5th`, `build_debug_il/`, `KEEP_FIFTH_TEMP`, or outputs produced by `--keep-temp`.
- CODE-017: Use `.gitignore` patterns and local temporary directories for experiments rather than leaving scratch assets in the repository.
## Security
- CODE-018: Avoid executing arbitrary code during generation or parsing.
- CODE-019: Validate inputs and keep user inputs separated from internal templates.
- CODE-020: Do not introduce network calls or file-system side effects without explicit review.
## Dependencies
- CODE-021: The core package set in this repository includes:
  
  - `Antlr4.Runtime.Standard` for the ANTLR runtime
  - `RazorLight` for code-generation templates
  - `System.CommandLine` for CLI parsing
  - `xUnit` and `FluentAssertions` for testing
  - `dunet` for discriminated unions
  - `Vogen` for value-object generation
## Reference
- GEN-008: Use `src/ast_generator/README.md` as the detailed reference for visitor and rewriter pattern selection.
- OVR-004: Use these reference files according to their role:
  
  - `.specify/memory/constitution.md` for architectural decisions and principles
  - `AGENTS.md` for operational commands and workflow guidance
  - `.specify/config.yml` for build and test command definitions
- SYN-007: Use these locations when looking for canonical syntax examples:
  
  - `test/ast-tests/CodeSamples/*.5th`
  - `src/parser/grammar/test_samples/*.5th`
  - `docs/Getting-Started/`
## Review
- GEN-009: Any pull request modifying `src/ast-generated/` must include:
  
  - The upstream metamodel or template changes
  - The regeneration command used
  - Confirmation that the generated files were not hand-edited
## Architecture
- GRAM-001: The parser surface is divided across three primary assets:
  
  - `src/parser/grammar/FifthLexer.g4` for tokens, keywords, literals, and lexical structure
  - `src/parser/grammar/FifthParser.g4` for syntactic rules and grammar structure
  - `src/parser/AstBuilderVisitor.cs` for parse-tree to high-level AST transformation
## Syntax
- GRAM-004: Do not use `var <name> =` in examples or tests. Use `name: type =` or the appropriate canonical Fifth form.
- GRAM-005: Do not use declarations such as `graph g =` or `triple t =`. Use `g: graph =` or `t: triple =`.
- GRAM-006: Do not use the legacy `when` guard shorthand. Use the parameter constraint form `param: Type | <expr>` together with block bodies.
- SYN-001: Basic syntax looks like this:
  
  ```fifth
  class Person {
      Name: string;
      Height: float;
  }
  
  main() => myprint(5 + 6);
  myprint(int x) => std.print(x);
  ```
- SYN-002: Use `name: type = value` form. Never use `var name =` in C# or JavaScript style, and never use type-first forms such as `type name =`.
  
  ```fifth
  x: int = 42;
  g: graph = KG.CreateGraph();
  ```
## Guards
- GRAM-007: The canonical contrast for guard syntax is:
  
  ```fifth
  // INVALID
  myprint(int x) when x == 0 => std.print(x);
  
  // VALID
  myprint(int x | x == 0) { std.print(x); }
  ```
- SYN-004: Use the parameter constraint form with block bodies:
  
  ```fifth
  myprint(int x | x == 0) { std.print(x); }
  ```
  
  Do not use the legacy `when` shorthand.
  
  ```fifth
  // INVALID
  // myprint(int x) when x == 0 => std.print(x);
  ```
## Knowledge Graph
- GRAM-009: Use these canonical knowledge-graph forms in examples and tests:
  
  - `name: store = sparql_store(<iri>);`
  - `store default = sparql_store(<iri>);`
  - `KG.CreateGraph()` to create graphs
  - `graph += triple` to add triples
  
  Validate these flows with `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj`.
- SYN-005: Use the canonical knowledge-graph forms:
  
  ```fifth
  myStore: store = sparql_store(<http://example.org/store>);
  store default = sparql_store(<http://example.org/default>);
  
  g: graph = KG.CreateGraph();
  // Add triples with += operator
  ```
- SYN-006: Use these literal forms in syntax and examples:
  
  - TriG literals use `<{...}>`
  - SPARQL literals use `?<...>`
  - Object-position literal values may be strings, booleans, chars, signed integers, unsigned integers, `float`, `double`, or `decimal`
## Overview
- OVR-001: Fifth Language is a C# .NET 10.0 compiler for the Fifth programming language. It uses an ANTLR-based split lexer and parser, AST code generation for builders and visitors, and a multi-pass compiler that lowers the AST through intermediate transformation stages.
## Structure
- OVR-003: The major repository areas are:
  
  - `src/ast-model/` for AST metamodel definitions including `AstMetamodel.cs`
  - `src/ast-generated/` for generated builders, visitors, and rewriters that must not be hand-edited
  - `src/ast_generator/` for the generator that produces `ast-generated/`
  - `src/parser/grammar/` for `FifthLexer.g4` and `FifthParser.g4`
  - `src/parser/AstBuilderVisitor.cs` for parse-tree to AST conversion
  - `src/compiler/LanguageTransformations/` for AST transformation passes
  - `src/compiler/ParserManager.cs` for transformation pipeline coordination
  - `src/fifthlang.system/` for built-in system functions and knowledge graph support
  - `test/ast-tests/` for AST and generator tests
  - `test/syntax-parser-tests/` for grammar parsing tests
  - `test/runtime-integration-tests/` for end-to-end verification tests
## Documentation
- PIPE-005: Document dependencies between transformation passes whenever later stages rely on earlier ones.
## Correctness
- PIPE-006: Each transformation pass must preserve AST validity and type safety.
## Code Generation
- PIPE-009: The compiler uses `LoweredAstToRoslynTranslator` to emit C# syntax trees from the lowered AST for Roslyn compilation and PE or PDB emission. Roslyn-generated PDBs must include full line-and-column sequence points so debugging fidelity is preserved.
## Contracts
- PR-006: When behavior changes affect public contracts or CLI behavior, update the relevant contracts and CLI help text.
## Breaking Change
- PR-012: Every breaking change must include a migration note in the pull request.
- PR-013: Breaking changes must include updated tests that reflect the new behavior.
## Deprecation
- PR-016: Every deprecation must be documented and covered by tests.
## Functions
- SYN-003: Function definitions can use either expression bodies or block bodies:
  
  ```fifth
  add(int a, int b) => a + b;
  
  greet(string name) {
      std.print("Hello " + name);
  }
  ```
## Framework
- TEST-001: The standard test stack is:
  
  - `xUnit` as the test framework
  - `FluentAssertions` for assertions
  - `test/ast-tests/`, `test/syntax-parser-tests/`, and `test/runtime-integration-tests/` as the primary test projects
## Process
- TEST-002: Practice TDD by writing tests, seeing them fail, and then implementing the change. Never mask failing tests with broad `try` or `catch` blocks. Let failures surface so CI reflects the true repository state.
## Completion
- TEST-003: A feature is not complete until end-to-end tests prove that it:
  
  1. Uses actual Fifth language syntax including constructs such as TriG literals, SPARQL literals, and operators
  2. Executes successfully at runtime rather than merely compiling
  3. Produces results that are accessible and correct
  4. Exercises the major code paths and result types involved
  
  Features with only compilation tests or with failing runtime tests are incomplete.
## Fixtures
- TEST-006: Tests that reference `.5th` sample files must declare `CopyToOutputDirectory` metadata in the owning test `.csproj`.
