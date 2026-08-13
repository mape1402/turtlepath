using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;

namespace TurtlePath.Studio.App.Updates;

public sealed class StudioUpdater(HttpClient httpClient) : IStudioUpdater
{
    private const string Platform = "win-x64";
    private const string StudioExecutableName = "TurtlePath.Studio.App.exe";
    private const string UpdaterExecutableName = "TurtlePath.Studio.Updater.exe";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<StudioUpdateCheckResult> CheckForUpdatesAsync(
        string manifestUrl,
        string channel,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(manifestUrl) || !Uri.TryCreate(manifestUrl, UriKind.Absolute, out var uri))
        {
            return new StudioUpdateCheckResult(
                IsAvailable: false,
                GetCurrentVersion(),
                LatestVersion: string.Empty,
                "Update manifest URL is not valid.",
                Manifest: null,
                Package: null);
        }

        await using var stream = await httpClient.GetStreamAsync(uri, cancellationToken);
        var manifest = await JsonSerializer.DeserializeAsync<StudioUpdateManifest>(stream, JsonOptions, cancellationToken);
        if (manifest is null)
        {
            return new StudioUpdateCheckResult(
                IsAvailable: false,
                GetCurrentVersion(),
                LatestVersion: string.Empty,
                "Update manifest could not be read.",
                Manifest: null,
                Package: null);
        }

        if (!string.IsNullOrWhiteSpace(channel) &&
            !string.Equals(manifest.Channel, channel, StringComparison.OrdinalIgnoreCase))
        {
            return new StudioUpdateCheckResult(
                IsAvailable: false,
                GetCurrentVersion(),
                manifest.LatestVersion,
                $"Manifest channel '{manifest.Channel}' does not match configured channel '{channel}'.",
                manifest,
                Package: null);
        }

        var package = manifest.Packages.FirstOrDefault(candidate =>
            string.Equals(candidate.Platform, Platform, StringComparison.OrdinalIgnoreCase));
        if (package is null)
        {
            return new StudioUpdateCheckResult(
                IsAvailable: false,
                GetCurrentVersion(),
                manifest.LatestVersion,
                $"No Studio package is available for {Platform}.",
                manifest,
                Package: null);
        }

        var currentVersion = GetCurrentVersion();
        var isAvailable = CompareVersions(manifest.LatestVersion, currentVersion) > 0;

        return new StudioUpdateCheckResult(
            isAvailable,
            currentVersion,
            manifest.LatestVersion,
            isAvailable
                ? $"Studio {manifest.LatestVersion} is available. Current version: {currentVersion}."
                : $"Studio is current. Installed version: {currentVersion}.",
            manifest,
            package);
    }

    public async Task StartUpdateAsync(StudioUpdateCheckResult update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!update.IsAvailable || update.Package is null)
            throw new InvalidOperationException("No Studio update is available.");

        var workingDirectory = Path.Combine(FileSystem.CacheDirectory, "studio-update", Guid.NewGuid().ToString("N"));
        var zipPath = Path.Combine(workingDirectory, "studio.zip");
        var extractDirectory = Path.Combine(workingDirectory, "extract");
        Directory.CreateDirectory(workingDirectory);

        await using (var stream = await httpClient.GetStreamAsync(update.Package.Url, cancellationToken))
        await using (var file = File.Create(zipPath))
        {
            await stream.CopyToAsync(file, cancellationToken);
        }

        var hash = await ComputeSha256Async(zipPath, cancellationToken);
        if (!string.Equals(hash, update.Package.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Downloaded Studio package hash does not match the manifest.");

        ZipFile.ExtractToDirectory(zipPath, extractDirectory, overwriteFiles: true);

        var currentDirectory = AppContext.BaseDirectory;
        var updaterPath = FindUpdaterPath(currentDirectory);
        var updaterCopyPath = Path.Combine(workingDirectory, UpdaterExecutableName);
        File.Copy(updaterPath, updaterCopyPath, overwrite: true);

        var launchPath = Path.Combine(currentDirectory, StudioExecutableName);
        Process.Start(new ProcessStartInfo
        {
            FileName = updaterCopyPath,
            UseShellExecute = true,
            Arguments = $"--source {Quote(extractDirectory)} --target {Quote(currentDirectory)} --pid {Environment.ProcessId} --launch {Quote(launchPath)}"
        });

        Microsoft.Maui.Controls.Application.Current?.Quit();
    }

    public static string GetCurrentVersion()
    {
        return NormalizeSemVer(AppInfo.Current.VersionString);
    }

    public static string NormalizeSemVer(string version)
    {
        if (Version.TryParse(version, out var parsed))
            return $"{parsed.Major}.{parsed.Minor}.{Math.Max(parsed.Build, 0)}";

        var normalized = version.Trim();
        return normalized.StartsWith('v') ? normalized[1..] : normalized;
    }

    private static int CompareVersions(string left, string right)
    {
        var normalizedLeft = NormalizeSemVer(left);
        var normalizedRight = NormalizeSemVer(right);

        if (Version.TryParse(normalizedLeft, out var leftVersion) &&
            Version.TryParse(normalizedRight, out var rightVersion))
        {
            return leftVersion.CompareTo(rightVersion);
        }

        return string.Compare(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string FindUpdaterPath(string baseDirectory)
    {
        var candidates = new[]
        {
            Path.Combine(baseDirectory, "updater", UpdaterExecutableName),
            Path.Combine(baseDirectory, UpdaterExecutableName)
        };

        return candidates.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("Studio updater executable was not found.", UpdaterExecutableName);
    }

    private static string Quote(string value) => $"\"{value}\"";
}
