# Fifth Language Engineering Constitution

This constitution governs how we build, test, version, and evolve the Fifth language projects in this repository, including the AST model, code generator, parser, compiler, and system libraries. It exists to create a predictable, testable, and observable development flow that GitHub SpecKit and automation can rely on.

Key reference: see `AGENTS.md` and `.github/copilot-instructions.md` for authoritative build, test, and generation commands. When in doubt, those files take precedence for operational details.

## Core Principles

### I. Library-First, Contracts-First
Every feature starts as a focused library under `src/` with a clear, documented purpose and public contract. Libraries must be self-contained, independently buildable, and testable with TUnit. Avoid organizational or “glue-only” libraries. Contracts are expressed through:
- AST metamodels in `src/ast-model/`
- Generated builders/visitors in `src/ast-generated/`
- ANTLR grammar in `src/parser/grammar/Fifth.g4`
- Public CLIs (e.g., `ast_generator`, `compiler`) with text I/O

### II. CLI and Text I/O Discipline
Each executable surface must provide a CLI entry that communicates via text I/O:
- stdin/args → input; stdout → primary output; stderr → errors/diagnostics
- Support human-readable text; add JSON output where suitable for automation
- Favor deterministic, scriptable commands to enable SpecKit orchestration

### III. Generator-as-Source-of-Truth (Do Not Hand-Edit Generated Code)
The AST generator is authoritative for builders, visitors, IL builders, and type inference support. Never hand-edit files under `src/ast-generated/`. To change generated outputs:
1. Update `src/ast-model/AstMetamodel.cs` (or templates under `src/ast_generator/Templates/` when appropriate)
2. Regenerate via `make run-generator` or the documented `dotnet run` invocation
3. Build the full solution to validate

### IV. Test-First (Non-Negotiable)
Practice TDD: write/approve tests, see them fail, then implement. Use TUnit + FluentAssertions across:
- `test/ast-tests/` for AST and generator correctness
- `test/syntax-parser-tests/` for grammar parsing
- `test/runtime-integration-tests/` for end-to-end verification
Never mask failing tests with broad try/catch. Let failures surface so CI and SpecKit correctly reflect state.

### V. Reproducible Builds & Toolchain Discipline
Tooling is pinned and enforced for reproducibility:
- .NET SDK 8.0.x per `global.json` (8.0.118)
- Java 17+ for ANTLR; ANTLR 4.8 runtime with the jar at `src/parser/tools/antlr-4.8-complete.jar`
- Build order: ast-model → ast_generator → ast-generated → parser → code_generator → compiler → tests
- Critical rule: NEVER CANCEL restore/build/test/generation tasks. Allow up to 1–2 minutes for completion as documented.

### VI. Simplicity, Minimal Surface, and Safety
Prefer the simplest design that works. Avoid incidental complexity and non-required abstractions (YAGNI). Make targeted, minimal changes that respect existing structure and APIs. Do not add catch-all error handling that hides defects. Changes that increase complexity must be justified in the PR.

### VII. Parser & Grammar Integrity
Changes to `Fifth.g4` require corresponding tests and visitor updates. Build runs ANTLR automatically. Expected warnings (like assoc option location) can be ignored if documented as benign. Parser transformations must keep the AST and visitor consistency intact.

### VIII. Observability & Diagnostics
Text I/O is the primary observability mechanism. Emit clear, actionable diagnostics to stderr. Prefer structured messages (line/column, file paths) for parsers/compilers. When logging is needed, use standard .NET logging abstractions; avoid custom frameworks. Output must be deterministic and stable to support automated parsing by SpecKit.

### IX. Versioning & Backward Compatibility
Use Semantic Versioning (MAJOR.MINOR.PATCH). Breaking changes require:
- A migration note in the PR
- Updated tests reflecting new behavior
- A minor/major version bump as appropriate
Generated code changes follow metamodel versioning. Deprecations must be documented and tested.

