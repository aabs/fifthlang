using FluentAssertions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace fifth_sdk_tests;

public class ReferenceResolutionTests
{
    [Fact]
    public void MissingProjectReferenceFailsBuild()
    {
        var repoRoot = FindRepoRoot();
        var projectDir = CreateTempProjectDirectory();
        var projectPath = Path.Combine(projectDir, "MissingProjectRef.5thproj");
        var missingProjectPath = Path.Combine(projectDir, "Missing.5thproj");

        File.WriteAllText(projectPath, $"""
<Project Sdk=\"Fifth.Sdk\">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=\"{missingProjectPath}\" />
  </ItemGroup>
</Project>
""");

        var result = BuildTarget(repoRoot, projectPath, "ResolveFifthProjectReferences");
        result.OverallResult.Should().Be(BuildResultCode.Failure);
    }

    [Fact]
    public void PackageReferencesRequireAssetsFile()
    {
        var repoRoot = FindRepoRoot();
        var projectDir = CreateTempProjectDirectory();
        var projectPath = Path.Combine(projectDir, "MissingAssets.5thproj");

        File.WriteAllText(projectPath, """
<Project Sdk="Fifth.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
""");

        var result = BuildTarget(repoRoot, projectPath, "ValidatePackageReferences");
        result.OverallResult.Should().Be(BuildResultCode.Failure);
    }

    private static BuildResult BuildTarget(string repoRoot, string projectPath, string target)
    {
        var sdkPath = Path.Combine(repoRoot, "src", "Fifth.Sdk", "Sdk");
        Environment.SetEnvironmentVariable("MSBuildSDKsPath", sdkPath);

        var globalProperties = new Dictionary<string, string>
        {
            ["TargetFramework"] = "net8.0",
            ["DesignTimeBuild"] = "false"
        };

        using var projectCollection = new ProjectCollection(globalProperties);
        var buildParameters = new BuildParameters(projectCollection)
        {
            Loggers = Array.Empty<ILogger>()
        };
        var requestData = new BuildRequestData(projectPath, globalProperties, null, new[] { target }, null);

        return BuildManager.DefaultBuildManager.Build(buildParameters, requestData);
    }

    private static string CreateTempProjectDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "fifth-sdk-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "fifthlang.sln")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root not found.");
    }
}
