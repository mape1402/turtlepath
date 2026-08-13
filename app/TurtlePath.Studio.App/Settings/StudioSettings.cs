namespace TurtlePath.Studio.App.Settings;

public sealed record StudioSettings(
    string DefaultOutputRoot,
    string ProjectNamePlaceholder,
    bool RestoreAfterCreation,
    bool BuildAfterCreation,
    bool TestAfterCreation,
    bool HideGuideAfterCreation,
    string UpdateManifestUrl,
    string UpdateChannel,
    bool CheckUpdatesOnStartup);
