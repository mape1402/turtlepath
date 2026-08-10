using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Validation;

namespace TurtlePath.Studio.Infrastructure.Validation;

public sealed class DotNetProjectValidator(ICommandExecutor commandExecutor) : IProjectValidator
{
    public async Task<ProjectValidationResult> ValidateAsync(
        ProjectValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectDirectory);

        var steps = new List<ProjectValidationStepResult>();

        if (request.Restore)
            steps.Add(await ExecuteAsync(ProjectValidationStep.Restore, ["restore"], request.ProjectDirectory, cancellationToken));

        if (request.Build)
            steps.Add(await ExecuteAsync(ProjectValidationStep.Build, ["build", "--configuration", "Release", "--no-restore"], request.ProjectDirectory, cancellationToken));

        if (request.Test)
            steps.Add(await ExecuteAsync(ProjectValidationStep.Test, ["test", "--configuration", "Release", "--no-build"], request.ProjectDirectory, cancellationToken));

        return new ProjectValidationResult(request.ProjectDirectory, steps);
    }

    private async Task<ProjectValidationStepResult> ExecuteAsync(
        ProjectValidationStep step,
        IReadOnlyList<string> arguments,
        string projectDirectory,
        CancellationToken cancellationToken)
    {
        var result = await commandExecutor.ExecuteAsync(
            new CommandSpec("dotnet", arguments, projectDirectory),
            cancellationToken);

        return new ProjectValidationStepResult(step, result);
    }
}
