using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Infrastructure.Validation;
using TurtlePath.Studio.Tests.Fakes;

namespace TurtlePath.Studio.Tests.Validation;

public class DotNetProjectValidatorTests
{
    [Fact]
    public async Task ValidateAsync_runs_restore_build_and_test_in_order()
    {
        var executor = new RecordingCommandExecutor();
        var validator = new DotNetProjectValidator(executor);
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var result = await validator.ValidateAsync(new ProjectValidationRequest(directory));

        Assert.True(result.Succeeded);
        Assert.Equal(
            [ProjectValidationStep.Restore, ProjectValidationStep.Build, ProjectValidationStep.Test],
            result.Steps.Select(step => step.Step));
        Assert.Equal(["restore"], executor.Commands[0].Arguments);
        Assert.Equal(["build", "--configuration", "Release", "--no-restore"], executor.Commands[1].Arguments);
        Assert.Equal(["test", "--configuration", "Release", "--no-build"], executor.Commands[2].Arguments);
    }

    [Fact]
    public async Task ValidateAsync_can_skip_steps()
    {
        var executor = new RecordingCommandExecutor();
        var validator = new DotNetProjectValidator(executor);

        var result = await validator.ValidateAsync(new ProjectValidationRequest(
            Path.GetTempPath(),
            Restore: false,
            Build: true,
            Test: false));

        var step = Assert.Single(result.Steps);

        Assert.Equal(ProjectValidationStep.Build, step.Step);
        Assert.Equal(["build", "--configuration", "Release", "--no-restore"], executor.Commands.Single().Arguments);
    }
}
