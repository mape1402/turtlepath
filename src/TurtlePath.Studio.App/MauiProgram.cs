using Microsoft.Extensions.Logging;
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

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddTurtlePathStudioInfrastructure();
        builder.Services.AddSingleton<InspectStudioEnvironmentUseCase>();
        builder.Services.AddSingleton<InstallTemplateUseCase>();
        builder.Services.AddSingleton<CreateTurtlePathProjectUseCase>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
