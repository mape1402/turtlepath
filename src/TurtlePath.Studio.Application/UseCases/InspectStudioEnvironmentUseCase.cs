using TurtlePath.Studio.Abstractions.Environment;
using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Application.Defaults;
using TurtlePath.Studio.Application.Environment;

namespace TurtlePath.Studio.Application.UseCases;

public sealed class InspectStudioEnvironmentUseCase(
    IDotNetEnvironmentReader dotNetEnvironmentReader,
    ITemplatePackageManager templatePackageManager)
{
    public async Task<StudioEnvironmentReport> ExecuteAsync(
        string templatePackageId = null,
        CancellationToken cancellationToken = default)
    {
        var dotNet = await dotNetEnvironmentReader.ReadAsync(cancellationToken);
        var template = await templatePackageManager.GetInstalledAsync(
            string.IsNullOrWhiteSpace(templatePackageId) ? TurtlePathStudioDefaults.TemplatePackageId : templatePackageId,
            cancellationToken);

        return new StudioEnvironmentReport(dotNet, template);
    }
}
