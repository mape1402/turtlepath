using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;

namespace TurtlePath.Studio.App.Updates;

public sealed class StudioUpdater : IStudioUpdater
{
    private const string Platform = "win-x64";
    private const string StudioExecutableName = "TurtlePath.Studio.App.exe";
    private const string UpdaterExecutableName = "TurtlePath.Studio.Updater.exe";
    private const string PowerShellExecutableName = "powershell.exe";

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

        string content;
        try
        {
            content = await DownloadTextWithPowerShellAsync(uri, cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or TaskCanceledException)
        {
            return new StudioUpdateCheckResult(
                IsAvailable: false,
                GetCurrentVersion(),
                LatestVersion: string.Empty,
                $"Update manifest could not be downloaded. {exception.Message}",
                Manifest: null,
                Package: null);
        }

        var trimmedContent = content.TrimStart('\uFEFF').TrimStart();
        if (string.IsNullOrWhiteSpace(trimmedContent) || !trimmedContent.StartsWith('{'))
        {
            return new StudioUpdateCheckResult(
                IsAvailable: false,
                GetCurrentVersion(),
                LatestVersion: string.Empty,
                "Update manifest URL did not return JSON. Check that the URL points to a public direct-download JSON file.",
                Manifest: null,
                Package: null);
        }

        StudioUpdateManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<StudioUpdateManifest>(trimmedContent, JsonOptions);
        }
        catch (JsonException exception)
        {
            return new StudioUpdateCheckResult(
                IsAvailable: false,
                GetCurrentVersion(),
                LatestVersion: string.Empty,
                $"Update manifest JSON is invalid. {exception.Message}",
                Manifest: null,
                Package: null);
        }

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

        if (!Uri.TryCreate(update.Package.Url, UriKind.Absolute, out var packageUri))
            throw new InvalidOperationException("Studio package URL is not valid.");

        await DownloadFileWithPowerShellAsync(packageUri, zipPath, cancellationToken);

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
        var assembly = typeof(StudioUpdater).Assembly;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informationalVersion))
            return NormalizeSemVer(informationalVersion);

        var assemblyVersion = assembly.GetName().Version?.ToString();
        if (!string.IsNullOrWhiteSpace(assemblyVersion))
            return NormalizeSemVer(assemblyVersion);

        return NormalizeSemVer(AppInfo.Current.VersionString);
    }

    public static string NormalizeSemVer(string version)
    {
        var normalized = version.Trim();
        if (normalized.StartsWith('v'))
            normalized = normalized[1..];

        var metadataIndex = normalized.IndexOf('+', StringComparison.Ordinal);
        if (metadataIndex >= 0)
            normalized = normalized[..metadataIndex];

        var prereleaseIndex = normalized.IndexOf('-', StringComparison.Ordinal);
        if (prereleaseIndex >= 0)
            normalized = normalized[..prereleaseIndex];

        if (Version.TryParse(normalized, out var parsed))
            return $"{parsed.Major}.{parsed.Minor}.{Math.Max(parsed.Build, 0)}";

        return normalized;
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

    private static async Task<string> DownloadTextWithPowerShellAsync(
        Uri uri,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, "studio-update-manifest", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        try
        {
            await DownloadFileWithPowerShellAsync(uri, path, cancellationToken);
            return await File.ReadAllTextAsync(path, cancellationToken);
        }
        finally
        {
            TryDelete(path);
        }
    }

    private static async Task DownloadFileWithPowerShellAsync(
        Uri uri,
        string outputPath,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var scriptPath = Path.Combine(
            FileSystem.CacheDirectory,
            "studio-update-scripts",
            $"{Guid.NewGuid():N}.ps1");
        Directory.CreateDirectory(Path.GetDirectoryName(scriptPath)!);

        await File.WriteAllTextAsync(
            scriptPath,
            """
            param(
                [Parameter(Mandatory = $true)]
                [string] $DownloadUri,

                [Parameter(Mandatory = $true)]
                [string] $OutputPath
            )

            $ProgressPreference = 'SilentlyContinue'
            Invoke-WebRequest -Uri $DownloadUri -OutFile $OutputPath -UseBasicParsing
            """,
            cancellationToken);

        PowerShellResult result;
        try
        {
            result = await RunPowerShellAsync(scriptPath, uri.AbsoluteUri, outputPath, cancellationToken);
        }
        finally
        {
            TryDelete(scriptPath);
        }

        if (result.ExitCode != 0)
            throw new InvalidOperationException($"PowerShell download failed. {result.Error.Trim()}");

        if (!File.Exists(outputPath) || new FileInfo(outputPath).Length == 0)
            throw new InvalidOperationException("PowerShell download completed, but no file was created.");
    }

    private static async Task<PowerShellResult> RunPowerShellAsync(
        string scriptPath,
        string uri,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = PowerShellExecutableName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add(uri);
        startInfo.ArgumentList.Add(outputPath);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("PowerShell could not be started.");

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        var error = await errorTask;

        return new PowerShellResult(process.ExitCode, output, error);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best effort cleanup only.
        }
    }

    private sealed record PowerShellResult(int ExitCode, string Output, string Error);
}
