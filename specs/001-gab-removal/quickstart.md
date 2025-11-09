# Quickstart: Removing Graph Assertion Block (GAB)

## Goals
- Remove GAB syntax and AST while preserving RDF features
- Keep diagnostics generic (no GAB-specific messages)

## Steps
1. Update grammar in `src/parser/grammar/FifthLexer.g4` and `FifthParser.g4` to delete tokens/rules for `<{ ... }>` blocks.
2. Update `src/parser/AstBuilderVisitor.cs` to remove visit paths for GAB nodes.
3. Update `src/ast-model/AstMetamodel.cs` to delete GAB-related node types; regenerate:
   ```bash
   just run-generator
   ```
4. Build and run parser tests:
   ```bash
   dotnet build fifthlang.sln
   dotnet test test/syntax-parser-tests/
   ```
5. Sweep docs/tests for GAB samples; convert to negative tests or remove. Validate examples:
   ```bash
   scripts/validate-examples.fish
   ```
6. Run RDF smoke tests to ensure no regressions:
   ```bash
   dotnet test test/kg-smoke-tests/
   dotnet test test/runtime-integration-tests/
   ```

## Notes
- Do not hand-edit `src/ast-generated/`; always regenerate after metamodel changes.
- Keep changes focused; avoid non-GAB refactors.
