namespace TurtlePath.Studio.App.Guides;

public interface IStudioGuideProvider
{
    Task<IReadOnlyList<StudioGuideOption>> GetGuidesAsync(
        string packageId,
        string templateVersion,
        CancellationToken cancellationToken = default);

    Task<StudioGuideDocument> GetGuideAsync(
        StudioGuideOption guide,
        StudioGuideCulture culture,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);
}
