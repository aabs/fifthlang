# Contracts (External APIs)

This feature introduces no external service/API endpoints. Contracts are internal to the compiler toolchain:

- AST metamodel additions in `src/ast-model/AstMetamodel.cs`
- Parser grammar rules in `src/parser/grammar/FifthParser.g4` and `FifthLexer.g4`
- Generated builders/visitors in `src/ast-generated/` (do not edit)
- Compiler transformation behaviors documented in the spec and tests

If API exposure is later required (e.g., CLI flags or LSP), add separate contracts under this folder with the appropriate schema (OpenAPI/JSON schema) and reference them from the plan.
