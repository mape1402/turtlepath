using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Infrastructure.Templates;
using TurtlePath.Studio.Tests.Fakes;

namespace TurtlePath.Studio.Tests.Templates;

public class DotNetTemplatePackageManagerTests
{
    [Fact]
    public async Task InstallAsync_uses_package_version_when_it_is_provided()
    {
        var executor = new RecordingCommandExecutor();
        using var httpClient = new HttpClient();
        var manager = new DotNetTemplatePackageManager(executor, httpClient);

        await manager.InstallAsync(new TemplateInstallRequest("TurtlePath.Template", "1.4.0", ForceUpdate: true));

        var command = Assert.Single(executor.Commands);

        Assert.Equal("dotnet", command.FileName);
        Assert.Equal(
            ["new", "install", "TurtlePath.Template@1.4.0", "--nuget-source", "https://api.nuget.org/v3/index.json", "--force"],
            command.Arguments);
    }

    [Fact]
    public async Task GetInstalledAsync_detects_installed_template_package()
    {
        var executor = new RecordingCommandExecutor();
        executor.EnqueueSuccess(
            "Currently installed items:",
            "   TurtlePath.Template",
            "      Version: 1.4.0",
            "      Templates:",
            "         TurtlePath Service (turtlepath) C#");
        using var httpClient = new HttpClient();
        var manager = new DotNetTemplatePackageManager(executor, httpClient);

        var result = await manager.GetInstalledAsync("TurtlePath.Template");

        Assert.True(result.IsInstalled);
        Assert.Equal("1.4.0", result.Version);
    }

    [Fact]
    public async Task GetInstalledAsync_matches_exact_package_when_package_names_overlap()
    {
        var executor = new RecordingCommandExecutor();
        executor.EnqueueSuccess(
            "Currently installed items:",
            "   TurtlePath.Template.HeroesShowcase",
            "      Version: 1.4.3",
            "      Templates:",
            "         TurtlePath Heroes Showcase (turtlepath-heroes-showcase) C#",
            string.Empty,
            "   TurtlePath.Template",
            "      Version: 1.4.4",
            "      Templates:",
            "         TurtlePath Service (turtlepath) C#");
        using var httpClient = new HttpClient();
        var manager = new DotNetTemplatePackageManager(executor, httpClient);

        var result = await manager.GetInstalledAsync("TurtlePath.Template");

        Assert.True(result.IsInstalled);
        Assert.Equal("1.4.4", result.Version);
    }
}
