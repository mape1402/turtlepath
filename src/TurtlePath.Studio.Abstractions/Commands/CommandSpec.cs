namespace TurtlePath.Studio.Abstractions.Commands;

public sealed record CommandSpec(
    string FileName,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory)
{
    public string DisplayText => $"{FileName} {string.Join(" ", Arguments)}";
}
