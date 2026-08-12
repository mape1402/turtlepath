using System.Runtime.InteropServices;
using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Environment;

namespace TurtlePath.Studio.Infrastructure.Environment;

public sealed class DotNetEnvironmentReader(ICommandExecutor commandExecutor) : IDotNetEnvironmentReader
{
    public async Task<DotNetEnvironmentInfo> ReadAsync(CancellationToken cancellationToken = default)
    {
        var version = await commandExecutor.ExecuteAsync(
            new CommandSpec("dotnet", ["--version"], global::System.Environment.CurrentDirectory),
            cancellationToken);

        if (!version.Succeeded)
        {
            return new DotNetEnvironmentInfo(
                false,
                string.Empty,
                [],
                string.Empty,
                string.Join(global::System.Environment.NewLine, version.Output.Select(line => line.Text)));
        }

        var sdks = await commandExecutor.ExecuteAsync(
            new CommandSpec("dotnet", ["--list-sdks"], global::System.Environment.CurrentDirectory),
            cancellationToken);

        var path = await commandExecutor.ExecuteAsync(
            CreateDotNetPathCommand(),
            cancellationToken);

        return new DotNetEnvironmentInfo(
            true,
            FirstText(version),
            ParseSdks(sdks),
            FirstText(path),
            string.Empty);
    }

    private static CommandSpec CreateDotNetPathCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new CommandSpec("where", ["dotnet"], global::System.Environment.CurrentDirectory);

        return new CommandSpec("which", ["dotnet"], global::System.Environment.CurrentDirectory);
    }

    private static IReadOnlyList<DotNetSdkInfo> ParseSdks(CommandExecutionResult result)
    {
        if (!result.Succeeded)
            return [];

        return result.Output
            .Where(line => line.Kind == CommandOutputKind.StandardOutput)
            .Select(line => line.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(ParseSdk)
            .ToArray();
    }

    private static DotNetSdkInfo ParseSdk(string text)
    {
        var bracketIndex = text.IndexOf('[', StringComparison.Ordinal);
        if (bracketIndex < 0)
            return new DotNetSdkInfo(text.Trim(), string.Empty);

        var version = text[..bracketIndex].Trim();
        var path = text[(bracketIndex + 1)..].Trim().TrimEnd(']');

        return new DotNetSdkInfo(version, path);
    }

    private static string FirstText(CommandExecutionResult result)
    {
        return result.Output
            .FirstOrDefault(line => line.Kind == CommandOutputKind.StandardOutput)
            ?.Text
            ?.Trim() ?? string.Empty;
    }
}
