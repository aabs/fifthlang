# Debugging Workflows

This note captures the supported ways to capture short-lived diagnostics without leaving temporary projects or binaries in the repository.

## Inspecting Emitted IL

1. Build a program with diagnostics enabled:
   ```fish
   dotnet run --project src/compiler/compiler.csproj -- --command build --source path/to/program.5th --output /tmp/program.exe --keep-temp --diagnostics
   ```
2. The compiler writes IL snapshots under `build_debug_il/` while `--keep-temp` is set. Inspect the generated `.il` file immediately, then delete the folder before committing any changes:
   ```fish
   cat build_debug_il/.il
   rm -rf build_debug_il
   ```
3. For deeper inspection use the .NET tools that ship with the SDK (for example `dotnet ildasm`) against the emitted PE file.

## Parsing or Lowering Dumps

For ad-hoc AST or lowering dumps, prefer running the existing test or CLI harnesses and piping their output to `/tmp` locations. Avoid checking in custom projects under `scripts/` for one-off analysis. If a utility becomes part of the permanent workflow, promote it into `src/tools/` (or the relevant project), add documentation here, and ensure it has automated tests.

## Temporary Files

Keep scratch `.5th` files under `/tmp` or another ignored directory. The repository `.gitignore` already blocks `tmp_*.5th`, `tmp_*.exe`, and the `tmp_*/` directory pattern; do not force-add items matching these globs. Remove `KEEP_FIFTH_TEMP` markers and other flags that keep temporary test directories once debugging wraps up.
