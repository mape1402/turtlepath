using System.Diagnostics;
using TurtlePath.Studio.Abstractions.Workspace;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace TurtlePath.Studio.App.Workspace;

public sealed class WindowsStudioWorkspaceService : IStudioWorkspaceService
{
    public async Task<string> PickOutputDirectoryAsync(
        string currentDirectory,
        CancellationToken cancellationToken = default)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");

        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        var platformWindow = window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

        if (platformWindow is not null)
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(platformWindow));

        var folder = await picker.PickSingleFolderAsync();

        return folder?.Path ?? currentDirectory;
    }

    public Task OpenDirectoryAsync(
        string directory,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }

        return Task.CompletedTask;
    }
}
