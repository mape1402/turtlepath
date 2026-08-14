using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace TurtlePath.Studio.App.Updates;

/// <summary>
/// Updates Studio directly from its NuGet package. NuGet is the only update source;
/// no remote manifest or release-specific URL is required.
/// </summary>
public sealed class StudioUpdater(HttpClient httpClient) : IStudioUpdater
{
    public const string PackageId = "TurtlePath.Studio";
    private const string Platform = "win-x64";
    private const string StudioExecutableName = "TurtlePath.Studio.App.exe";
    private const string UpdaterExecutableName = "TurtlePath.Studio.Updater.exe";
    private const string NuGetIndexUrl = "https://api.nuget.org/v3-flatcontainer/turtlepath.studio/index.json";

    public async Task<StudioUpdateCheckResult> CheckForUpdatesAsync(
        string packageId,
        string channel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var index = await httpClient.GetFromJsonAsync<NuGetVersionIndex>(NuGetIndexUrl, cancellationToken);
            var latestVersion = index?.Versions?
                .Where(version => Version.TryParse(version, out _))
                .OrderByDescending(version => Version.Parse(version))
                .FirstOrDefault();

            if (latestVersion is null)
                return Failure("NuGet did not return a published Studio version.");

            var packageUrl = $"https://api.nuget.org/v3-flatcontainer/{PackageId.ToLowerInvariant()}/{latestVersion.ToLowerInvariant()}/{PackageId.ToLowerInvariant()}.{latestVersion.ToLowerInvariant()}.nupkg";
            var currentVersion = GetCurrentVersion();
            var isAvailable = CompareVersions(latestVersion, currentVersion) > 0;
            var package = new StudioPackage(Platform, packageUrl, latestVersion);

            return new StudioUpdateCheckResult(
                isAvailable,
                currentVersion,
                latestVersion,
                isAvailable
                    ? $"Studio {latestVersion} is available. Current version: {currentVersion}."
                    : $"Studio is current. Installed version: {currentVersion}.",
                package);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            return Failure($"Studio updates could not be checked through NuGet. {exception.Message}");
        }
    }

    public async Task StartUpdateAsync(StudioUpdateCheckResult update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!update.IsAvailable || update.Package is null)
            throw new InvalidOperationException("No Studio update is available.");

        var workingDirectory = Path.Combine(FileSystem.CacheDirectory, "studio-update", Guid.NewGuid().ToString("N"));
        var packagePath = Path.Combine(workingDirectory, "studio.nupkg");
        var packageExtractDirectory = Path.Combine(workingDirectory, "package");
        var payloadDirectory = Path.Combine(packageExtractDirectory, "tools", Platform);
        Directory.CreateDirectory(workingDirectory);

        try
        {
            await DownloadAsync(update.Package.Url, packagePath, cancellationToken);
            ZipFile.ExtractToDirectory(packagePath, packageExtractDirectory, overwriteFiles: true);

            if (!Directory.Exists(payloadDirectory) ||
                !File.Exists(Path.Combine(payloadDirectory, StudioExecutableName)))
            {
                throw new InvalidOperationException("The Studio NuGet package does not contain a valid Windows payload.");
            }

            var currentDirectory = AppContext.BaseDirectory;
            var updaterPath = FindUpdaterPath(currentDirectory);
            var updaterCopyPath = Path.Combine(workingDirectory, UpdaterExecutableName);
            File.Copy(updaterPath, updaterCopyPath, overwrite: true);
            var launchPath = Path.Combine(currentDirectory, StudioExecutableName);

            Process.Start(new ProcessStartInfo
            {
                FileName = updaterCopyPath,
                UseShellExecute = true,
                Arguments = $"--source {Quote(payloadDirectory)} --target {Quote(currentDirectory)} --pid {Environment.ProcessId} --launch {Quote(launchPath)}"
            });

            Microsoft.Maui.Controls.Application.Current?.Quit();
        }
        catch
        {
            TryDeleteDirectory(workingDirectory);
            throw;
        }
    }

    public static string GetCurrentVersion()
    {
        var assembly = typeof(StudioUpdater).Assembly;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return NormalizeSemVer(informationalVersion ?? assembly.GetName().Version?.ToString() ?? AppInfo.Current.VersionString);
    }

    public static string NormalizeSemVer(string version)
    {
        var normalized = version.Trim().TrimStart('v');
        var metadataIndex = normalized.IndexOf('+', StringComparison.Ordinal);
        if (metadataIndex >= 0)
            normalized = normalized[..metadataIndex];

        var prereleaseIndex = normalized.IndexOf('-', StringComparison.Ordinal);
        if (prereleaseIndex >= 0)
            normalized = normalized[..prereleaseIndex];

        return Version.TryParse(normalized, out var parsed)
            ? $"{parsed.Major}.{parsed.Minor}.{Math.Max(parsed.Build, 0)}"
            : normalized;
    }

    private static StudioUpdateCheckResult Failure(string message) => new(
        false,
        GetCurrentVersion(),
        string.Empty,
        message,
        null);

    private static int CompareVersions(string left, string right)
    {
        return Version.TryParse(NormalizeSemVer(left), out var leftVersion) &&
               Version.TryParse(NormalizeSemVer(right), out var rightVersion)
            ? leftVersion.CompareTo(rightVersion)
            : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private async Task DownloadAsync(string url, string destinationPath, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken);
    }

    private static string FindUpdaterPath(string baseDirectory) =>
        new[]
        {
            Path.Combine(baseDirectory, "updater", UpdaterExecutableName),
            Path.Combine(baseDirectory, UpdaterExecutableName)
        }.FirstOrDefault(File.Exists)
        ?? throw new FileNotFoundException("Studio updater executable was not found.", UpdaterExecutableName);

    private static string Quote(string value) => $"\"{value}\"";

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Best effort cleanup only.
        }
    }

    private sealed record NuGetVersionIndex(IReadOnlyList<string> Versions);
}
