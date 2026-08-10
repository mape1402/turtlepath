namespace TurtlePath.Studio.Abstractions.Templates;

public sealed record TemplatePackageInfo(
    string PackageId,
    string Version,
    bool IsInstalled);
