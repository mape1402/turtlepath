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
    public void Main_project_does_not_reference_infrastructure_packages()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath", "TurtlePath.csproj"));

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", project);
        Assert.DoesNotContain("OctoMap", project);
        Assert.DoesNotContain("Crabalidator", project);
        Assert.DoesNotContain("Sieve", project);
    }

    [Fact]
    public void Abstractions_project_does_not_reference_infrastructure_packages()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Abstractions", "TurtlePath.Abstractions.csproj"));

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", project);
        Assert.DoesNotContain("OctoMap", project);
        Assert.DoesNotContain("Crabalidator", project);
        Assert.DoesNotContain("Sieve", project);
    }

    [Fact]
    public void Mapping_validation_and_persistence_contracts_share_the_abstractions_package()
    {
        var root = FindRepositoryRoot();
        var applicationProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath", "TurtlePath.csproj"));
        var octoMapProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.OctoMap", "TurtlePath.OctoMap.csproj"));
        var crabalidatorProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Crabalidator", "TurtlePath.Crabalidator.csproj"));
        var sieveProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Sieve", "TurtlePath.Sieve.csproj"));

        Assert.Contains("TurtlePath.Abstractions", applicationProject);
        Assert.Contains("TurtlePath.Abstractions", octoMapProject);
        Assert.Contains("TurtlePath.Abstractions", crabalidatorProject);
        Assert.Contains("TurtlePath.Abstractions", sieveProject);
        Assert.DoesNotContain("..\\TurtlePath\\TurtlePath.csproj", octoMapProject);
        Assert.DoesNotContain("..\\TurtlePath\\TurtlePath.csproj", crabalidatorProject);
    }

    [Fact]
    public void Domain_project_owns_contracts_and_identifiers_without_entity_framework_references()
    {
        var root = FindRepositoryRoot();
        var domainProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Domain", "TurtlePath.Domain.csproj"));
        var identifier = Path.Combine(root, "src", "TurtlePath.Domain", "Identifier", "CId.cs");
        var entity = Path.Combine(root, "src", "TurtlePath.Domain", "Contracts", "IEntity.cs");
        var baseEntity = Path.Combine(root, "src", "TurtlePath.Domain", "Contracts", "BaseEntity.cs");

        Assert.True(File.Exists(identifier));
        Assert.True(File.Exists(entity));
        Assert.True(File.Exists(baseEntity));
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", domainProject);
    }

    [Theory]
    [InlineData("TurtlePath.AspNetCore")]
    [InlineData("TurtlePath.Application")]
    [InlineData("TurtlePath.Identifier")]
    [InlineData("TurtlePath.Domain.Identifier")]
    [InlineData("TurtlePath.Identifier.EntityFrameworkCore")]
    [InlineData("TurtlePath.Domain.Identifier.EntityFrameworkCore")]
    [InlineData("TurtlePath.Serialization")]
    [InlineData("TurtlePath.Swagger")]
    public void Removed_packages_do_not_exist(string projectName)
    {
        var root = FindRepositoryRoot();

        Assert.False(Directory.Exists(Path.Combine(root, "src", projectName)));
    }

    [Theory]
    [InlineData("Core")]
    [InlineData("Services")]
    [InlineData("Infrastructure")]
    public void Source_projects_do_not_use_generic_bucket_folders(string folderName)
    {
        var root = FindRepositoryRoot();
        var src = Path.Combine(root, "src");

        var matches = Directory
            .EnumerateDirectories(src, folderName, SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                           !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        Assert.Empty(matches);
    }

    [Fact]
    public void Entity_framework_project_owns_entity_framework_references()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.EntityFrameworkCore", "TurtlePath.EntityFrameworkCore.csproj"));
        var baseDbContext = Path.Combine(root, "src", "TurtlePath.EntityFrameworkCore", "BaseDbContext.cs");

        Assert.True(File.Exists(baseDbContext));
        Assert.Contains("Microsoft.EntityFrameworkCore", project);
        Assert.DoesNotContain("OctoMap", project);
        Assert.DoesNotContain("Crabalidator", project);
        Assert.DoesNotContain("Sieve", project);
    }

    [Fact]
    public void Adapter_projects_own_mapping_and_validation_references()
    {
        var root = FindRepositoryRoot();
        var octoMapProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.OctoMap", "TurtlePath.OctoMap.csproj"));
        var crabalidatorProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Crabalidator", "TurtlePath.Crabalidator.csproj"));

        Assert.Contains("OctoMap", octoMapProject);
        Assert.Contains("Crabalidator", crabalidatorProject);
    }

    [Fact]
    public void Sieve_project_owns_sieve_reference()
    {
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Sieve", "TurtlePath.Sieve.csproj"));

        Assert.Contains("Sieve", project);
    }

    [Fact]
    public void Domain_identifier_folder_owns_identifier_json_converters_without_openapi_references()
    {
        var root = FindRepositoryRoot();
        var domainProject = File.ReadAllText(Path.Combine(root, "src", "TurtlePath.Domain", "TurtlePath.Domain.csproj"));
        var converter = Path.Combine(root, "src", "TurtlePath.Domain", "Identifier", "Json", "CIdJsonConverter.cs");
        var nullableConverter = Path.Combine(root, "src", "TurtlePath.Domain", "Identifier", "Json", "CIdNullableJsonConverter.cs");

        Assert.True(File.Exists(converter));
        Assert.True(File.Exists(nullableConverter));
        Assert.DoesNotContain("Swashbuckle", domainProject);
        Assert.DoesNotContain("Microsoft.AspNetCore.App", domainProject);
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


