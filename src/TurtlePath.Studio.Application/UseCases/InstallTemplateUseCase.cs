using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Application.Defaults;

namespace TurtlePath.Studio.Application.UseCases;

public sealed class InstallTemplateUseCase(ITemplatePackageManager templatePackageManager)
{
    public Task<CommandExecutionResult> ExecuteAsync(
        string version = null,
        bool forceUpdate = false,
        CancellationToken cancellationToken = default)
    {
        var request = new TemplateInstallRequest(
            TurtlePathStudioDefaults.TemplatePackageId,
            version,
            forceUpdate);

        return templatePackageManager.InstallAsync(request, cancellationToken);
    }
}
