# Fifth.Sdk

MSBuild SDK for building Fifth language projects (`.5thproj`).

## Overview

Fifth.Sdk enables Fifth language projects to be seamlessly integrated into .NET solutions using standard MSBuild tooling. It provides the necessary MSBuild targets and properties to compile Fifth source files (`.5th`) into executable or library .NET assemblies with support for project and package references.

## Requirements

- .NET 8.0 SDK or higher
- Fifth compiler available either from the repo build or as a .NET tool

## Usage

### Creating a .5thproj File

Create a new file with the `.5thproj` extension:

```xml
<Project Sdk="Fifth.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>MyFifthApp</AssemblyName>
    
    <!-- Optional: Specify compiler path if not in default location -->
    <FifthCompilerPath>../path/to/compiler.dll</FifthCompilerPath>

    <!-- Optional: Use the compiler as a .NET tool (preferred for distribution) -->
    <FifthCompilerCommand>fifthc</FifthCompilerCommand>
  </PropertyGroup>
</Project>
```

### Source Files

By default, all `.5th` files in the project directory and subdirectories are included in the compilation.

Example `hello.5th`:

```fifth
main(): void {
    std.print("Hello from Fifth!");
}
```

### Building

```bash
dotnet build MyProject.5thproj
```

### Properties

- **FifthCompilerPath** (optional): Full path to the Fifth compiler DLL. If not specified, the SDK will attempt to locate it relative to the SDK installation.
- **FifthCompilerCommand** (optional): Compiler command to invoke (e.g., `fifthc`) when the compiler is installed as a .NET tool.
- **FifthSourceDirectory** (optional): Directory containing Fifth source files. Defaults to the project directory.
- **FifthOutputPath** (optional): Full path to the output artifact. Defaults to `bin\<Configuration>\<TargetFramework>\<AssemblyName>.<ext>`.
- **FifthSupportedTargetFrameworks** (optional): Semicolon-delimited allowlist of supported target frameworks.

### Targets

- **Build**: Compiles Fifth source files
- **Clean**: Removes build outputs
- **Rebuild**: Performs Clean followed by Build

## Development

### Local Testing

For local development and testing, create a `NuGet.Config` in your test project directory:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-sdk" value="../../src/Fifth.Sdk/bin/Debug" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local-sdk">
      <package pattern="Fifth.Sdk" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

And a `global.json`:

```json
{
  "msbuild-sdks": {
    "Fifth.Sdk": "0.1.0"
  }
}
```

### Building the SDK

```bash
dotnet pack src/Fifth.Sdk/Fifth.Sdk.csproj -c Release
```

### Building the Compiler Tool

```bash
dotnet pack src/compiler/compiler.csproj -c Release
```

### Publishing (exact commands)

```bash
dotnet nuget push src/Fifth.Sdk/bin/Release/Fifth.Sdk.<version>.nupkg --api-key <API_KEY> --source <NUGET_SOURCE>
dotnet nuget push src/compiler/bin/Release/Fifth.Compiler.Tool.<version>.nupkg --api-key <API_KEY> --source <NUGET_SOURCE>
```

### Versioning notes

- Use semantic versioning for both packages.
- Keep `Fifth.Sdk` and `Fifth.Compiler.Tool` versions aligned unless there is a clear reason not to.
- If you change the version, update the `Version` properties in:
  - `src/Fifth.Sdk/Fifth.Sdk.csproj`
  - `src/compiler/compiler.csproj`
- Update any `global.json` that pins `Fifth.Sdk` in consumers or samples.

## Integration with .NET Solutions

Fifth projects can be added to .NET solutions alongside C# and F# projects:

```bash
dotnet sln add MyFifthProject.5thproj
```

## Limitations

- Requires the Fifth compiler to be available via `FifthCompilerPath` or `FifthCompilerCommand`
- .NET 8.0+ target framework required

## Future Enhancements

- Support for library projects
- NuGet package distribution of the SDK
- Integration with IDE tooling (syntax highlighting, IntelliSense)
- Support for project-to-project references

## License

See the repository root LICENSE file for license information.
