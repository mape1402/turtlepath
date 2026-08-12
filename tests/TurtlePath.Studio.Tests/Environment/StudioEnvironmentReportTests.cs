using TurtlePath.Studio.Abstractions.Environment;
using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Application.Environment;

namespace TurtlePath.Studio.Tests.Environment;

public sealed class StudioEnvironmentReportTests
{
    [Fact]
    public void CanCreateProjects_when_template_is_installed_even_if_update_exists()
    {
        var report = new StudioEnvironmentReport(
            CreateDotNetEnvironment(),
            new TemplatePackageInfo("TurtlePath.Template", "1.5.1", true, "1.6.0"));

        Assert.True(report.CanCreateProjects);
        Assert.True(report.TemplateRequiresUpdate);
    }

    [Fact]
    public void CanCreateProjects_when_latest_version_cannot_be_verified()
    {
        var report = new StudioEnvironmentReport(
            CreateDotNetEnvironment(),
            new TemplatePackageInfo("TurtlePath.Template", "1.6.0", true));

        Assert.True(report.CanCreateProjects);
        Assert.False(report.TemplateRequiresUpdate);
    }

    private static DotNetEnvironmentInfo CreateDotNetEnvironment()
        => new(true, "10.0.400", [new DotNetSdkInfo("10.0.400", "dotnet")], "dotnet", string.Empty);
}
