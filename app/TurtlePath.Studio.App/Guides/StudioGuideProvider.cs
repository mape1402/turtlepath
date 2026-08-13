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
            guide.Id);

        return root;
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
