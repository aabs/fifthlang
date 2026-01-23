# Fifth LSP in VS Code Insiders (Local)

This guide explains how to run the Fifth Language Server locally and connect it to VS Code Insiders over stdio.

## Prerequisites

- .NET SDK 8.0 (per repo `global.json`)
- VS Code Insiders
- A generic LSP client extension that supports **stdio** and custom language mappings

## Use the Fifth VS Code client (recommended)

This repo includes a dedicated VS Code client extension at [src/vscode-client](src/vscode-client). It wires up the Fifth language id and launches the server over stdio.

1) Install dependencies:

- `cd src/vscode-client`
- `npm install`

2) Build the extension:

- `npm run compile`

3) Launch the extension:

- Open `src/vscode-client` in VS Code
- Press `F5` to start an Extension Development Host

4) Confirm settings (optional):

- `fifthLanguageServer.serverDllPath` (default: `src/language-server/bin/Debug/net8.0/Fifth.LanguageServer.dll`)
- `fifthLanguageServer.dotnetPath` (default: `dotnet`)
- `fifthLanguageServer.args` (default: empty)

## Build the server

From the repo root:

1) Build the solution:

- `dotnet build fifthlang.sln`

2) Locate the server:

- Built DLL: `src/language-server/bin/Debug/net8.0/Fifth.LanguageServer.dll`

You can also run directly without prebuilding:

- `dotnet run --project src/language-server/Fifth.LanguageServer.csproj`

## Configure VS Code Insiders

### 1) Associate `.5th` files with a language id

Open Settings (JSON) and add:

```json
"files.associations": {
  "*.5th": "fifth"
}
```

### 2) Configure your LSP client extension

In Settings (JSON), add a client definition matching your extension’s schema. Use stdio and the `fifth` language id.

Example (adjust keys to match your LSP client extension):

```json
"lsp.servers": {
  "fifth": {
    "command": "dotnet",
    "args": [
      "src/language-server/bin/Debug/net8.0/Fifth.LanguageServer.dll"
    ],
    "languages": ["fifth"]
  }
}
```

If your extension requires a working directory, set it to the repo root.

## Try it out

1) Open a `.5th` file.
2) Type a syntax error and verify diagnostics appear.
3) Hover a symbol to see details.
4) Use “Go to Definition” on a symbol declared in another workspace file.
5) Trigger completion to see keywords and symbols.

## Notes

- The server is **local-only** and uses **stdio**.
- Diagnostics are computed for **open documents only**.
- Go-to-definition searches across **all workspace files**, including unopened files.

## Troubleshooting

- If the server won’t start, run it manually in a terminal to inspect stderr output.
- If no features appear, confirm the LSP client is actually launching the server and that `.5th` files are associated with the `fifth` language id.
- If completions/definitions are empty, ensure the workspace root is the repo root and the files are within that workspace.
