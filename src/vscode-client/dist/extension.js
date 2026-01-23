"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.activate = activate;
exports.deactivate = deactivate;
const fs = __importStar(require("fs"));
const path = __importStar(require("path"));
const vscode = __importStar(require("vscode"));
const node_1 = require("vscode-languageclient/node");
let client;
function activate(context) {
    const outputChannel = vscode.window.createOutputChannel("Fifth Language Server");
    const config = vscode.workspace.getConfiguration("fifthLanguageServer");
    const dotnetPath = config.get("dotnetPath", "dotnet");
    const serverDllPath = config.get("serverDllPath", "src/language-server/bin/Debug/net8.0/Fifth.LanguageServer.dll");
    const extraArgs = config.get("args", []);
    const resolvedServerPath = resolveServerPath(serverDllPath, context.extensionPath);
    const cwd = getWorkspaceFolder();
    outputChannel.appendLine(`dotnetPath=${dotnetPath}`);
    outputChannel.appendLine(`serverDllPath=${serverDllPath}`);
    outputChannel.appendLine(`resolvedServerPath=${resolvedServerPath}`);
    outputChannel.appendLine(`cwd=${cwd ?? "(none)"}`);
    if (!fs.existsSync(resolvedServerPath)) {
        outputChannel.appendLine("server dll not found at resolved path");
    }
    const serverOptions = {
        command: dotnetPath,
        args: [resolvedServerPath, ...extraArgs],
        options: cwd ? { cwd } : undefined
    };
    const clientOptions = {
        documentSelector: [{ language: "fifth" }],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher("**/*.5th")
        },
        outputChannel
    };
    const languageClient = new node_1.LanguageClient("fifthLanguageServer", "Fifth Language Server", serverOptions, clientOptions);
    client = languageClient;
    context.subscriptions.push(languageClient, outputChannel);
    void languageClient
        .start()
        .then(() => outputChannel.appendLine("language client started"))
        .catch(err => outputChannel.appendLine(`language client start failed: ${String(err)}`));
}
async function deactivate() {
    if (client) {
        await client.stop();
    }
}
function resolveServerPath(serverDllPath, extensionPath) {
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
function getWorkspaceFolder() {
    return vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
}
//# sourceMappingURL=extension.js.map