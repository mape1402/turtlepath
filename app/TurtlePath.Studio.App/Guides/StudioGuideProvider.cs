using System.IO.Compression;
using System.Text.Json;
using Microsoft.Maui.Storage;
using TurtlePath.Studio.Application.Defaults;

namespace TurtlePath.Studio.App.Guides;

public sealed class StudioGuideProvider(HttpClient httpClient) : IStudioGuideProvider
{
    private const string CacheVersion = "v8";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private IReadOnlyList<StudioGuideOption>? guides;

    public async Task<IReadOnlyList<StudioGuideOption>> GetGuidesAsync(
        string packageId,
        string templateVersion,
        CancellationToken cancellationToken = default)
    {
        packageId = string.IsNullOrWhiteSpace(packageId)
            ? TurtlePathStudioDefaults.TemplatePackageId
            : packageId;

        var knownGuides = await LoadManifestAsync(cancellationToken);
        knownGuides = AddInstalledTemplateVersion(knownGuides, packageId, templateVersion);

        var matchingGuides = knownGuides
            .Where(guide => string.Equals(guide.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
            .Where(guide => VersionRange.Contains(guide.SupportedTemplateVersionRange, templateVersion))
            .OrderByDescending(guide => Version.Parse(guide.DocumentationVersion))
            .ToArray();

        return matchingGuides;
    }

    public async Task<StudioGuideDocument> GetGuideAsync(
        StudioGuideOption guide,
        StudioGuideCulture culture,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(guide);
        ArgumentNullException.ThrowIfNull(culture);

        var cacheDirectory = GetGuideCacheDirectory(guide);
        var htmlPath = Path.Combine(cacheDirectory, $"{culture.Code}.html");
        var manifestPath = Path.Combine(cacheDirectory, "manifest.json");

        var installedPackage = await TryLoadInstalledTemplateGuideAsync(guide, culture, cancellationToken);
        if (installedPackage is not null)
        {
            Directory.CreateDirectory(cacheDirectory);
            await File.WriteAllTextAsync(htmlPath, installedPackage.Html, cancellationToken);
            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(guide, JsonOptions), cancellationToken);
            return installedPackage;
        }

        if (!forceRefresh && File.Exists(htmlPath))
        {
            return new StudioGuideDocument(
                guide,
                culture,
                await File.ReadAllTextAsync(htmlPath, cancellationToken),
                $"Cached guide docs {guide.DocumentationVersion} for template versions {guide.SupportedTemplateVersionRange}.",
                LoadedFromCache: true,
                IsEmbeddedFallback: false);
        }

        if (!forceRefresh)
        {
            var embedded = await TryLoadEmbeddedGuideAsync(guide, culture, cancellationToken);
            if (embedded is not null)
            {
                Directory.CreateDirectory(cacheDirectory);
                await File.WriteAllTextAsync(htmlPath, embedded.Html, cancellationToken);
                await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(guide, JsonOptions), cancellationToken);

                return embedded with
                {
                    Status = $"Using bundled guide docs {guide.DocumentationVersion}; cached locally for the next load.",
                    LoadedFromCache = true,
                    IsEmbeddedFallback = false
                };
            }
        }

        try
        {
            using var response = await httpClient.GetAsync(culture.SourceUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var markdown = await response.Content.ReadAsStringAsync(cancellationToken);
            var html = SimpleMarkdownRenderer.Render(markdown, $"{guide.Title} ({culture.Title})");

            Directory.CreateDirectory(cacheDirectory);
            await File.WriteAllTextAsync(htmlPath, html, cancellationToken);
            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(guide, JsonOptions), cancellationToken);

            return new StudioGuideDocument(
                guide,
                culture,
                html,
                $"Downloaded guide docs {guide.DocumentationVersion} for template versions {guide.SupportedTemplateVersionRange}.",
                LoadedFromCache: false,
                IsEmbeddedFallback: false);
        }
        catch
        {
            if (File.Exists(htmlPath))
            {
                return new StudioGuideDocument(
                    guide,
                    culture,
                    await File.ReadAllTextAsync(htmlPath, cancellationToken),
                    $"Using cached guide docs {guide.DocumentationVersion} for template versions {guide.SupportedTemplateVersionRange}; GitHub is not reachable.",
                    LoadedFromCache: true,
                    IsEmbeddedFallback: false);
            }

            return await LoadEmbeddedFallbackAsync(guide, culture, cancellationToken);
        }
    }

