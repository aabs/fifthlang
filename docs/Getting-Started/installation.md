# Installation

Fifth provides pre-built, self-contained binaries for Linux, macOS, and Windows. No additional .NET runtime is required to run the compiler.

## Supported Platforms

| Platform | Architectures | Runtimes |
| --- | --- | --- |
| Linux | x64, arm64 | net8.0, net10.0 |
| macOS | x64, arm64 | net8.0, net10.0 |
| Windows | x64, arm64 | net8.0, net10.0 |

The `net8.0` packages are the supported baseline. The `net10.0` builds rely on preview SDKs and are provided for early adopters to validate future runtime behavior.

---

## Download

Download the latest release from the [GitHub Releases page](https://github.com/aabs/fifthlang/releases).

Archives follow the naming pattern:
```
fifth-<version>-<runtime>-<framework>.<tar.gz|zip>
```

For example: `fifth-0.9.0-linux-x64-net8.0.tar.gz`

---

## Verify Your Download

Each release includes a `SHA256SUMS` file for checksum verification.

### Linux (GNU coreutils)

```bash
VERSION=0.9.0
RUNTIME=linux-x64
FRAMEWORK=net8.0

curl -LO "https://github.com/aabs/fifthlang/releases/download/v${VERSION}/fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.tar.gz"
curl -LO "https://github.com/aabs/fifthlang/releases/download/v${VERSION}/SHA256SUMS"

sha256sum --ignore-missing -c SHA256SUMS
```

### macOS (BSD shasum)

```bash
VERSION=0.9.0
RUNTIME=osx-x64
FRAMEWORK=net8.0

curl -LO "https://github.com/aabs/fifthlang/releases/download/v${VERSION}/fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.tar.gz"
curl -LO "https://github.com/aabs/fifthlang/releases/download/v${VERSION}/SHA256SUMS"

grep "fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.tar.gz" SHA256SUMS | shasum -a 256 -c -
```

### Windows (PowerShell)

```powershell
$version = "0.9.0"
$runtime = "win-x64"
$framework = "net8.0"

Invoke-WebRequest -Uri "https://github.com/aabs/fifthlang/releases/download/v$version/fifth-$version-$runtime-$framework.zip" -OutFile "fifth-$version-$runtime-$framework.zip"
Invoke-WebRequest -Uri "https://github.com/aabs/fifthlang/releases/download/v$version/SHA256SUMS" -OutFile "SHA256SUMS"

# Compare the hashes
Get-FileHash ".\fifth-$version-$runtime-$framework.zip" -Algorithm SHA256
Get-Content .\SHA256SUMS | Select-String "fifth-$version-$runtime-$framework.zip"
```

Only proceed when the computed hash matches the line from `SHA256SUMS`.

---

## Install

### Linux / macOS

```bash
VERSION=0.9.0
RUNTIME=linux-x64  # or osx-x64, osx-arm64, linux-arm64
FRAMEWORK=net8.0

# Create installation directory
mkdir -p ~/opt/fifth/${VERSION}

# Extract
tar -xzf fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.tar.gz -C ~/opt/fifth/${VERSION}

# Add to PATH (add this to your ~/.bashrc, ~/.zshrc, or ~/.config/fish/config.fish)
export PATH=~/opt/fifth/${VERSION}/fifth-${VERSION}/bin:$PATH
```

### Windows

```powershell
$version = "0.9.0"
$runtime = "win-x64"  # or win-arm64
$framework = "net8.0"

# Create installation directory
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\fifth\$version" | Out-Null

# Extract
Expand-Archive -Force -Path ".\fifth-$version-$runtime-$framework.zip" -DestinationPath "$env:USERPROFILE\fifth\$version"
```

Add `%USERPROFILE%\fifth\<version>\fifth-<version>\bin` to your user PATH via **System Properties > Environment Variables**, or use:

```powershell
$newPath = "$env:USERPROFILE\fifth\$version\fifth-$version\bin"
[Environment]::SetEnvironmentVariable("PATH", "$env:PATH;$newPath", "User")
```

---

## Archive Contents

Each archive contains:

```
fifth-<version>/
  bin/
    fifth (or fifth.exe on Windows)
  lib/
    *.dll support libraries for IDE tooling and the SDK
  LICENSE
  README.md
  VERSION.txt
```

The `bin/fifth` binary is the Fifth compiler. The `lib/` folder contains support libraries for IDE tooling and SDK integration.

---

## Verify Installation

After installation, verify the compiler is working:

```bash
fifth --version
fifth --help
```

---

## Build from Source

If you prefer to build from source or need to work on the compiler itself:

### Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- Java 17+ (for ANTLR grammar compilation)

### Build Steps

```bash
git clone https://github.com/aabs/fifthlang.git
cd fifthlang
dotnet restore fifthlang.sln
dotnet build fifthlang.sln
```

The build may take 1-2 minutes on first run. See [AGENTS.md](https://github.com/aabs/fifthlang/blob/master/AGENTS.md) for detailed development instructions.

---

## Troubleshooting

### "command not found" after installation

Ensure the `bin` directory is in your PATH and restart your terminal session.

### Permission denied (Linux/macOS)

Make the binary executable:

```bash
chmod +x ~/opt/fifth/${VERSION}/fifth-${VERSION}/bin/fifth
```

### Windows Defender SmartScreen warning

The binaries are not code-signed. Click "More info" then "Run anyway" if you trust the download (verify the checksum first).

---

## Next Steps

- [Learn Fifth in Y Minutes](learn5thInYMinutes.md) — Quick language tour
- [Knowledge Graphs Guide](knowledge-graphs.md) — RDF/SPARQL features
- [Example Programs](examples/) — Real Fifth code
