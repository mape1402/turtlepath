using System.IO.Compression;
using System.Text.Json;
using TurtlePath.Studio.Application.Defaults;

namespace TurtlePath.Studio.App.Guides;

/// <summary>
/// Resolves template guides from the versioned documentation package published on NuGet.
/// The downloaded package is retained locally so the guide remains available offline.
/// </summary>
public sealed class StudioGuideProvider(HttpClient httpClient) : IStudioGuideProvider
{
    private const string DocumentationPackageId = "TurtlePath.Template.Documentation";
    private const string CacheVersion = "v10";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private DocumentationPackageManifest? latestManifest;

    public async Task<IReadOnlyList<StudioGuideOption>> GetGuidesAsync(
        string packageId,
        string templateVersion,
        CancellationToken cancellationToken = default)
    {
        packageId = string.IsNullOrWhiteSpace(packageId)
            ? TurtlePathStudioDefaults.TemplatePackageId
            : packageId;

        try
        {
            // Resolve the manifest from NuGet once per Studio session. The package and rendered guide
            // remain cached locally, while the first lookup can still discover a newly published guide.
            var manifest = await LoadLatestManifestAsync(forceRefresh: true, cancellationToken);
            var normalizedVersion = NormalizeVersion(templateVersion);
            var mapping = manifest.Map.FirstOrDefault(candidate => candidate.TemplateVersions.Any(version =>
                string.Equals(NormalizeVersion(version), normalizedVersion, StringComparison.OrdinalIgnoreCase)));

            if (mapping is null)
                return [];

            var guideVersion = NormalizeVersion(mapping.GuideVersion);
            var cultures = new[]
            {
                new StudioGuideCulture("en", "English", string.Empty),
                new StudioGuideCulture("es", "Espanol", string.Empty)
            };

            return [new StudioGuideOption(
                $"template-use-guide-{guideVersion}",
                "TurtlePath Template Use Guide",
                guideVersion,
                packageId,
                BuildExactRange(normalizedVersion),
                mapping.TemplateVersions,
                cultures,
                "NuGet")];
        }
        catch
        {
            return [];
        }
    }

    public async Task<StudioGuideDocument> GetGuideAsync(
        StudioGuideOption guide,
        StudioGuideCulture culture,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(guide);
        ArgumentNullException.ThrowIfNull(culture);

        var packageVersion = NormalizeVersion(guide.DocumentationVersion);
        var htmlPath = Path.Combine(GetGuideCacheDirectory(packageVersion), $"{culture.Code}.html");

        if (!forceRefresh && File.Exists(htmlPath))
        {
            return new StudioGuideDocument(
                guide,
                culture,
                await File.ReadAllTextAsync(htmlPath, cancellationToken),
                $"Cached guide {packageVersion} from {DocumentationPackageId}.",
                LoadedFromCache: true,
                IsEmbeddedFallback: false);
        }

        try
        {
            var packagePath = await EnsurePackageAsync(packageVersion, forceRefresh, cancellationToken);
            var markdown = ReadGuideMarkdown(packagePath, packageVersion, culture.Code);
            var html = await RenderMarkdownAsync(markdown, guide, culture, cancellationToken);
            Directory.CreateDirectory(GetGuideCacheDirectory(packageVersion));
            await File.WriteAllTextAsync(htmlPath, html, cancellationToken);

            return new StudioGuideDocument(
                guide,
                culture,
                html,
                $"Loaded guide {packageVersion} from {DocumentationPackageId}.",
                LoadedFromCache: false,
                IsEmbeddedFallback: false);
        }
        catch when (File.Exists(htmlPath))
        {
            return new StudioGuideDocument(
                guide,
                culture,
                await File.ReadAllTextAsync(htmlPath, cancellationToken),
                $"Using cached guide {packageVersion}; the documentation package could not be read.",
                LoadedFromCache: true,
                IsEmbeddedFallback: false);
        }
        catch
        {
            return new StudioGuideDocument(
                guide,
                culture,
                SimpleMarkdownRenderer.Render("# Guide unavailable\n\nThe selected documentation package is not available locally.", "Guide unavailable"),
                $"Guide {packageVersion} is not available locally. The documentation package may not be published yet or could not be downloaded.",
                LoadedFromCache: false,
                IsEmbeddedFallback: true);
        }
    }

