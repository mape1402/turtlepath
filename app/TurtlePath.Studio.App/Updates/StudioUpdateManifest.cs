namespace TurtlePath.Studio.App.Updates;

public sealed record StudioUpdateManifest(
    string Channel,
    string LatestVersion,
    string MinimumSupportedVersion,
    string ReleaseNotesUrl,
    IReadOnlyList<StudioUpdatePackage> Packages);
