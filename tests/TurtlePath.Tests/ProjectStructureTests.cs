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

    [Fact]
    public void Domain_project_does_not_reference_application_or_infrastructure_packages()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Domain", "TurtlePath.Domain.csproj"));

        Assert.DoesNotContain("Pelican.Mediator", project);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", project);
        Assert.DoesNotContain("OctoMap", project);
        Assert.DoesNotContain("Crabalidator", project);
        Assert.DoesNotContain("Sieve", project);
    }

    [Fact]
    public void Application_project_does_not_reference_infrastructure_packages()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Application", "TurtlePath.Application.csproj"));

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", project);
        Assert.DoesNotContain("OctoMap", project);
        Assert.DoesNotContain("Crabalidator", project);
        Assert.DoesNotContain("Sieve", project);
    }

    [Fact]
    public void Persistence_abstractions_project_does_not_reference_entity_framework()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Persistence.Abstractions", "TurtlePath.Persistence.Abstractions.csproj"));

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", project);
        Assert.DoesNotContain("Sieve", project);
    }

    [Fact]
    public void Entity_framework_project_owns_entity_framework_references()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.EntityFrameworkCore", "TurtlePath.EntityFrameworkCore.csproj"));

        Assert.Contains("Microsoft.EntityFrameworkCore", project);
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
