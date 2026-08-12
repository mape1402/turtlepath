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
}
