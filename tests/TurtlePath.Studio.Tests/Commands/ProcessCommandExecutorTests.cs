using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Infrastructure.Commands;
using System.Reflection;

namespace TurtlePath.Studio.Tests.Commands;

public sealed class ProcessCommandExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_runs_process_and_captures_output()
    {
        var executor = new ProcessCommandExecutor();

        var result = await executor.ExecuteAsync(new CommandSpec(
            "dotnet",
            ["--version"],
            global::System.Environment.CurrentDirectory));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains(result.Output, line =>
            line.Kind == CommandOutputKind.StandardOutput &&
            !string.IsNullOrWhiteSpace(line.Text));
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_returns_failure_result_when_process_cannot_start()
    {
        var executor = new ProcessCommandExecutor();

        var result = await executor.ExecuteAsync(new CommandSpec(
            "missing-turtlepath-command",
            [],
            global::System.Environment.CurrentDirectory));

        Assert.False(result.Succeeded);
        Assert.Equal(-1, result.ExitCode);
        Assert.Contains(result.Output, line =>
            line.Kind == CommandOutputKind.StandardError &&
            !string.IsNullOrWhiteSpace(line.Text));
    }

    [Fact]
    public async Task ExecuteAsync_validates_command_argument()
    {
        var executor = new ProcessCommandExecutor();

        await Assert.ThrowsAsync<ArgumentNullException>(() => executor.ExecuteAsync(null));
    }

    [Fact]
    public void Private_helpers_resolve_non_dotnet_names_and_ignore_null_output()
    {
        var resolve = typeof(ProcessCommandExecutor).GetMethod(
            "ResolveFileName",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var addOutput = typeof(ProcessCommandExecutor).GetMethod(
            "AddOutput",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var output = new List<CommandOutputLine>();

        var resolved = Assert.IsType<string>(resolve.Invoke(null, ["custom-command"]));
        addOutput.Invoke(null, [output, CommandOutputKind.StandardOutput, null]);
        addOutput.Invoke(null, [output, CommandOutputKind.StandardError, "error"]);

        Assert.Equal("custom-command", resolved);
        var line = Assert.Single(output);
        Assert.Equal(CommandOutputKind.StandardError, line.Kind);
        Assert.Equal("error", line.Text);
    }
}
