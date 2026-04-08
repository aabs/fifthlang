---
id: steering-project-overview
title: Fifth Language Project Overview
inclusion: auto
---

# Fifth Language Project Overview

:::rule id="OVR-001" severity="warning" category="overview" domain="project"
Fifth Language is a C# .NET 10.0 compiler for the Fifth programming language. It uses an ANTLR-based split lexer and parser, AST code generation for builders and visitors, and a multi-pass compiler that lowers the AST through intermediate transformation stages.
:::

## Compiler Pipeline

:::rule id="OVR-002" severity="warning" category="pipeline" domain="project"
The canonical compiler flow is:

1. Lexical analysis and parsing into an ANTLR parse tree
2. Parse-tree transformation into a high-level AST through `AstBuilderVisitor.cs`
3. High-level AST lowering through multiple language-transformation passes
4. Roslyn code generation from the lowered AST into a PE assembly
:::

## Key Directories

:::rule id="OVR-003" severity="warning" category="structure" domain="project"
The major repository areas are:

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
:::

## Authoritative References

:::rule id="OVR-004" severity="warning" category="reference" domain="project"
Use these reference files according to their role:

- `.specify/memory/constitution.md` for architectural decisions and principles
- `AGENTS.md` for operational commands and workflow guidance
- `.specify/config.yml` for build and test command definitions
:::
