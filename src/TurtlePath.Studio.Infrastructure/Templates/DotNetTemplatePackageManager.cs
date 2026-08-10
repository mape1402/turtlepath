using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Templates;

namespace TurtlePath.Studio.Infrastructure.Templates;

public sealed class DotNetTemplatePackageManager(ICommandExecutor commandExecutor) : ITemplatePackageManager
{
    public async Task<TemplatePackageInfo> GetInstalledAsync(
        string packageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        var result = await commandExecutor.ExecuteAsync(
            new CommandSpec("dotnet", ["new", "uninstall"], global::System.Environment.CurrentDirectory),
            cancellationToken);

        if (!result.Succeeded)
            return new TemplatePackageInfo(packageId, string.Empty, false);

        var packageLine = result.Output
            .Where(line => line.Kind == CommandOutputKind.StandardOutput)
            .Select(line => line.Text)
            .FirstOrDefault(text => text.Contains(packageId, StringComparison.OrdinalIgnoreCase));

        return new TemplatePackageInfo(
            packageId,
            ParseVersion(packageLine),
            packageLine is not null);
    }

    public Task<CommandExecutionResult> InstallAsync(
        TemplateInstallRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageId);

        var package = string.IsNullOrWhiteSpace(request.Version)
            ? request.PackageId
            : $"{request.PackageId}::{request.Version}";

        var arguments = new List<string> { "new", "install", package };

        if (request.ForceUpdate)
            arguments.Add("--force");

        return commandExecutor.ExecuteAsync(
            new CommandSpec("dotnet", arguments, global::System.Environment.CurrentDirectory),
            cancellationToken);
    }

    private static string ParseVersion(string packageLine)
    {
        if (string.IsNullOrWhiteSpace(packageLine))
            return string.Empty;

        var parts = packageLine
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return parts.LastOrDefault(part => char.IsDigit(part[0])) ?? string.Empty;
    }
}
