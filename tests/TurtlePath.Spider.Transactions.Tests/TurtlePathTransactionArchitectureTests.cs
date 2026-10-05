using TurtlePath.Spider.Transactions;

namespace TurtlePath.Spider.Transactions.Tests;

public sealed class TurtlePathTransactionArchitectureTests
{
    [Fact]
    public void BuildManifest_includes_transaction_boundary_metadata()
    {
        var manifest = TurtlePathTransactionArchitecture.BuildManifest();

        var boundary = Assert.Single(manifest.Components, component => component.Id == "turtlepath.boundary.transaction");

        Assert.Equal("spider.boundary", boundary.Kind);
        Assert.Equal("TurtlePath transaction boundary", boundary.DisplayName);
        Assert.Equal("transaction", boundary.Metadata["boundary.type"]);
        Assert.Contains("spider-runtime-tracing", boundary.Metadata["observability"]);
    }
}
