---
id: steering-coding-standards
title: Coding Standards And Principles
inclusion: auto
---

# Coding Standards and Principles

## Core Principles

:::rule id="CODE-001" mandatory="false" category="design"
Every feature should start as a focused library under `src/` with a clear public contract.
:::

:::rule id="CODE-002" mandatory="false" category="design"
Prefer the simplest design that works. Do not introduce incidental complexity or abstractions that are not required.
:::

:::rule id="CODE-003" mandatory="false" category="maintainability"
Make targeted, minimal changes that respect existing structure and public APIs.
:::

:::rule id="CODE-004" category="quality"
Do not add catch-all error handling that hides defects. Any change that increases complexity must be justified explicitly.
:::

## C# Conventions

:::rule id="CODE-005" mandatory="false" category="platform"
Target C# 14, or the latest language version supported by the .NET 10 SDK, and target .NET 10.0.
:::

:::rule id="CODE-006" mandatory="false" category="versioning"
Use Semantic Versioning in `MAJOR.MINOR.PATCH` form for all packages.
:::

## CLI and Text I/O Discipline

:::rule id="CODE-007" category="cli"
Use stdin and arguments for input, stdout for primary output, and stderr for errors and diagnostics.
:::

:::rule id="CODE-008" mandatory="false" category="cli"
Support human-readable text by default and add JSON output where it materially improves automation.
:::

:::rule id="CODE-009" mandatory="false" category="cli"
Favor deterministic, scriptable commands. Output must be stable and must not depend on timestamps or non-deterministic ordering.
:::

## File Editing Rules

:::rule id="CODE-010" category="generation"
Never hand-edit files in `src/ast-generated/`.
:::

:::rule id="CODE-011" category="generation"
To modify the AST, edit the metamodels in `src/ast-model/` and then regenerate the generated output.
:::

:::rule id="CODE-012" category="parser"
When grammar behavior changes, update both `FifthLexer.g4` and `FifthParser.g4` as needed.
:::

:::rule id="CODE-013" category="parser"
Always update `AstBuilderVisitor.cs` when grammar changes alter the parse tree or surface syntax.
:::

## Repository Cleanliness

:::rule id="CODE-014" category="repository"
Do not commit temporary debugging helpers, IL dumps, or scratch `.5th` programs.
:::

:::rule id="CODE-015" category="repository"
The `scripts/` directory is reserved for durable automation only.
:::

:::rule id="CODE-016" category="repository"
Do not commit `tmp_*.5th`, `build_debug_il/`, `KEEP_FIFTH_TEMP`, or outputs produced by `--keep-temp`.
:::

:::rule id="CODE-017" mandatory="false" category="repository"
Use `.gitignore` patterns and local temporary directories for experiments rather than leaving scratch assets in the repository.
:::

## Security

:::rule id="CODE-018" category="security"
Avoid executing arbitrary code during generation or parsing.
:::

:::rule id="CODE-019" category="security"
Validate inputs and keep user inputs separated from internal templates.
:::

:::rule id="CODE-020" category="security"
Do not introduce network calls or file-system side effects without explicit review.
:::

## Key NuGet Packages

:::rule id="CODE-021" mandatory="false" category="dependencies"
The core package set in this repository includes:

- `Antlr4.Runtime.Standard` for the ANTLR runtime
- `RazorLight` for code-generation templates
- `System.CommandLine` for CLI parsing
- `xUnit` and `FluentAssertions` for testing
- `dunet` for discriminated unions
- `Vogen` for value-object generation
:::