## Engineering Constraints & Standards

### Toolchain & Environment
- .NET 8.0 SDK required; Java 17+ required for ANTLR operations
- Validate with the exact command sequences in `AGENTS.md`
- For build timing, set timeouts per documented guidance; do not prematurely interrupt tasks

### Code Generation
- All generated code resides in `src/ast-generated/`
- Update metamodels/templates only; regenerate instead of manual edits
- PRs modifying generated folders must include the upstream metamodel/template changes and the regeneration command used

### Testing Requirements
- Unit tests: TUnit with FluentAssertions
- Parser tests: grammar samples in `src/parser/grammar/test_samples/*.5th`
- Integration tests: `test/runtime-integration-tests/`
- Add tests first; ensure failing state is visible before implementing fixes

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

## Development Workflow & Quality Gates

### Standard Developer Loop
1. Define or refine the SpecKit task/spec
2. TDD: add/adjust tests under the appropriate test project
3. For AST changes: edit `AstMetamodel.cs` → regenerate → build
4. Build the full solution: `dotnet build fifthlang.sln`
5. Run focused tests first (e.g., `test/ast-tests/`), then broader suites
6. Update docs (`README.md`, `AGENTS.md` references) if behavior changes
7. Prepare a PR with clear scope, rationale, and test evidence

### Required Commands (authoritative)
As documented in `AGENTS.md` and `.github/copilot-instructions.md`:
- Restore: `dotnet restore fifthlang.sln`
- Build: `dotnet build fifthlang.sln`
- Tests (AST): `dotnet test test/ast-tests/ast_tests.csproj`
- All tests: `dotnet test fifthlang.sln`
- Generate AST: `make run-generator` or `dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated`

### PR Checklist (SpecKit Gate)
- Builds succeed for the full solution (no cancellations)
- New/updated tests added; all suites pass locally
- No hand-edits in `src/ast-generated/`; regeneration steps included
- Grammar changes have corresponding parser and visitor updates
- Public contracts and CLI help text are updated when behavior changes
- Rationale for any complexity increases is documented

### Review Standards
- Favor smallest viable change; keep diffs focused
- Ensure changes adhere to core principles (Sections I–IX)
- Confirm reproducibility by re-running documented commands
- Verify deterministic outputs and diagnostics for automation use

## SpecKit Integration

### Validations
- Run restore/build/test commands exactly as documented; flag early termination as failure
- Detect edits under `src/ast-generated/` without corresponding metamodel/template change → fail
- Ensure PR diffs include test changes when behavior changes
- Verify deterministic CLI outputs (no timestamps/non-deterministic ordering) where practical

### Artifacts & Interfaces
- Executables: `src/ast_generator`, `src/compiler`, `src/code_generator`
- Inputs: `.5th` samples under `test/` and `src/parser/grammar/test_samples/`
- Outputs: stdout text, optional JSON streams for machine parsing

### Files to Watch
- Metamodel: `src/ast-model/AstMetamodel.cs`
- Templates: `src/ast_generator/Templates/`
- Generated: `src/ast-generated/` (read-only by policy)
- Grammar: `src/parser/grammar/Fifth.g4`
- Build configs: `global.json`, solution/project files
- Guidance: `AGENTS.md`, `.github/copilot-instructions.md`

## Governance

This constitution supersedes ad-hoc practices for this repository. All PRs and reviews must verify compliance with the principles, constraints, and gates above. Amendments to this constitution require:
- A design note or PR section describing the change and its impact
- Updates to `AGENTS.md` and/or `.github/copilot-instructions.md` if commands or workflows change
- A migration plan for any breaking process or contract changes

SpecKit is considered a first-class consumer. Specifications and tasks must be kept in sync with code and tests, and their automation must not be broken by non-deterministic outputs or undocumented changes.

**Version**: 1.0.0 | **Ratified**: 2025-09-13 | **Last Amended**: 2025-09-13