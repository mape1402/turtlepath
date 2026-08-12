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
        && string.Equals(Version, LatestVersion, StringComparison.OrdinalIgnoreCase);
}
