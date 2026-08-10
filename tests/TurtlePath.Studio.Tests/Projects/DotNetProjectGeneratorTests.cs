using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Infrastructure.Projects;
using TurtlePath.Studio.Tests.Fakes;

namespace TurtlePath.Studio.Tests.Projects;

public class DotNetProjectGeneratorTests
{
    [Theory]
    [InlineData(ProjectHostMode.ApiConsumer, "api-consumer")]
    [InlineData(ProjectHostMode.Job, "job")]
    public async Task CreateAsync_builds_expected_turtlepath_template_command(
        ProjectHostMode hostMode,
        string expectedHost)
    {
        var executor = new RecordingCommandExecutor();
        var generator = new DotNetProjectGenerator(executor);
        var output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        await generator.CreateAsync(new CreateProjectRequest(
            "Billing",
            output,
            hostMode,
            RestoreAfterCreation: false,
            BuildAfterCreation: false,
            TestAfterCreation: false));

        var command = Assert.Single(executor.Commands);

        Assert.Equal("dotnet", command.FileName);
        Assert.Equal(
            ["new", "turtlepath", "-n", "Billing", "-o", Path.GetFullPath(output), "--host", expectedHost],
            command.Arguments);
        Assert.Equal(Path.GetFullPath(output), command.WorkingDirectory);
    }
}
