using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Application.Defaults;

namespace TurtlePath.Studio.Application.UseCases;

public sealed class InstallTemplateUseCase(ITemplatePackageManager templatePackageManager)
{
    public Task<CommandExecutionResult> ExecuteAsync(
        string packageId = null,
        string version = null,
        bool forceUpdate = false,
        CancellationToken cancellationToken = default)
    {
        var request = new TemplateInstallRequest(
            string.IsNullOrWhiteSpace(packageId) ? TurtlePathStudioDefaults.TemplatePackageId : packageId,
            version,
            forceUpdate);

        return templatePackageManager.InstallAsync(request, cancellationToken);
    }
}
