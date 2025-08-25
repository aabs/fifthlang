# VS Code Dev Kit: Running TUnit Tests

This repo uses TUnit for tests. The C# Dev Kit Testing UI can discover and run these tests with a small setup.

## One-time setup
- Install extensions:
  - C# Dev Kit (`ms-dotnettools.csdevkit`)
  - Dotnet Test Explorer (`formulahendry.dotnet-test-explorer`)
- The workspace already includes helpful settings in `.vscode/settings.json`:
  - `dotnet.defaultSolution`: `fifthlang.sln`
  - `dotnet-test-explorer.useDotnetTestDiscover`: true
  - `dotnet-test-explorer.useVsCodeTestApi`: true
  - `dotnet-test-explorer.testProjectPath`: `test/**/*.csproj`
  - `dotnet-test-explorer.testArguments`: `--no-build --nologo --logger trx`

## Enable Dev Kit's Testing Platform
- VS Code Settings → search for "Use Testing Platform Protocol"
- Enable: `C# Dev Kit › Testing: Use Testing Platform Protocol`
- Reload the window

## Refresh discovery
- Build the solution once, then open the Testing panel and click Refresh
- If empty, try:
  - Command Palette → Developer: Reload Window
  - Command Palette → Test: Clear All Test Results
  - Command Palette → C# Dev Kit: Restart Language Server

## CLI verification (optional)
```fish
# From the repo root
 dotnet build fifthlang.sln
 dotnet test fifthlang.sln --list-tests
```

If tests still do not appear in the Testing panel, ensure the above extensions are enabled and up to date. TUnit is fully supported via Dev Kit's Testing Platform Protocol; Dotnet Test Explorer bridges results into VS Code's unified Testing UI when needed.

## Coverage (optional)
- The repo includes `fifth.runsettings` so both CLI/Dev Kit runs can emit Cobertura coverage files.
- Quick commands:
```fish
# Generate TRX + Cobertura
make coverage
# Build an HTML report at ./CoverageReport
make coverage-report
```
- These commands align with CI, which uses the same runsettings for consistent coverage and reporting.

## Match CI (Release)
If you want to mirror CI locally, use Release configuration, the shared runsettings, and enable coverage collection:
```fish
dotnet build fifthlang.sln -c Release
dotnet test fifthlang.sln -c Release --no-build --logger "trx;LogFileName=results.trx" --collect "XPlat Code Coverage" --settings fifth.runsettings
```

## Troubleshooting
- Invalid TargetPath: This typically means tests were invoked with `--no-build` before the binaries existed. Fix by building once:
  ```fish
  dotnet build fifthlang.sln  # or: dotnet build -c Release
  ```
  Then re-run tests. The workspace uses `--no-build` for speed after the first build.
- No tests in Testing panel: Build the solution, enable Dev Kit’s “Use Testing Platform Protocol”, then Refresh. If still empty, try: Reload Window, Clear All Test Results, and Restart Language Server.
- Java/ANTLR build errors: The parser requires Java 17+. Verify with `java -version` and install Java 17 if missing.
- Coverage files missing: Ensure runs use `--settings fifth.runsettings`. For a quick check, run `make coverage` and look for `coverage.cobertura.xml` under each test project’s `TestResults` folder.
  - TRX files are usually written under each test project’s `TestResults/` by default; when using `--results-directory TestResults` they land in a root `./TestResults/` folder instead. CI uses a root `TestResults` directory for easier artifact upload.
