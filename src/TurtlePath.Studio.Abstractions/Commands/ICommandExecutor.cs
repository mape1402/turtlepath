namespace TurtlePath.Studio.Abstractions.Commands;

public interface ICommandExecutor
{
    Task<CommandExecutionResult> ExecuteAsync(
        CommandSpec command,
        CancellationToken cancellationToken = default);
}
