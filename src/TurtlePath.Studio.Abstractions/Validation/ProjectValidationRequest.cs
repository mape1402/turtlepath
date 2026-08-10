namespace TurtlePath.Studio.Abstractions.Validation;

public sealed record ProjectValidationRequest(
    string ProjectDirectory,
    bool Restore = true,
    bool Build = true,
    bool Test = true);
