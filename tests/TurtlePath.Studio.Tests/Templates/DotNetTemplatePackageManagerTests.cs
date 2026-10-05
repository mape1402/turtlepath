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
    public async Task InstallAsync_uses_package_id_without_optional_flags()
    {
        var executor = new RecordingCommandExecutor();
        using var httpClient = new HttpClient();
        var manager = new DotNetTemplatePackageManager(executor, httpClient);

        await manager.InstallAsync(new TemplateInstallRequest("TurtlePath.Template"));

        var command = Assert.Single(executor.Commands);

        Assert.Equal(
            ["new", "install", "TurtlePath.Template", "--nuget-source", "https://api.nuget.org/v3/index.json"],
            command.Arguments);
        await Assert.ThrowsAsync<ArgumentNullException>(() => manager.InstallAsync(null));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.InstallAsync(new TemplateInstallRequest(" ")));
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

    [Fact]
    public async Task GetInstalledAsync_handles_uninstalled_and_inline_version_outputs()
    {
        var executor = new RecordingCommandExecutor();
        executor.EnqueueSuccess("TurtlePath.Template 1.5.0", "Uninstall Command:");
        executor.EnqueueFailure("dotnet failed");
        using var httpClient = new HttpClient(new StaticJsonHandler("{\"versions\":[\"1.0.0\",\"1.1.0-preview.1\",\"1.2.0\"]}"));
        var manager = new DotNetTemplatePackageManager(executor, httpClient);

        var installed = await manager.GetInstalledAsync("TurtlePath.Template");
        var missing = await manager.GetInstalledAsync("TurtlePath.Template");

        Assert.True(installed.IsInstalled);
        Assert.Equal("1.5.0", installed.Version);
        Assert.Equal("1.2.0", installed.LatestVersion);
        Assert.False(missing.IsInstalled);
        Assert.Equal(string.Empty, missing.Version);
        await Assert.ThrowsAsync<ArgumentException>(() => manager.GetInstalledAsync(" "));
    }

    [Fact]
    public async Task GetLatestVersionAsync_returns_empty_when_feed_is_unavailable_or_malformed()
    {
        var unavailable = new DotNetTemplatePackageManager(
            new RecordingCommandExecutor(),
            new HttpClient(new ThrowingHandler()));
        var malformed = new DotNetTemplatePackageManager(
            new RecordingCommandExecutor(),
            new HttpClient(new StaticJsonHandler("{}")));

        Assert.Equal(string.Empty, await unavailable.GetLatestVersionAsync("TurtlePath.Template"));
        Assert.Equal(string.Empty, await malformed.GetLatestVersionAsync("TurtlePath.Template"));
        await Assert.ThrowsAsync<ArgumentException>(() => unavailable.GetLatestVersionAsync(""));
    }

    private sealed class StaticJsonHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("feed unavailable");
    }
}
