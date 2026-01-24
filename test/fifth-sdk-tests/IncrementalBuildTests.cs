using FluentAssertions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace fifth_sdk_tests;

public class IncrementalBuildTests
{
    [Fact]
    public void UnsupportedTargetFrameworkFails()
    {
        var repoRoot = FindRepoRoot();
        var projectDir = CreateTempProjectDirectory();
        var projectPath = Path.Combine(projectDir, "UnsupportedTfm.5thproj");

        File.WriteAllText(projectPath, """
<Project Sdk="Fifth.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>
""");

        var result = BuildTarget(repoRoot, projectPath, "ValidateFifthTargetFrameworks");
        result.OverallResult.Should().Be(BuildResultCode.Failure);
    }

    [Fact]
    public void DesignTimeManifestIsEmitted()
    {
        var repoRoot = FindRepoRoot();
        var projectDir = CreateTempProjectDirectory();
        var projectPath = Path.Combine(projectDir, "DesignTime.5thproj");

        File.WriteAllText(projectPath, """
<Project Sdk="Fifth.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <FifthSource Include="hello.5th" />
  </ItemGroup>
</Project>
""");

        File.WriteAllText(Path.Combine(projectDir, "hello.5th"), "main() => std.print(1);");

        var result = BuildTarget(repoRoot, projectPath, "EmitFifthDesignTimeManifest", designTimeBuild: true);
        result.OverallResult.Should().Be(BuildResultCode.Success);

        var manifestPath = Path.Combine(projectDir, "obj", "fifth_designtime.manifest");
        File.Exists(manifestPath).Should().BeTrue();
        var contents = File.ReadAllText(manifestPath);
        contents.Should().Contain("TargetFramework=");
        contents.Should().Contain("Define=");
        contents.Should().Contain("Source=");
    }

    private static BuildResult BuildTarget(string repoRoot, string projectPath, string target, bool designTimeBuild = false)
    {
        var sdkPath = Path.Combine(repoRoot, "src", "Fifth.Sdk", "Sdk");
        Environment.SetEnvironmentVariable("MSBuildSDKsPath", sdkPath);

        var globalProperties = new Dictionary<string, string>
        {
            ["TargetFramework"] = "net8.0",
            ["DesignTimeBuild"] = designTimeBuild ? "true" : "false"
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
