using TurtlePath.Studio.Abstractions.Workspace;

namespace TurtlePath.Studio.Web.Workspace;

public sealed class BrowserStudioWorkspaceService : IStudioWorkspaceService
{
    public Task<string> PickOutputDirectoryAsync(
        string currentDirectory,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(currentDirectory);
    }

    public Task OpenDirectoryAsync(
        string directory,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
