using TurtlePath.Studio.Application.Defaults;

namespace TurtlePath.Studio.Tests.Defaults;

public sealed class TurtlePathStudioDefaultsTests
{
    [Fact]
    public void Defaults_expose_template_package_ids_and_short_names()
    {
        Assert.Equal("TurtlePath.Template", TurtlePathStudioDefaults.TemplatePackageId);
        Assert.Equal("turtlepath", TurtlePathStudioDefaults.TemplateShortName);
        Assert.Equal("TurtlePath.Template.HeroesShowcase", TurtlePathStudioDefaults.HeroesShowcaseTemplatePackageId);
        Assert.Equal("turtlepath-heroes-showcase", TurtlePathStudioDefaults.HeroesShowcaseTemplateShortName);
        Assert.Equal(
            [TurtlePathStudioDefaults.TemplatePackageId, TurtlePathStudioDefaults.HeroesShowcaseTemplatePackageId],
            TurtlePathStudioDefaults.TemplatePackageIds);
    }
}
