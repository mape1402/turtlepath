namespace TurtlePath.Tests;

public class ProjectStructureTests
{
    [Theory]
    [InlineData("README.md")]
    [InlineData("CHANGELOG.md")]
    [InlineData("Directory.Build.props")]
    [InlineData(".github/workflows/CI.yml")]
    [InlineData(".github/workflows/release.yml")]
    public void Repository_contains_expected_solution_items(string path)
    {
        var root = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(root, path)), $"{path} should exist.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TurtlePath.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root could not be found.");
    }
}
