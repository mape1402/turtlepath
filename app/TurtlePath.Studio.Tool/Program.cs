using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

try
{
    var options = StudioInstallOptions.Parse(args);
    if (options.ShowHelp)
    {
        PrintHelp();
        return 0;
    }

    if (!OperatingSystem.IsWindows())
        Console.WriteLine("Warning: TurtlePath Studio is currently published as a Windows x64 application.");

    var installer = new StudioInstaller();
    await installer.InstallAsync(options);
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine(
        """
        TurtlePath Studio Tool

        Usage:
          turtlepath-studio install [options]

        Options:
          --version <tag>     GitHub release tag to download. Default: studio-v1.0.0
          --repo <owner/name> GitHub repository. Default: mape1402/turtlepath
          --asset <name>      Release asset name. Default: TurtlePath.Studio.win-x64.zip
          --output <path>     Install directory. Default: %LOCALAPPDATA%\TurtlePath\Studio
          --force             Replace the existing install directory.
          --launch            Launch Studio after installation.
          -h|--help           Show help.
        """);
}

internal sealed record StudioInstallOptions(
    string Repository,
    string ReleaseTag,
    string AssetName,
    string OutputDirectory,
    bool Force,
    bool Launch,
    bool ShowHelp)
{
    public static StudioInstallOptions Parse(string[] args)
    {
        var repository = StudioToolDefaults.Repository;
        var releaseTag = StudioToolDefaults.ReleaseTag;
        var assetName = StudioToolDefaults.AssetName;
        var outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TurtlePath",
            "Studio");
        var force = false;
        var launch = false;

        var index = 0;
        if (args.Length > 0 && string.Equals(args[0], "install", StringComparison.OrdinalIgnoreCase))
            index = 1;

        while (index < args.Length)
        {
            var arg = args[index];
            switch (arg)
            {
                case "-h":
                case "--help":
                    return new StudioInstallOptions(repository, releaseTag, assetName, outputDirectory, force, launch, true);

                case "--repo":
                    repository = ReadValue(args, ref index, arg);
                    break;

                case "--version":
                    releaseTag = ReadValue(args, ref index, arg);
                    break;

                case "--asset":
                    assetName = ReadValue(args, ref index, arg);
                    break;

                case "--output":
                    outputDirectory = Path.GetFullPath(ReadValue(args, ref index, arg));
                    break;

                case "--force":
                    force = true;
                    break;

                case "--launch":
                    launch = true;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown option '{arg}'. Use --help for usage.");
            }

            index++;
        }

        return new StudioInstallOptions(repository, releaseTag, assetName, outputDirectory, force, launch, false);
    }

    private static string ReadValue(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
            throw new InvalidOperationException($"{option} requires a value.");

        index++;
        return args[index];
    }
}

internal sealed class StudioInstaller
{
    public async Task InstallAsync(StudioInstallOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Repository) || !options.Repository.Contains("/", StringComparison.Ordinal))
            throw new InvalidOperationException("Repository must use the 'owner/name' format.");

        if (Directory.Exists(options.OutputDirectory))
        {
            if (!options.Force)
                throw new InvalidOperationException($"Install directory already exists: {options.OutputDirectory}. Use --force to replace it.");

            Directory.Delete(options.OutputDirectory, recursive: true);
        }

        Directory.CreateDirectory(options.OutputDirectory);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TurtlePath-Studio-Tool", "1.0"));

        var release = await GetReleaseAsync(httpClient, options.Repository, options.ReleaseTag);
        var asset = release.Assets.FirstOrDefault(item => string.Equals(item.Name, options.AssetName, StringComparison.OrdinalIgnoreCase));
        if (asset is null)
            throw new InvalidOperationException($"Release '{options.ReleaseTag}' does not contain asset '{options.AssetName}'.");

        var archivePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-{options.AssetName}");
        try
        {
            await DownloadAsync(httpClient, asset.DownloadUrl, archivePath);
            ZipFile.ExtractToDirectory(archivePath, options.OutputDirectory, overwriteFiles: true);
        }
        finally
        {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
        }

        var executablePath = Path.Combine(options.OutputDirectory, StudioToolDefaults.ExecutableName);
        if (!File.Exists(executablePath))
            throw new InvalidOperationException($"Studio was extracted, but '{StudioToolDefaults.ExecutableName}' was not found in {options.OutputDirectory}.");

        Console.WriteLine($"TurtlePath Studio installed at: {options.OutputDirectory}");
        Console.WriteLine($"Executable: {executablePath}");

        if (options.Launch)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(executablePath) { UseShellExecute = true });
    }

    private static async Task<GitHubRelease> GetReleaseAsync(
        HttpClient httpClient,
        string repository,
        string releaseTag)
    {
        var requestUri = releaseTag.Equals("latest", StringComparison.OrdinalIgnoreCase)
            ? $"https://api.github.com/repos/{repository}/releases/latest"
            : $"https://api.github.com/repos/{repository}/releases/tags/{Uri.EscapeDataString(releaseTag)}";

        using var response = await httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Could not resolve Studio release '{releaseTag}' from {repository}. GitHub returned {(int)response.StatusCode}.");

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var assets = new List<GitHubReleaseAsset>();
        if (document.RootElement.TryGetProperty("assets", out var assetsElement))
        {
            foreach (var assetElement in assetsElement.EnumerateArray())
            {
                var name = assetElement.GetProperty("name").GetString() ?? string.Empty;
                var downloadUrl = assetElement.GetProperty("browser_download_url").GetString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(downloadUrl))
                    assets.Add(new GitHubReleaseAsset(name, downloadUrl));
            }
        }

        return new GitHubRelease(assets);
    }

    private static async Task DownloadAsync(
        HttpClient httpClient,
        string downloadUrl,
        string destinationPath)
    {
        using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Could not download Studio asset. GitHub returned {(int)response.StatusCode}.");

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination);
    }
}

internal sealed record GitHubRelease(IReadOnlyList<GitHubReleaseAsset> Assets);

internal sealed record GitHubReleaseAsset(string Name, string DownloadUrl);

internal static class StudioToolDefaults
{
    public const string Repository = "mape1402/turtlepath";

    public const string ReleaseTag = "studio-v1.0.0";

    public const string AssetName = "TurtlePath.Studio.win-x64.zip";

    public const string ExecutableName = "TurtlePath.Studio.App.exe";
}
