using TurtlePath.Commands;

namespace TurtlePath.Tests;

public sealed class TurtlePathCommandArchitectureTests
{
    [Fact]
    public void BuildManifest_includes_default_command_flows()
    {
        var manifest = TurtlePathCommandArchitecture.BuildManifest();

        Assert.Contains(manifest.Components, component => component.Id == "turtlepath.command.create" && component.Kind == "spider.flow");
        Assert.Contains(manifest.Components, component => component.Id == "turtlepath.command.update" && component.Kind == "spider.flow");
        Assert.Contains(manifest.Components, component => component.Id == "turtlepath.command.patch" && component.Kind == "spider.flow");
        Assert.Contains(manifest.Components, component => component.Id == "turtlepath.command.delete" && component.Kind == "spider.flow");
        Assert.Contains(manifest.Components, component => component.Id == "turtlepath.command.create.validate" && component.Kind == "spider.flow-step");
        Assert.Contains(manifest.Relations, relation => relation.SourceId == "turtlepath.command.create" && relation.TargetId == "turtlepath.profile.command" && relation.Kind == "uses-profile");
    }
}
