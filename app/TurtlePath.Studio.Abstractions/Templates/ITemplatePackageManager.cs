using TurtlePath.Studio.Abstractions.Commands;

namespace TurtlePath.Studio.Abstractions.Templates;

public interface ITemplatePackageManager
{
    Task<TemplatePackageInfo> GetInstalledAsync(
        string packageId,
        CancellationToken cancellationToken = default);

    Task<string> GetLatestVersionAsync(
        string packageId,
        CancellationToken cancellationToken = default);

    Task<CommandExecutionResult> InstallAsync(
        TemplateInstallRequest request,
        CancellationToken cancellationToken = default);
}
