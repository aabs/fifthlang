# Parser Project

This project implements the low-level parsing of Fifth source code using ANTLR 4. It owns the split grammar and the parse-tree → AST bridge.

For overarching rules and workflows, see `/.specify/memory/constitution.md` (authoritative) and the workspace `AGENTS.md`.

## Grammar layout

- `grammar/FifthLexer.g4` — tokens, keywords, literals
- `grammar/FifthParser.g4` — syntactic rules
- Generated sources: emitted into `grammar/grammar/` by the project build
- ANTLR tool jar: `src/parser/tools/antlr-4.8-complete.jar`

ANTLR generation runs automatically during build via the `GenerateParser` target. The target invokes ANTLR 4.8 to emit C# sources. Manual regeneration is rarely needed; if you must, prefer running the project build.

## AST building

- `AstBuilderVisitor.cs` is the key non-generated component that walks the ANTLR parse tree and constructs the high-level AST defined in `src/ast-model/`.
- Any grammar change that introduces or reshapes syntax MUST be accompanied by aligned updates in `AstBuilderVisitor.cs`.

## Do-not-edit generated code

- Never hand-edit files under `src/ast-generated/` (AST builders/visitors/rewriters). Edit the metamodel(s) and regenerate instead; see the constitution and workspace `AGENTS.md` for details.

## Expected diagnostics

- Be careful when introducing changes; the build must not produce ANTLR errors. Some warnings like the assoc option location warning are expected and acceptable (documented as benign).

## Validation checklist (run locally before pushing)

1) Build and generate
	- Build full solution (ANTLR runs automatically)
	- Ensure no ANTLR errors; warnings may be acceptable per constitution

2) Example and docs compliance
	- Run the example validator: `scripts/validate-examples.fish` (fish shell)
	- Fix any `.5th` samples that don’t parse; mark intentional negatives so the validator skips them

3) Parser tests
	- Add/update samples under `src/parser/grammar/test_samples/*.5th`
	- Run `dotnet test test/syntax-parser-tests/ -v minimal`

4) AST bridge
	- Update `AstBuilderVisitor.cs` to reflect grammar changes
	- Run `dotnet test test/ast-tests/ast_tests.csproj` and integration tests as needed

## Notes

- Keep grammar, visitor, and tests in lockstep. Small, focused changes are easier to validate.
- Prefer grammar-supported forms only in docs and examples (no legacy `when` guard shorthand). See the grammar compliance section in the constitution.