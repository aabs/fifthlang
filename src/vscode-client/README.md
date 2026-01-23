# Fifth Language Client (VS Code)

This extension connects VS Code to the Fifth Language Server over stdio.

## Prerequisites

- .NET SDK 8.0 (per the repo `global.json`)
- The Fifth Language Server build output

## Setup

1) Build the server:

- `dotnet build fifthlang.sln`

2) Install dependencies for the extension:

- `npm install`

3) Build the extension:

- `npm run compile`

4) Launch the extension (VS Code):

- Open this folder in VS Code
- Press `F5` to start an Extension Development Host
- Use the `Run Fifth Client` launch config (not the plain Node.js debugger)

## Configuration

Configure the server path in VS Code settings:

- `fifthLanguageServer.serverDllPath`: Path to `Fifth.LanguageServer.dll` (workspace-relative or absolute)
- `fifthLanguageServer.dotnetPath`: Path to the `dotnet` executable
- `fifthLanguageServer.args`: Extra arguments for the server

Default server DLL path:

- `../language-server/bin/Debug/net8.0/Fifth.LanguageServer.dll`

The path is resolved relative to the current workspace first, then the extension folder.

## Notes

- The client targets `.5th` files with the `fifth` language id.
- The server runs locally and uses stdio transport.
- Syntax highlighting is provided by a bundled TextMate grammar.
