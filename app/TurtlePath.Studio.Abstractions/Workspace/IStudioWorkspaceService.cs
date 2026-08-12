namespace TurtlePath.Studio.Abstractions.Workspace;

public interface IStudioWorkspaceService
{
    Task<string> PickOutputDirectoryAsync(
        string currentDirectory,
        CancellationToken cancellationToken = default);

    Task OpenDirectoryAsync(
        string directory,
        CancellationToken cancellationToken = default);
}
