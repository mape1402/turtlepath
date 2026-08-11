namespace TurtlePath.Studio.App.Settings;

public interface IStudioSettingsStore
{
    StudioSettings Load();

    void Save(StudioSettings settings);

    void Reset();
}
