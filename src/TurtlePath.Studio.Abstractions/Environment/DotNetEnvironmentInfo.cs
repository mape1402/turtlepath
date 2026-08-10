namespace TurtlePath.Studio.Abstractions.Environment;

public sealed record DotNetEnvironmentInfo(
    bool IsAvailable,
    string Version,
    IReadOnlyList<DotNetSdkInfo> Sdks,
    string DotNetPath,
    string Error)
{
    public bool SupportsNet9 => Sdks.Any(sdk => sdk.Version.StartsWith("9.", StringComparison.OrdinalIgnoreCase));

    public bool SupportsNet10 => Sdks.Any(sdk => sdk.Version.StartsWith("10.", StringComparison.OrdinalIgnoreCase));
}
