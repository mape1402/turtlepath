using System.Diagnostics;

var arguments = UpdateArguments.Parse(args);
if (arguments is null)
{
    Console.Error.WriteLine("Usage: TurtlePath.Studio.Updater --source <directory> --target <directory> --pid <process id> --launch <exe path>");
    return 2;
}

try
{
    await WaitForStudioAsync(arguments.ProcessId);
    CopyDirectory(arguments.SourceDirectory, arguments.TargetDirectory);
    StartStudio(arguments.LaunchPath);

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}

static async Task WaitForStudioAsync(int processId)
{
    try
    {
        using var process = Process.GetProcessById(processId);
        await process.WaitForExitAsync();
    }
    catch (ArgumentException)
    {
        // Process already exited.
    }
}

static void CopyDirectory(string sourceDirectory, string targetDirectory)
{
    Directory.CreateDirectory(targetDirectory);

    foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
    {
        var relativePath = Path.GetRelativePath(sourceDirectory, directory);
        Directory.CreateDirectory(Path.Combine(targetDirectory, relativePath));
    }

    foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
    {
        var relativePath = Path.GetRelativePath(sourceDirectory, file);
        var targetPath = Path.Combine(targetDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(file, targetPath, overwrite: true);
    }
}

static void StartStudio(string launchPath)
{
    if (!File.Exists(launchPath))
        return;

    Process.Start(new ProcessStartInfo
    {
        FileName = launchPath,
        UseShellExecute = true,
        WorkingDirectory = Path.GetDirectoryName(launchPath)
    });
}

internal sealed record UpdateArguments(string SourceDirectory, string TargetDirectory, int ProcessId, string LaunchPath)
{
    public static UpdateArguments? Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length - 1; index += 2)
        {
            if (!args[index].StartsWith("--", StringComparison.Ordinal))
                return null;

            values[args[index][2..]] = args[index + 1];
        }

        return values.TryGetValue("source", out var source) &&
               values.TryGetValue("target", out var target) &&
               values.TryGetValue("pid", out var pidValue) &&
               values.TryGetValue("launch", out var launch) &&
               int.TryParse(pidValue, out var pid)
            ? new UpdateArguments(source, target, pid, launch)
            : null;
    }
}
