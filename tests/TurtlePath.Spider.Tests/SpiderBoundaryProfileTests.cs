using TurtlePath.Spider;

namespace TurtlePath.Spider.Tests;

public sealed class SpiderBoundaryProfileTests
{
    [Fact]
    public void Profile_configures_typed_options()
    {
        var options = new BoundaryTestOptions();
        ISpiderBoundaryProfile<BoundaryTestOptions> profile = new BoundaryTestProfile();

        profile.Configure(options);

        Assert.True(options.Enabled);
    }

    private sealed class BoundaryTestProfile : SpiderBoundaryProfile<BoundaryTestOptions>
    {
        public override void Configure(BoundaryTestOptions options)
        {
            options.Enabled = true;
        }
    }

    private sealed class BoundaryTestOptions
    {
        public bool Enabled { get; set; }
    }
}
