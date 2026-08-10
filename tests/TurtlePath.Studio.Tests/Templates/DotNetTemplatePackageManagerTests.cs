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
        var manager = new DotNetTemplatePackageManager(executor);

        await manager.InstallAsync(new TemplateInstallRequest("TurtlePath.Template", "1.4.0", ForceUpdate: true));

        var command = Assert.Single(executor.Commands);

        Assert.Equal("dotnet", command.FileName);
        Assert.Equal(["new", "install", "TurtlePath.Template::1.4.0", "--force"], command.Arguments);
    }

    [Fact]
    public async Task GetInstalledAsync_detects_installed_template_package()
    {
        var executor = new RecordingCommandExecutor();
        executor.EnqueueSuccess("TurtlePath.Template 1.4.0");
        var manager = new DotNetTemplatePackageManager(executor);

        var result = await manager.GetInstalledAsync("TurtlePath.Template");

        Assert.True(result.IsInstalled);
        Assert.Equal("1.4.0", result.Version);
    }
}
