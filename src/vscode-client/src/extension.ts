import * as fs from "fs";
import * as path from "path";
import * as vscode from "vscode";
import { LanguageClient, LanguageClientOptions, ServerOptions } from "vscode-languageclient/node";

let client: LanguageClient | undefined;

export function activate(context: vscode.ExtensionContext): void {
    const outputChannel = vscode.window.createOutputChannel("Fifth Language Server");
    const config = vscode.workspace.getConfiguration("fifthLanguageServer");
    const dotnetPath = config.get<string>("dotnetPath", "dotnet");
    const serverDllPath = config.get<string>(
        "serverDllPath",
        "src/language-server/bin/Debug/net8.0/Fifth.LanguageServer.dll"
    );
    const extraArgs = config.get<string[]>("args", []);

    const resolvedServerPath = resolveServerPath(serverDllPath, context.extensionPath);
    const cwd = getWorkspaceFolder();

    outputChannel.appendLine(`dotnetPath=${dotnetPath}`);
    outputChannel.appendLine(`serverDllPath=${serverDllPath}`);
    outputChannel.appendLine(`resolvedServerPath=${resolvedServerPath}`);
    outputChannel.appendLine(`cwd=${cwd ?? "(none)"}`);
    if (!fs.existsSync(resolvedServerPath)) {
        outputChannel.appendLine("server dll not found at resolved path");
    }

    const serverOptions: ServerOptions = {
        command: dotnetPath,
        args: [resolvedServerPath, ...extraArgs],
        options: cwd ? { cwd } : undefined
    };

    const clientOptions: LanguageClientOptions = {
        documentSelector: [{ language: "fifth" }],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher("**/*.5th")
        },
        outputChannel
    };

    const languageClient = new LanguageClient(
        "fifthLanguageServer",
        "Fifth Language Server",
        serverOptions,
        clientOptions
    );

    client = languageClient;

    context.subscriptions.push(languageClient, outputChannel);
    void languageClient
        .start()
        .then(() => outputChannel.appendLine("language client started"))
        .catch(err => outputChannel.appendLine(`language client start failed: ${String(err)}`));
}

export async function deactivate(): Promise<void> {
    if (client) {
        await client.stop();
    }
}

function resolveServerPath(serverDllPath: string, extensionPath: string): string {
    if (path.isAbsolute(serverDllPath)) {
        return serverDllPath;
    }

    const workspaceFolder = getWorkspaceFolder();
    const workspaceCandidate = workspaceFolder
        ? path.resolve(workspaceFolder, serverDllPath)
        : undefined;

    if (workspaceCandidate && fs.existsSync(workspaceCandidate)) {
        return workspaceCandidate;
    }

    const extensionCandidate = path.resolve(extensionPath, serverDllPath);
    if (fs.existsSync(extensionCandidate)) {
        return extensionCandidate;
    }

    return workspaceCandidate ?? extensionCandidate;
}

function getWorkspaceFolder(): string | undefined {
    return vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
}
