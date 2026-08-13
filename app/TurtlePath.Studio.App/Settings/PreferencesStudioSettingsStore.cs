using Microsoft.Maui.Storage;

namespace TurtlePath.Studio.App.Settings;

public sealed class PreferencesStudioSettingsStore : IStudioSettingsStore
{
    private const string DefaultOutputRootKey = "studio.defaults.outputRoot";
    private const string ProjectNamePlaceholderKey = "studio.defaults.projectNamePlaceholder";
    private const string RestoreAfterCreationKey = "studio.defaults.restoreAfterCreation";
    private const string BuildAfterCreationKey = "studio.defaults.buildAfterCreation";
    private const string TestAfterCreationKey = "studio.defaults.testAfterCreation";
    private const string HideGuideAfterCreationKey = "studio.defaults.hideGuideAfterCreation";
    private const string UpdateManifestUrlKey = "studio.updates.manifestUrl";
    private const string UpdateChannelKey = "studio.updates.channel";
    private const string CheckUpdatesOnStartupKey = "studio.updates.checkOnStartup";

    public const string DefaultUpdateManifestUrl = "https://github.com/mape1402/turtlepath/releases/latest/download/studio.manifest.json";
    public const string DefaultUpdateChannel = "stable";

    public StudioSettings Load()
    {
        var defaults = CreateDefaults();

        return new StudioSettings(
            Preferences.Default.Get(DefaultOutputRootKey, defaults.DefaultOutputRoot),
            Preferences.Default.Get(ProjectNamePlaceholderKey, defaults.ProjectNamePlaceholder),
            Preferences.Default.Get(RestoreAfterCreationKey, defaults.RestoreAfterCreation),
            Preferences.Default.Get(BuildAfterCreationKey, defaults.BuildAfterCreation),
            Preferences.Default.Get(TestAfterCreationKey, defaults.TestAfterCreation),
            Preferences.Default.Get(HideGuideAfterCreationKey, defaults.HideGuideAfterCreation),
            Preferences.Default.Get(UpdateManifestUrlKey, defaults.UpdateManifestUrl),
            Preferences.Default.Get(UpdateChannelKey, defaults.UpdateChannel),
            Preferences.Default.Get(CheckUpdatesOnStartupKey, defaults.CheckUpdatesOnStartup));
    }

    public void Save(StudioSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Preferences.Default.Set(DefaultOutputRootKey, settings.DefaultOutputRoot);
        Preferences.Default.Set(ProjectNamePlaceholderKey, settings.ProjectNamePlaceholder);
        Preferences.Default.Set(RestoreAfterCreationKey, settings.RestoreAfterCreation);
        Preferences.Default.Set(BuildAfterCreationKey, settings.BuildAfterCreation);
        Preferences.Default.Set(TestAfterCreationKey, settings.TestAfterCreation);
        Preferences.Default.Set(HideGuideAfterCreationKey, settings.HideGuideAfterCreation);
        Preferences.Default.Set(UpdateManifestUrlKey, settings.UpdateManifestUrl);
        Preferences.Default.Set(UpdateChannelKey, settings.UpdateChannel);
        Preferences.Default.Set(CheckUpdatesOnStartupKey, settings.CheckUpdatesOnStartup);
    }

    public void Reset()
    {
        Preferences.Default.Remove(DefaultOutputRootKey);
        Preferences.Default.Remove(ProjectNamePlaceholderKey);
        Preferences.Default.Remove(RestoreAfterCreationKey);
        Preferences.Default.Remove(BuildAfterCreationKey);
        Preferences.Default.Remove(TestAfterCreationKey);
        Preferences.Default.Remove(HideGuideAfterCreationKey);
        Preferences.Default.Remove(UpdateManifestUrlKey);
        Preferences.Default.Remove(UpdateChannelKey);
        Preferences.Default.Remove(CheckUpdatesOnStartupKey);
    }

    private static StudioSettings CreateDefaults()
    {
        return new StudioSettings(
            global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.MyDocuments),
            "TurtlePath.Service",
            true,
            true,
            true,
            false,
            DefaultUpdateManifestUrl,
            DefaultUpdateChannel,
            true);
    }
}