    private static async Task<StudioGuideDocument?> TryLoadEmbeddedGuideAsync(
        StudioGuideOption guide,
        StudioGuideCulture culture,
        CancellationToken cancellationToken)
    {
        try
        {
            var embeddedPath = $"Docs/{guide.Id}/{culture.Code}.md";
            await using var stream = await FileSystem.OpenAppPackageFileAsync(embeddedPath);
            using var reader = new StreamReader(stream);
            var markdown = await reader.ReadToEndAsync(cancellationToken);

            return new StudioGuideDocument(
                guide,
                culture,
                SimpleMarkdownRenderer.Render(markdown, $"{guide.Title} ({culture.Title})"),
                "Using bundled guide documentation.",
                LoadedFromCache: false,
                IsEmbeddedFallback: true);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<StudioGuideDocument> LoadEmbeddedFallbackAsync(
        StudioGuideOption guide,
        StudioGuideCulture culture,
        CancellationToken cancellationToken)
    {
        var embedded = await TryLoadEmbeddedGuideAsync(guide, culture, cancellationToken);
        if (embedded is not null)
            return embedded with
            {
                Status = "Using embedded fallback documentation because GitHub and cache are unavailable.",
                IsEmbeddedFallback = true
            };

        var html = SimpleMarkdownRenderer.Render(
            "# Guide unavailable\n\nThere is no local embedded copy for this guide and GitHub is not reachable. Check the configured documentation manifest or update TurtlePath Studio.",
            "Guide unavailable");

        return new StudioGuideDocument(
            guide,
            culture,
            html,
            "Selected documentation is not cached yet and GitHub is unavailable.",
            LoadedFromCache: false,
            IsEmbeddedFallback: true);
    }

    private static IReadOnlyList<StudioGuideOption> AddInstalledTemplateVersion(
        IReadOnlyList<StudioGuideOption> knownGuides,
        string packageId,
        string templateVersion)
    {
        if (!Version.TryParse(NormalizeVersion(templateVersion), out var parsedTemplateVersion))
            return knownGuides;

        var guide = knownGuides.FirstOrDefault(candidate =>
            string.Equals(candidate.PackageId, packageId, StringComparison.OrdinalIgnoreCase));

        if (guide is null)
            return knownGuides;

        var supportedVersions = guide.SupportedTemplateVersions
            .Append(parsedTemplateVersion.ToString(3))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(version => Version.Parse(NormalizeVersion(version)))
            .ToArray();

        var range = supportedVersions.Length == 1
            ? $"[{supportedVersions[0]}]"
            : $"[{supportedVersions[0]},{supportedVersions[^1]}]";

        return knownGuides
            .Select(candidate => ReferenceEquals(candidate, guide)
                ? candidate with
                {
                    SupportedTemplateVersionRange = range,
                    SupportedTemplateVersions = supportedVersions
                }
                : candidate)
            .ToArray();
    }

    private async Task<StudioGuideDocument?> TryLoadInstalledTemplateGuideAsync(
        StudioGuideOption guide,
        StudioGuideCulture culture,
        CancellationToken cancellationToken)
    {
        var packagePath = FindInstalledTemplatePackage(guide);
        if (packagePath is null)
            return null;

        try
        {
            using var archive = ZipFile.OpenRead(packagePath);
            var entry = archive.Entries.FirstOrDefault(candidate =>
                candidate.FullName.EndsWith($"/docs/Use Guide_{culture.Code}.md", StringComparison.OrdinalIgnoreCase));

            if (entry is null)
                return null;

            await using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            var markdown = await reader.ReadToEndAsync(cancellationToken);

            return new StudioGuideDocument(
                guide,
                culture,
                SimpleMarkdownRenderer.Render(markdown, $"{guide.Title} ({culture.Title})"),
                $"Using guide docs from the installed {guide.PackageId} template package.",
                LoadedFromCache: false,
                IsEmbeddedFallback: false,
                IsTemplatePackage: true);
        }
        catch
        {
            return null;
        }
    }

    private static string? FindInstalledTemplatePackage(StudioGuideOption guide)
    {
        var packageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".templateengine",
            "packages");

        if (!Directory.Exists(packageDirectory))
            return null;

        return Directory.EnumerateFiles(packageDirectory, $"{guide.PackageId}.*.nupkg")
            .Select(path => new
            {
                Path = path,
                Version = ParsePackageVersion(path, guide.PackageId)
            })
            .Where(candidate => candidate.Version is not null && guide.SupportedTemplateVersions.Any(version =>
                string.Equals(NormalizeVersion(version), candidate.Version!.ToString(3), StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(candidate => candidate.Version)
            .Select(candidate => candidate.Path)
            .FirstOrDefault();
    }

    private static Version? ParsePackageVersion(string path, string packageId)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var prefix = $"{packageId}.";
        if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        return Version.TryParse(NormalizeVersion(fileName[prefix.Length..]), out var version)
            ? version
            : null;
    }

    private async Task<IReadOnlyList<StudioGuideOption>> LoadManifestAsync(CancellationToken cancellationToken)
    {
        if (guides is not null)
            return guides;

        await using var stream = await FileSystem.OpenAppPackageFileAsync("Docs/guide-manifest.json");
        var manifest = await JsonSerializer.DeserializeAsync<StudioGuideManifest>(stream, JsonOptions, cancellationToken);
        guides = manifest?.Guides ?? [];

        return guides;
    }

    private static string GetGuideCacheDirectory(StudioGuideOption guide)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TurtlePath",
            "Studio",
            "Docs",
            CacheVersion,
            $"{guide.Id}-{string.Join('-', guide.SupportedTemplateVersions.OrderByDescending(version => Version.Parse(NormalizeVersion(version))).Take(1))}");

        return root;
    }

    private static string NormalizeVersion(string version)
    {
        var normalized = version.Trim();
        if (normalized.StartsWith('v'))
            normalized = normalized[1..];

        var metadataIndex = normalized.IndexOf('+', StringComparison.Ordinal);
        if (metadataIndex >= 0)
            normalized = normalized[..metadataIndex];

        return normalized;
    }

    private static class VersionRange
    {
        public static bool Contains(string range, string version)
        {
            if (string.IsNullOrWhiteSpace(version) || !Version.TryParse(Normalize(version), out var parsed))
                return true;

            if (string.IsNullOrWhiteSpace(range) || range.Length < 5)
                return true;

            var includeMin = range[0] == '[';
            var includeMax = range[^1] == ']';
            var parts = range[1..^1].Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                return true;

            return IsLowerBoundValid(parsed, parts[0], includeMin) &&
                   IsUpperBoundValid(parsed, parts[1], includeMax);
        }

        private static bool IsLowerBoundValid(Version version, string minimum, bool inclusive)
        {
            if (string.IsNullOrWhiteSpace(minimum) || !Version.TryParse(Normalize(minimum), out var parsed))
                return true;

            var comparison = version.CompareTo(parsed);
            return inclusive ? comparison >= 0 : comparison > 0;
        }

        private static bool IsUpperBoundValid(Version version, string maximum, bool inclusive)
        {
            if (string.IsNullOrWhiteSpace(maximum) || !Version.TryParse(Normalize(maximum), out var parsed))
                return true;

            var comparison = version.CompareTo(parsed);
            return inclusive ? comparison <= 0 : comparison < 0;
        }

        private static string Normalize(string version)
        {
            var normalized = version.Trim();
            if (normalized.StartsWith('v'))
                normalized = normalized[1..];

            var metadataIndex = normalized.IndexOf('+', StringComparison.Ordinal);
            if (metadataIndex >= 0)
                normalized = normalized[..metadataIndex];

            return normalized;
        }
    }
}
