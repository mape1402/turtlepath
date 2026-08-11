using TurtlePath.Studio.Abstractions.Environment;
using TurtlePath.Studio.Abstractions.Templates;

namespace TurtlePath.Studio.Application.Environment;

public sealed record StudioEnvironmentReport(
    DotNetEnvironmentInfo DotNet,
    TemplatePackageInfo Template)
{
    public bool CanCreateProjects => DotNet.IsAvailable && Template.IsInstalled && Template.IsLatest;

    public bool TemplateRequiresInstall => !Template.IsInstalled;

    public bool TemplateRequiresUpdate => Template.IsInstalled && !Template.IsLatest;
}
