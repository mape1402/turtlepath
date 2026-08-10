using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;

namespace TurtlePath.Studio.Application.UseCases;

public sealed class CreateTurtlePathProjectUseCase(
    IProjectGenerator projectGenerator,
    IProjectValidator projectValidator)
{
    public async Task<CreateTurtlePathProjectUseCaseResult> ExecuteAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var creation = await projectGenerator.CreateAsync(request, cancellationToken);
        ProjectValidationResult validation = null;

        if (request.RestoreAfterCreation || request.BuildAfterCreation || request.TestAfterCreation)
        {
            validation = await projectValidator.ValidateAsync(
                new ProjectValidationRequest(
                    creation.ProjectDirectory,
                    request.RestoreAfterCreation,
                    request.BuildAfterCreation,
                    request.TestAfterCreation),
                cancellationToken);
        }

        return new CreateTurtlePathProjectUseCaseResult(creation, validation);
    }
}
