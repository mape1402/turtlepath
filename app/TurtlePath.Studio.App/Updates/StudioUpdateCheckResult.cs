namespace TurtlePath.Studio.App.Updates;

public sealed record StudioUpdateCheckResult(
    bool IsAvailable,
    string CurrentVersion,
    string LatestVersion,
    string Message,
    StudioUpdateManifest? Manifest,
    StudioUpdatePackage? Package);
