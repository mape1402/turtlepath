using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;

namespace TurtlePath.Studio.Application.UseCases;

public sealed record CreateTurtlePathProjectUseCaseResult(
    CreateProjectResult Creation,
    ProjectValidationResult Validation)
{
    public bool Succeeded => Creation.Generation.Succeeded && (Validation?.Succeeded ?? true);
}