    private async Task<DocumentationPackageManifest> LoadLatestManifestAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && latestManifest is not null)
            return latestManifest;

        if (!forceRefresh)
        {
            var cachedManifest = await TryLoadCachedManifestAsync(cancellationToken);
            if (cachedManifest is not null)
                return latestManifest = cachedManifest;

            return latestManifest = new DocumentationPackageManifest([]);
        }

        try
        {
            var indexUrl = $"https://api.nuget.org/v3-flatcontainer/{DocumentationPackageId.ToLowerInvariant()}/index.json";
            using var response = await httpClient.GetAsync(indexUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var index = await JsonSerializer.DeserializeAsync<NuGetVersionIndex>(
                await response.Content.ReadAsStreamAsync(cancellationToken), JsonOptions, cancellationToken);
            var latestVersion = index?.Versions?
                .Where(version => Version.TryParse(NormalizeVersion(version), out _))
                .OrderByDescending(version => Version.Parse(NormalizeVersion(version)))
                .FirstOrDefault();

            if (latestVersion is not null)
            {
                var packagePath = await EnsurePackageAsync(NormalizeVersion(latestVersion), forceRefresh, cancellationToken);
                latestManifest = ReadPackageManifest(packagePath);
                await File.WriteAllTextAsync(GetLatestVersionPath(), NormalizeVersion(latestVersion), cancellationToken);
                return latestManifest;
            }
        }
        catch
        {
            // Offline startup is supported by the last package downloaded locally.
        }

        var cachedVersionPath = GetLatestVersionPath();
        if (File.Exists(cachedVersionPath))
        {
            try
            {
                var cachedVersion = NormalizeVersion(await File.ReadAllTextAsync(cachedVersionPath, cancellationToken));
                var cachedPackagePath = Path.Combine(
                    GetPackageCacheDirectory(cachedVersion),
                    $"{DocumentationPackageId}.{cachedVersion}.nupkg");

                if (File.Exists(cachedPackagePath))
                    latestManifest = ReadPackageManifest(cachedPackagePath);

                return latestManifest ??= new DocumentationPackageManifest([]);
            }
            catch
            {
                return latestManifest = new DocumentationPackageManifest([]);
            }
        }

        return latestManifest = new DocumentationPackageManifest([]);
    }

    private static async Task<DocumentationPackageManifest?> TryLoadCachedManifestAsync(CancellationToken cancellationToken)
    {
        var cachedVersionPath = GetLatestVersionPath();
        if (!File.Exists(cachedVersionPath))
            return null;

        var cachedVersion = NormalizeVersion(await File.ReadAllTextAsync(cachedVersionPath, cancellationToken));
        var packagePath = Path.Combine(
            GetPackageCacheDirectory(cachedVersion),
            $"{DocumentationPackageId}.{cachedVersion}.nupkg");

        return File.Exists(packagePath) ? ReadPackageManifest(packagePath) : null;
    }

    private async Task<string> EnsurePackageAsync(string version, bool forceRefresh, CancellationToken cancellationToken)
    {
        var directory = GetPackageCacheDirectory(version);
        var packagePath = Path.Combine(directory, $"{DocumentationPackageId}.{version}.nupkg");
        if (!forceRefresh && File.Exists(packagePath))
            return packagePath;

        var packageUrl = $"https://api.nuget.org/v3-flatcontainer/{DocumentationPackageId.ToLowerInvariant()}/{version.ToLowerInvariant()}/{DocumentationPackageId.ToLowerInvariant()}.{version.ToLowerInvariant()}.nupkg";
        using var response = await httpClient.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        Directory.CreateDirectory(directory);
        var temporaryPath = $"{packagePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var output = File.Create(temporaryPath))
                await response.Content.CopyToAsync(output, cancellationToken);

            File.Move(temporaryPath, packagePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }

        return packagePath;
    }

    private static DocumentationPackageManifest ReadPackageManifest(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.GetEntry("guide-manifest.json") ?? throw new InvalidDataException("The documentation package has no guide-manifest.json.");
        using var stream = entry.Open();
        return JsonSerializer.Deserialize<DocumentationPackageManifest>(stream, JsonOptions)
            ?? new DocumentationPackageManifest([]);
    }

    private static string ReadGuideMarkdown(string packagePath, string version, string culture)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.GetEntry($"user_guide_v{version}_{culture}.md")
            ?? throw new FileNotFoundException($"Guide content for culture '{culture}' was not found.");
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }

    private static Task<string> RenderMarkdownAsync(
        string markdown,
        StudioGuideOption guide,
        StudioGuideCulture culture,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => SimpleMarkdownRenderer.Render(markdown, $"{guide.Title} ({culture.Title})"),
            cancellationToken);
    }

    private static string GetPackageCacheDirectory(string version) => Path.Combine(GetCacheRoot(), "Packages", DocumentationPackageId, version);

    private static string GetGuideCacheDirectory(string version) => Path.Combine(GetPackageCacheDirectory(version), "Rendered");

    private static string GetLatestVersionPath() => Path.Combine(GetCacheRoot(), "latest.txt");

    private static string GetCacheRoot() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TurtlePath", "Studio", "Docs", CacheVersion);

    private static string BuildExactRange(string version) => $"[{version}]";

    private static string NormalizeVersion(string version)
    {
        var normalized = version.Trim().TrimStart('v');
        var metadataIndex = normalized.IndexOf('+', StringComparison.Ordinal);
        return metadataIndex >= 0 ? normalized[..metadataIndex] : normalized;
    }

    private sealed record NuGetVersionIndex(IReadOnlyList<string> Versions);

    private sealed record DocumentationPackageManifest(IReadOnlyList<DocumentationMap> Map);

    private sealed record DocumentationMap(string GuideVersion, IReadOnlyList<string> TemplateVersions);
}
