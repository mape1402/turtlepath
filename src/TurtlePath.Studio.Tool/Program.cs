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
          turtlepath-studio update [options]

        Options:
          --version <tag>     GitHub release tag to download. Default: latest Studio release
          --repo <owner/name> GitHub repository. Default: mape1402/turtlepath
          --asset <name>      Release asset name. Default: TurtlePath.Studio.win-x64.zip
          --output <path>     Install directory. Default: %LOCALAPPDATA%\TurtlePath\Studio
          --force             Replace the existing install directory.
          --no-shortcut       Do not create or update the desktop shortcut.
          --launch            Launch Studio after installation.
          -h|--help           Show help.
        """);
}

internal sealed record StudioInstallOptions(
    StudioToolCommand Command,
    string Repository,
    string ReleaseTag,
    string AssetName,
    string OutputDirectory,
    bool Force,
    bool CreateShortcut,
    bool Launch,
    bool ShowHelp)
{
    public static StudioInstallOptions Parse(string[] args)
    {
        var command = StudioToolCommand.Install;
        var repository = StudioToolDefaults.Repository;
        var releaseTag = StudioToolDefaults.ReleaseTag;
        var assetName = StudioToolDefaults.AssetName;
        var outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TurtlePath",
            "Studio");
        var force = false;
        var createShortcut = true;
        var launch = false;

        var index = 0;
        if (args.Length > 0 && !args[0].StartsWith("-", StringComparison.Ordinal))
        {
            command = args[0].ToLowerInvariant() switch
            {
                "install" => StudioToolCommand.Install,
                "update" => StudioToolCommand.Update,
                _ => throw new InvalidOperationException($"Unknown command '{args[0]}'. Use --help for usage.")
            };

            index = 1;
        }

        while (index < args.Length)
        {
            var arg = args[index];
            switch (arg)
            {
                case "-h":
                case "--help":
                    return new StudioInstallOptions(command, repository, releaseTag, assetName, outputDirectory, force, createShortcut, launch, true);

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

                case "--no-shortcut":
                    createShortcut = false;
                    break;

                case "--launch":
                    launch = true;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown option '{arg}'. Use --help for usage.");
            }

            index++;
        }

        return new StudioInstallOptions(command, repository, releaseTag, assetName, outputDirectory, force, createShortcut, launch, false);
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

        var replaceExistingInstall = options.Force || options.Command == StudioToolCommand.Update;
        if (Directory.Exists(options.OutputDirectory))
        {
            if (!replaceExistingInstall)
                throw new InvalidOperationException($"Install directory already exists: {options.OutputDirectory}. Use --force to replace it.");

            Directory.Delete(options.OutputDirectory, recursive: true);
        }

        Directory.CreateDirectory(options.OutputDirectory);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TurtlePath-Studio-Tool", "1.0"));

        var release = await GetReleaseAsync(httpClient, options.Repository, options.ReleaseTag);
        var asset = release.Assets.FirstOrDefault(item => string.Equals(item.Name, options.AssetName, StringComparison.OrdinalIgnoreCase));
        if (asset is null)
            throw new InvalidOperationException($"Release '{release.TagName}' does not contain asset '{options.AssetName}'.");

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
        Console.WriteLine($"Version: {release.TagName}");
        Console.WriteLine($"Executable: {executablePath}");

        if (options.CreateShortcut)
            CreateDesktopShortcut(executablePath);

        if (options.Launch)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(executablePath) { UseShellExecute = true });
    }

    private static void CreateDesktopShortcut(string executablePath)
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("Desktop shortcut skipped because shortcuts are only supported on Windows.");
            return;
        }

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(desktopPath))
        {
            Console.WriteLine("Desktop shortcut skipped because the desktop folder could not be resolved.");
            return;
        }

        var shortcutPath = Path.Combine(desktopPath, $"{StudioToolDefaults.ShortcutName}.lnk");
        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType is null)
            throw new InvalidOperationException("Could not create the desktop shortcut because Windows Script Host is not available.");

        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Could not create the desktop shortcut shell.");
        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = executablePath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty;
        shortcut.IconLocation = executablePath;
        shortcut.Description = StudioToolDefaults.ShortcutName;
        shortcut.Save();

        Console.WriteLine($"Desktop shortcut: {shortcutPath}");
    }

    private static async Task<GitHubRelease> GetReleaseAsync(
        HttpClient httpClient,
        string repository,
        string releaseTag)
    {
        return releaseTag.Equals(StudioToolDefaults.LatestReleaseTag, StringComparison.OrdinalIgnoreCase)
            ? await GetLatestStudioReleaseAsync(httpClient, repository)
            : await GetReleaseByTagAsync(httpClient, repository, releaseTag);
    }

    private static async Task<GitHubRelease> GetReleaseByTagAsync(
        HttpClient httpClient,
        string repository,
        string releaseTag)
    {
        var requestUri = $"https://api.github.com/repos/{repository}/releases/tags/{Uri.EscapeDataString(releaseTag)}";
        using var response = await httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Could not resolve Studio release '{releaseTag}' from {repository}. GitHub returned {(int)response.StatusCode}.");

        return await ReadReleaseAsync(await response.Content.ReadAsStreamAsync());
    }

    private static async Task<GitHubRelease> GetLatestStudioReleaseAsync(
        HttpClient httpClient,
        string repository)
    {
        var requestUri = $"https://api.github.com/repos/{repository}/releases?per_page=100";
        using var response = await httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Could not resolve the latest Studio release from {repository}. GitHub returned {(int)response.StatusCode}.");

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        foreach (var releaseElement in document.RootElement.EnumerateArray())
        {
            var tagName = releaseElement.GetProperty("tag_name").GetString() ?? string.Empty;
            var isDraft = releaseElement.TryGetProperty("draft", out var draftElement) && draftElement.GetBoolean();
            var isPrerelease = releaseElement.TryGetProperty("prerelease", out var prereleaseElement) && prereleaseElement.GetBoolean();

            if (!isDraft
                && !isPrerelease
                && tagName.StartsWith(StudioToolDefaults.ReleaseTagPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return ReadRelease(releaseElement);
            }
        }

        throw new InvalidOperationException($"No published Studio release was found in {repository}.");
    }

    private static async Task<GitHubRelease> ReadReleaseAsync(Stream stream)
    {
        using var document = await JsonDocument.ParseAsync(stream);
        return ReadRelease(document.RootElement);
    }

    private static GitHubRelease ReadRelease(JsonElement releaseElement)
    {
        var tagName = releaseElement.GetProperty("tag_name").GetString() ?? string.Empty;
        var assets = new List<GitHubReleaseAsset>();
        if (releaseElement.TryGetProperty("assets", out var assetsElement))
        {
            foreach (var assetElement in assetsElement.EnumerateArray())
            {
                var name = assetElement.GetProperty("name").GetString() ?? string.Empty;
                var downloadUrl = assetElement.GetProperty("browser_download_url").GetString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(downloadUrl))
                    assets.Add(new GitHubReleaseAsset(name, downloadUrl));
            }
        }

        return new GitHubRelease(tagName, assets);
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

internal sealed record GitHubRelease(string TagName, IReadOnlyList<GitHubReleaseAsset> Assets);

internal sealed record GitHubReleaseAsset(string Name, string DownloadUrl);

internal enum StudioToolCommand
{
    Install,

    Update
}

internal static class StudioToolDefaults
{
    public const string Repository = "mape1402/turtlepath";

    public const string LatestReleaseTag = "latest";

    public const string ReleaseTag = LatestReleaseTag;

    public const string ReleaseTagPrefix = "studio-v";

    public const string AssetName = "TurtlePath.Studio.win-x64.zip";

    public const string ExecutableName = "TurtlePath.Studio.App.exe";

    public const string ShortcutName = "TurtlePath Studio";
}
