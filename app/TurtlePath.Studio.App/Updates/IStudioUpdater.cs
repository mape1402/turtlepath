namespace TurtlePath.Studio.App.Updates;

public interface IStudioUpdater
{
    Task<StudioUpdateCheckResult> CheckForUpdatesAsync(string packageId, string channel, CancellationToken cancellationToken = default);

    Task StartUpdateAsync(StudioUpdateCheckResult update, CancellationToken cancellationToken = default);
}
