using TurtlePath.Studio.Infrastructure.Environment;
using TurtlePath.Studio.Tests.Fakes;

namespace TurtlePath.Studio.Tests.Environment;

public sealed class DotNetEnvironmentReaderTests
{
    [Fact]
    public async Task ReadAsync_returns_unavailable_info_when_dotnet_version_fails()
    {
        var executor = new RecordingCommandExecutor();
        executor.EnqueueFailure("dotnet is not installed");
        var reader = new DotNetEnvironmentReader(executor);

        var info = await reader.ReadAsync();

        Assert.False(info.IsAvailable);
        Assert.Equal(string.Empty, info.Version);
        Assert.Empty(info.Sdks);
        Assert.Equal(string.Empty, info.DotNetPath);
        Assert.Equal("dotnet is not installed", info.Error);
        Assert.Equal(["dotnet"], executor.Commands.Select(command => command.FileName));
    }

    [Fact]
    public async Task ReadAsync_parses_version_sdks_and_dotnet_path()
    {
        var executor = new RecordingCommandExecutor();
        executor.EnqueueSuccess("  10.0.100  ");
        executor.EnqueueSuccess(
            "9.0.305 [C:\\Program Files\\dotnet\\sdk]",
            "10.0.100-preview [C:\\Program Files\\dotnet\\sdk]",
            "orphan-sdk-line");
        executor.EnqueueSuccess("C:\\Program Files\\dotnet\\dotnet.exe");
        var reader = new DotNetEnvironmentReader(executor);

        var info = await reader.ReadAsync();

        Assert.True(info.IsAvailable);
        Assert.Equal("10.0.100", info.Version);
        Assert.Equal("C:\\Program Files\\dotnet\\dotnet.exe", info.DotNetPath);
        Assert.Collection(
            info.Sdks,
            sdk =>
            {
                Assert.Equal("9.0.305", sdk.Version);
                Assert.Equal("C:\\Program Files\\dotnet\\sdk", sdk.Path);
            },
            sdk =>
            {
                Assert.Equal("10.0.100-preview", sdk.Version);
                Assert.Equal("C:\\Program Files\\dotnet\\sdk", sdk.Path);
            },
            sdk =>
            {
                Assert.Equal("orphan-sdk-line", sdk.Version);
                Assert.Equal(string.Empty, sdk.Path);
            });
        Assert.Equal(["dotnet", "dotnet"], executor.Commands.Take(2).Select(command => command.FileName));
        Assert.Contains(executor.Commands[2].FileName, new[] { "where", "which" });
    }

    [Fact]
    public async Task ReadAsync_returns_empty_sdks_when_sdk_command_fails()
    {
        var executor = new RecordingCommandExecutor();
        executor.EnqueueSuccess("10.0.100");
        executor.EnqueueFailure("sdk list failed");
        executor.EnqueueSuccess("dotnet");
        var reader = new DotNetEnvironmentReader(executor);

        var info = await reader.ReadAsync();

        Assert.True(info.IsAvailable);
        Assert.Empty(info.Sdks);
        Assert.False(info.SupportsNet9);
        Assert.False(info.SupportsNet10);
    }

    [Fact]
    public void DotNetEnvironmentInfo_reports_supported_target_frameworks()
    {
        var info = new TurtlePath.Studio.Abstractions.Environment.DotNetEnvironmentInfo(
            true,
            "10.0.100",
            [
                new TurtlePath.Studio.Abstractions.Environment.DotNetSdkInfo("9.0.305", "sdk"),
                new TurtlePath.Studio.Abstractions.Environment.DotNetSdkInfo("10.0.100", "sdk")
            ],
            "dotnet",
            string.Empty);

        Assert.True(info.SupportsNet9);
        Assert.True(info.SupportsNet10);
    }
}
