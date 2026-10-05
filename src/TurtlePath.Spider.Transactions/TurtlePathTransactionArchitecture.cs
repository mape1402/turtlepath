using Spider.Pipelines.Architecture;

namespace TurtlePath.Spider.Transactions;

/// <summary>
/// Provides Spider architecture metadata for TurtlePath transaction boundaries.
/// </summary>
public static class TurtlePathTransactionArchitecture
{
    /// <summary>
    /// Builds the default TurtlePath transaction boundary architecture manifest.
    /// </summary>
    /// <returns>A Spider architecture manifest that describes the transaction boundary.</returns>
    public static SpiderArchitectureManifest BuildManifest()
    {
        var components = new List<SpiderComponentDescriptor>
        {
            new(
                "turtlepath.boundary.transaction",
                "spider.boundary",
                "TurtlePath transaction boundary",
                new Dictionary<string, string>
                {
                    ["description"] = "Wraps TurtlePath command execution in a transaction when the request matches the configured transaction profile.",
                    ["tags"] = "turtlepath,boundary,transaction",
                    ["boundary.type"] = "transaction",
                    ["purpose"] = "Protects persistence changes made by command handlers.",
                    ["observability"] = "spider-runtime-tracing",
                    ["failure.behavior"] = "Rolls back the active transaction when the pipeline faults.",
                }),
            new(
                "turtlepath.profile.boundary",
                "spider.flow-profile",
                "TurtlePath.Boundary",
                new Dictionary<string, string>
                {
                    ["description"] = "Default profile used by TurtlePath infrastructure boundaries.",
                    ["tags"] = "turtlepath,boundary,profile",
                }),
        };

        return new SpiderArchitectureManifest(components, []);
    }
}
