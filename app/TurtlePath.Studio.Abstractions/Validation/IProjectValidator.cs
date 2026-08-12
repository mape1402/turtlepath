namespace TurtlePath.Studio.Abstractions.Validation;

public interface IProjectValidator
{
    Task<ProjectValidationResult> ValidateAsync(
        ProjectValidationRequest request,
        CancellationToken cancellationToken = default);
}
