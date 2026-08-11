using Microsoft.Extensions.Logging;
using TurtlePath.Studio.Abstractions.Workspace;
using TurtlePath.Studio.App.Settings;
using TurtlePath.Studio.App.Workspace;
using TurtlePath.Studio.App.ViewModels;
using TurtlePath.Studio.Application.UseCases;
using TurtlePath.Studio.Infrastructure.DependencyInjection;

namespace TurtlePath.Studio.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddTurtlePathStudioInfrastructure();
        builder.Services.AddSingleton<InspectStudioEnvironmentUseCase>();
        builder.Services.AddSingleton<InstallTemplateUseCase>();
        builder.Services.AddSingleton<CreateTurtlePathProjectUseCase>();
        builder.Services.AddSingleton<IStudioSettingsStore, PreferencesStudioSettingsStore>();
        builder.Services.AddSingleton<IStudioWorkspaceService, WindowsStudioWorkspaceService>();
        builder.Services.AddSingleton<StudioViewModel>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
