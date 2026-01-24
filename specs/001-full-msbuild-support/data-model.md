# Data Model: Full MSBuild Support

## Entity: Project
- **Fields**: `ProjectId`, `Name`, `OutputType`, `TargetFrameworks`, `Sources`, `OutputPath`, `IntermediateOutputPath`, `BuildEventPolicy`
- **Relationships**: has many `ProjectReference`, `PackageReference`, `BuildEvent`, `BuildOutput`
- **Validation rules**: `OutputType` must be Exe or Library; at least one source; target frameworks must be allowlisted

## Entity: ProjectReference
- **Fields**: `ProjectId`, `ReferencedProjectId`, `ReferencePath`
- **Relationships**: belongs to `Project`
- **Validation rules**: referenced project must exist; circular references fail the build

## Entity: PackageReference
- **Fields**: `ProjectId`, `PackageId`, `VersionRange`
- **Relationships**: belongs to `Project`
- **Validation rules**: version range must resolve; version conflicts fail the build

## Entity: BuildOutput
- **Fields**: `ProjectId`, `TargetFramework`, `ArtifactPath`, `OutputType`, `Timestamp`
- **Relationships**: belongs to `Project`
- **Validation rules**: artifact path must be deterministic per project configuration

## Entity: TargetFramework
- **Fields**: `Name`, `Moniker`, `IsSupported`
- **Relationships**: belongs to `Project`
- **Validation rules**: unsupported frameworks must fail the build with a clear error

## Entity: SupportedTargetFramework
- **Fields**: `Moniker`, `IsDefault`, `DeprecationStatus`
- **Relationships**: referenced by `Project.TargetFrameworks`
- **Validation rules**: projects may only target allowlisted frameworks

## Entity: BuildEvent
- **Fields**: `ProjectId`, `Phase` (Pre/Post), `Command`, `Result`
- **Relationships**: belongs to `Project`
- **Validation rules**: failure must fail the build and provide diagnostics
