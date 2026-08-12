namespace TurtlePath.Studio.Abstractions.Validation;

public sealed record ProjectValidationResult(
    string ProjectDirectory,
    IReadOnlyList<ProjectValidationStepResult> Steps)
{
    public bool Succeeded => Steps.All(step => step.Execution.Succeeded);
}
