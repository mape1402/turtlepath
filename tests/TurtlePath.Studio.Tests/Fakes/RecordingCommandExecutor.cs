using TurtlePath.Studio.Abstractions.Commands;

namespace TurtlePath.Studio.Tests.Fakes;

internal sealed class RecordingCommandExecutor : ICommandExecutor
{
    private readonly Queue<CommandExecutionResult> results = new();

    public List<CommandSpec> Commands { get; } = [];

    public void EnqueueSuccess(params string[] output)
    {
        var lines = output
            .Select(text => new CommandOutputLine(CommandOutputKind.StandardOutput, text, DateTimeOffset.Now))
            .ToArray();

        results.Enqueue(new CommandExecutionResult(
            new CommandSpec("fake", [], Environment.CurrentDirectory),
            0,
            TimeSpan.Zero,
            lines));
    }

    public Task<CommandExecutionResult> ExecuteAsync(
        CommandSpec command,
        CancellationToken cancellationToken = default)
    {
        Commands.Add(command);

        if (results.Count > 0)
            return Task.FromResult(results.Dequeue() with { Command = command });

        return Task.FromResult(new CommandExecutionResult(
            command,
            0,
            TimeSpan.Zero,
            []));
    }
}
