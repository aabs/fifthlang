# PR Checklist (Fifth Language Engineering Constitution)

Please confirm each item before requesting review.

- [ ] Builds the full solution without cancellation using documented commands
- [ ] Tests added/updated first and now passing locally (unit/parser/integration as applicable)
- [ ] No manual edits under `src/ast-generated/`; included metamodel/template changes and regeneration steps
- [ ] For grammar changes: visitor updates and parser tests included
- [ ] CLI behavior/contracts documented; outputs are deterministic and scriptable
- [ ] Complexity increases are justified in the description
- [ ] Versioning considered (SemVer); migration notes provided for breakages
- [ ] Docs updated (`README.md`, `AGENTS.md` references) where behavior or commands changed

## Summary
- What changed and why?
- User-visible impact (CLI, AST, grammar, codegen)?
- Alternatives considered?

## Validation
Provide commands you ran locally (paste exact output snippets as needed):

```
# Required sequence
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet --info
java -version

dotnet restore fifthlang.sln
# Regenerate when changing AST metamodel/templates
# dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

dotnet build fifthlang.sln --no-restore

dotnet test test/ast-tests/ast_tests.csproj --no-build
# Optionally all tests
# dotnet test fifthlang.sln --no-build
```

## Screenshots/Logs (optional)

## Breaking Changes (if any)
- Impacted users/scenarios:
- Migration steps:

## Related Issues/Links
- 