# Fifth.Sdk

MSBuild SDK for building Fifth language projects (`.5thproj`).

## Overview

Fifth.Sdk enables Fifth language projects to be seamlessly integrated into .NET solutions using standard MSBuild tooling. It provides the necessary MSBuild targets and properties to compile Fifth source files (`.5th`) into executable .NET assemblies.

## Requirements

- .NET 8.0 SDK or higher
- Fifth compiler built in the repository

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
- **FifthSourceDirectory** (optional): Directory containing Fifth source files. Defaults to the project directory.
- **FifthOutputPath** (optional): Full path to the output executable. Defaults to `bin\<Configuration>\<TargetFramework>\<AssemblyName>.exe`.

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
dotnet pack src/Fifth.Sdk/Fifth.Sdk.csproj --configuration Debug
```

## Integration with .NET Solutions

Fifth projects can be added to .NET solutions alongside C# and F# projects:

```bash
dotnet sln add MyFifthProject.5thproj
```

## Limitations

- Currently only supports executable projects (`OutputType=Exe`)
- Requires the Fifth compiler to be pre-built
- .NET 8.0+ target framework required

## Future Enhancements

- Support for library projects
- NuGet package distribution of the SDK
- Integration with IDE tooling (syntax highlighting, IntelliSense)
- Support for project-to-project references

## License

See the repository root LICENSE file for license information.
