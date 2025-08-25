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
