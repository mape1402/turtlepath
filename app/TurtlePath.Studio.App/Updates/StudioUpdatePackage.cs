namespace TurtlePath.Studio.App.Updates;

public sealed record StudioUpdatePackage(
    string Platform,
    string Url,
    string Sha256,
    long Size);
