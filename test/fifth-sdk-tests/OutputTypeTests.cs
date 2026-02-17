using FluentAssertions;
using Microsoft.Build.Evaluation;

namespace fifth_sdk_tests;

public class OutputTypeTests
{
    [Fact]
    public void ExeOutputTypeSetsTargetExtAndPath()
    {
        var (project, collection) = LoadProjectWithOutputType("Exe");
        try
        {
            project.GetPropertyValue("TargetExt").Should().Be(".exe");
            project.GetPropertyValue("TargetPath").Should().EndWith(".exe");
            project.GetPropertyValue("FifthOutputPath").Should().Be(project.GetPropertyValue("TargetPath"));
        }
        finally
        {
            collection.Dispose();
        }
    }

    [Fact]
    public void LibraryOutputTypeSetsTargetExtAndPath()
    {
        var (project, collection) = LoadProjectWithOutputType("Library");
        try
        {
            project.GetPropertyValue("TargetExt").Should().Be(".dll");
            project.GetPropertyValue("TargetPath").Should().EndWith(".dll");
            project.GetPropertyValue("FifthOutputPath").Should().Be(project.GetPropertyValue("TargetPath"));
        }
        finally
        {
            collection.Dispose();
        }
    }

    private static (Project Project, ProjectCollection Collection) LoadProjectWithOutputType(string outputType)
    {
        var repoRoot = FindRepoRoot();
        var sdkPath = Path.Combine(repoRoot, "src", "Fifth.Sdk", "Sdk");
        Environment.SetEnvironmentVariable("MSBuildSDKsPath", sdkPath);

        var projectPath = Path.Combine(repoRoot, "test", "fifth-sdk-tests", "HelloFifth.5thproj");
        var globalProperties = new Dictionary<string, string>
        {
            ["OutputType"] = outputType,
            ["TargetFramework"] = "net8.0"
        };

        var collection = new ProjectCollection(globalProperties);
        var project = collection.LoadProject(projectPath);
        return (project, collection);
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
