# Publish Fifth compiler tool and SDK

Short guide for publishing the Fifth compiler as a .NET tool and the MSBuild SDK package.

## Prerequisites

- .NET SDK 8.0.x (per global.json)
- NuGet API key and target feed

## Versioning notes

- Use semantic versioning for both packages.
- Keep `Fifth.Sdk` and `Fifth.Compiler.Tool` versions aligned unless there is a clear reason not to.
- If you change the version, update:
  - `src/Fifth.Sdk/Fifth.Sdk.csproj` (`Version`)
  - `src/compiler/compiler.csproj` (`Version`)
  - any sample `global.json` that pins the SDK

## Pack

```bash
dotnet pack src/Fifth.Sdk/Fifth.Sdk.csproj -c Release
dotnet pack src/compiler/compiler.csproj -c Release
```

Artifacts:
- `src/Fifth.Sdk/bin/Release/Fifth.Sdk.<version>.nupkg`
- `src/compiler/bin/Release/Fifth.Compiler.Tool.<version>.nupkg`

## Publish

```bash
dotnet nuget push src/Fifth.Sdk/bin/Release/Fifth.Sdk.<version>.nupkg --api-key <API_KEY> --source <NUGET_SOURCE>
dotnet nuget push src/compiler/bin/Release/Fifth.Compiler.Tool.<version>.nupkg --api-key <API_KEY> --source <NUGET_SOURCE>
```

## Consumer setup

Install the tool:

```bash
dotnet tool install -g Fifth.Compiler.Tool --version <version🕙 13:50:32 ❯ dotnet tool install -g Fifth.Compiler.Tool
Tool 'fifth.compiler.tool' failed to update due to the following:
The settings file in the tool's NuGet package is invalid: Command 'fifthc' uses unsupported runner ''."
Tool 'fifth.compiler.tool' failed to install. Contact the tool author for assistance.🕙 13:50:32 ❯ dotnet tool install -g Fifth.Compiler.Tool
Tool 'fifth.compiler.tool' failed to update due to the following:
The settings file in the tool's NuGet package is invalid: Command 'fifthc' uses unsupported runner ''."
Tool 'fifth.compiler.tool' failed to install. Contact the tool author for assistance.>
```

Pin the SDK in `global.json`:

```json
{
  "msbuild-sdks": {
    "Fifth.Sdk": "<version>"
  }
}
```

Use the tool in a `.5thproj`:

```xml
<Project Sdk="Fifth.Sdk">
  <PropertyGroup>
    <FifthCompilerCommand>fifthc</FifthCompilerCommand>
  </PropertyGroup>
</Project>
```
