---
description: coding-standards
inclusion: always
---
## Design
- CODE-001: Every feature should start as a focused library under `src/` with a clear public contract.
- CODE-002: Prefer the simplest design that works. Do not introduce incidental complexity or abstractions that are not required.
## Maintainability
- CODE-003: Make targeted, minimal changes that respect existing structure and public APIs.
## Quality
- CODE-004: Do not add catch-all error handling that hides defects. Any change that increases complexity must be justified explicitly.
## Platform
- CODE-005: Target C# 14, or the latest language version supported by the .NET 10 SDK, and target .NET 10.0.
## Versioning
- CODE-006: Use Semantic Versioning in `MAJOR.MINOR.PATCH` form for all packages.
## Cli
- CODE-007: Use stdin and arguments for input, stdout for primary output, and stderr for errors and diagnostics.
- CODE-008: Support human-readable text by default and add JSON output where it materially improves automation.
- CODE-009: Favor deterministic, scriptable commands. Output must be stable and must not depend on timestamps or non-deterministic ordering.
## Generation
- CODE-010 [MANDATORY]: Never hand-edit files in `src/ast-generated/`.
- CODE-011 [MANDATORY]: To modify the AST, edit the metamodels in `src/ast-model/` and then regenerate the generated output.
## Parser
- CODE-012 [MANDATORY]: When grammar behavior changes, update both `FifthLexer.g4` and `FifthParser.g4` as needed.
- CODE-013 [MANDATORY]: Always update `AstBuilderVisitor.cs` when grammar changes alter the parse tree or surface syntax.
## Repository
- CODE-014 [MANDATORY]: Do not commit temporary debugging helpers, IL dumps, or scratch `.5th` programs.
- CODE-015 [MANDATORY]: The `scripts/` directory is reserved for durable automation only.
- CODE-016 [MANDATORY]: Do not commit `tmp_*.5th`, `build_debug_il/`, `KEEP_FIFTH_TEMP`, or outputs produced by `--keep-temp`.
- CODE-017 [MANDATORY]: Use `.gitignore` patterns and local temporary directories for experiments rather than leaving scratch assets in the repository.
## Security
- CODE-018 [MANDATORY]: Avoid executing arbitrary code during generation or parsing.
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
