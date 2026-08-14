namespace TurtlePath.Studio.App.Updates;

public sealed record StudioUpdateCheckResult(
    bool IsAvailable,
    string CurrentVersion,
    string LatestVersion,
    string Message,
    StudioPackage? Package)
{
    public bool Succeeded => Package is not null;
}
