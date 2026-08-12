namespace TurtlePath.Studio.Abstractions.Templates;

public sealed record TemplatePackageInfo(
    string PackageId,
    string Version,
    bool IsInstalled,
    string LatestVersion = "")
{
    public bool HasLatestVersion => !string.IsNullOrWhiteSpace(LatestVersion);

    public bool IsLatest => IsInstalled
        && HasLatestVersion
        && string.Equals(NormalizeVersion(Version), NormalizeVersion(LatestVersion), StringComparison.OrdinalIgnoreCase);

    public bool IsOutdated => IsInstalled
        && HasLatestVersion
        && !IsLatest;

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return string.Empty;

        var normalized = version.Trim();
        if (normalized.StartsWith('v'))
            normalized = normalized[1..];

        var metadataIndex = normalized.IndexOf('+', StringComparison.Ordinal);
        if (metadataIndex >= 0)
            normalized = normalized[..metadataIndex];

        return normalized;
    }
}
