namespace TurtlePath.Studio.Abstractions.Commands;

public sealed record CommandExecutionResult(
    CommandSpec Command,
    int ExitCode,
    TimeSpan Duration,
    IReadOnlyList<CommandOutputLine> Output)
{
    public bool Succeeded => ExitCode == 0;
}
