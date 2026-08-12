namespace TurtlePath.Studio.Abstractions.Templates;

public sealed record TemplateInstallRequest(
    string PackageId,
    string Version = null,
    bool ForceUpdate = false);
