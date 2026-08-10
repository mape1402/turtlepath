using System.Diagnostics;
using TurtlePath.Studio.Abstractions.Commands;

namespace TurtlePath.Studio.Infrastructure.Commands;

public sealed class ProcessCommandExecutor : ICommandExecutor
{
    public async Task<CommandExecutionResult> ExecuteAsync(
        CommandSpec command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var output = new List<CommandOutputLine>();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command.FileName,
                WorkingDirectory = string.IsNullOrWhiteSpace(command.WorkingDirectory)
                    ? global::System.Environment.CurrentDirectory
                    : command.WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            foreach (var argument in command.Arguments)
                process.StartInfo.ArgumentList.Add(argument);

            process.OutputDataReceived += (_, args) => AddOutput(output, CommandOutputKind.StandardOutput, args.Data);
            process.ErrorDataReceived += (_, args) => AddOutput(output, CommandOutputKind.StandardError, args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);
            stopwatch.Stop();

            return new CommandExecutionResult(command, process.ExitCode, stopwatch.Elapsed, output);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            stopwatch.Stop();
            output.Add(new CommandOutputLine(
                CommandOutputKind.StandardError,
                exception.Message,
                DateTimeOffset.Now));

            return new CommandExecutionResult(command, -1, stopwatch.Elapsed, output);
        }
    }

    private static void AddOutput(
        List<CommandOutputLine> output,
        CommandOutputKind kind,
        string text)
    {
        if (text is null)
            return;

        lock (output)
            output.Add(new CommandOutputLine(kind, text, DateTimeOffset.Now));
    }
}
