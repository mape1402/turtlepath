using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Templates;
using System.Text.Json;

namespace TurtlePath.Studio.Infrastructure.Templates;

public sealed class DotNetTemplatePackageManager(
    ICommandExecutor commandExecutor,
    HttpClient httpClient) : ITemplatePackageManager
{
    private const string NuGetSource = "https://api.nuget.org/v3/index.json";

    public async Task<TemplatePackageInfo> GetInstalledAsync(
        string packageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        var result = await commandExecutor.ExecuteAsync(
            new CommandSpec("dotnet", ["new", "uninstall"], global::System.Environment.CurrentDirectory),
            cancellationToken);

        var latestVersion = await GetLatestVersionAsync(packageId, cancellationToken);

        if (!result.Succeeded)
            return new TemplatePackageInfo(packageId, string.Empty, false, latestVersion);

        var lines = result.Output
            .Where(line => line.Kind == CommandOutputKind.StandardOutput)
            .Select(line => line.Text)
            .ToArray();

        var packageLineIndex = FindPackageLineIndex(lines, packageId);
        var packageLine = packageLineIndex >= 0 ? lines[packageLineIndex] : null;

        return new TemplatePackageInfo(
            packageId,
            ParseVersion(lines, packageLineIndex),
            packageLine is not null,
            latestVersion);
    }

    public async Task<string> GetLatestVersionAsync(
        string packageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        try
        {
            var packageKey = packageId.ToLowerInvariant();
            await using var stream = await httpClient.GetStreamAsync(
                $"https://api.nuget.org/v3-flatcontainer/{packageKey}/index.json",
                cancellationToken);

            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("versions", out var versions))
                return string.Empty;

            return versions.EnumerateArray()
                .Select(version => version.GetString())
                .Where(version => !string.IsNullOrWhiteSpace(version) && !version.Contains('-', StringComparison.Ordinal))
                .LastOrDefault() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public Task<CommandExecutionResult> InstallAsync(
        TemplateInstallRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageId);

        var package = string.IsNullOrWhiteSpace(request.Version)
            ? request.PackageId
            : $"{request.PackageId}@{request.Version}";

        var arguments = new List<string>
        {
            "new",
            "install",
            package,
            "--nuget-source",
            NuGetSource
        };

        if (request.ForceUpdate)
            arguments.Add("--force");

        return commandExecutor.ExecuteAsync(
            new CommandSpec("dotnet", arguments, global::System.Environment.CurrentDirectory),
            cancellationToken);
    }

    private static int FindPackageLineIndex(
        IReadOnlyList<string> lines,
        string packageId)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            var text = lines[index].Trim();
            if (string.Equals(text, packageId, StringComparison.OrdinalIgnoreCase))
                return index;

            if (text.StartsWith($"{packageId} ", StringComparison.OrdinalIgnoreCase))
                return index;
        }

        return -1;
    }

    private static string ParseVersion(
        IReadOnlyList<string> lines,
        int packageLineIndex)
    {
        if (packageLineIndex < 0 || packageLineIndex >= lines.Count)
            return string.Empty;

        for (var index = packageLineIndex + 1; index < lines.Count; index++)
        {
            var text = lines[index].Trim();
            if (text.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))
                return text["Version:".Length..].Trim();

            if (text.Equals("Uninstall Command:", StringComparison.OrdinalIgnoreCase))
                break;
        }

        var parts = lines[packageLineIndex]
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return parts.LastOrDefault(part => char.IsDigit(part[0])) ?? string.Empty;
    }
}
