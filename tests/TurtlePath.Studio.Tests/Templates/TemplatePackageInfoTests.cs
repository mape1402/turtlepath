using TurtlePath.Studio.Abstractions.Templates;

namespace TurtlePath.Studio.Tests.Templates;

public sealed class TemplatePackageInfoTests
{
    [Theory]
    [InlineData("1.6.0", "1.6.0")]
    [InlineData("v1.6.0", "1.6.0")]
    [InlineData("1.6.0+build.42", "1.6.0")]
    public void IsLatest_normalizes_common_version_shapes(string installed, string latest)
    {
        var info = new TemplatePackageInfo("TurtlePath.Template", installed, true, latest);

        Assert.True(info.IsLatest);
        Assert.False(info.IsOutdated);
    }

    [Fact]
    public void Version_status_handles_missing_and_outdated_versions()
    {
        var missing = new TemplatePackageInfo("TurtlePath.Template", " ", true);
        var blankInstalled = new TemplatePackageInfo("TurtlePath.Template", " ", true, "1.0.0");
        var outdated = new TemplatePackageInfo("TurtlePath.Template", "1.0.0", true, "1.1.0");
        var notInstalled = new TemplatePackageInfo("TurtlePath.Template", "1.0.0", false, "1.1.0");

        Assert.Equal("TurtlePath.Template", missing.PackageId);
        Assert.False(missing.HasLatestVersion);
        Assert.False(blankInstalled.IsLatest);
        Assert.False(missing.IsLatest);
        Assert.True(outdated.HasLatestVersion);
        Assert.True(outdated.IsOutdated);
        Assert.False(notInstalled.IsOutdated);
    }
}
