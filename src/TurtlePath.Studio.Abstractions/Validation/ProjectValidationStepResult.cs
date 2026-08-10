using TurtlePath.Studio.Abstractions.Commands;

namespace TurtlePath.Studio.Abstractions.Validation;

public sealed record ProjectValidationStepResult(
    ProjectValidationStep Step,
    CommandExecutionResult Execution);
