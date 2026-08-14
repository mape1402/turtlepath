using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Environment;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Infrastructure.Commands;
using TurtlePath.Studio.Infrastructure.Environment;
using TurtlePath.Studio.Infrastructure.Projects;
using TurtlePath.Studio.Infrastructure.Templates;
using TurtlePath.Studio.Infrastructure.Validation;

namespace TurtlePath.Studio.Infrastructure.DependencyInjection;

public static class TurtlePathStudioInfrastructureExtensions
{
    public static IServiceCollection AddTurtlePathStudioInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        });
        services.AddSingleton<ICommandExecutor, ProcessCommandExecutor>();
        services.AddSingleton<IDotNetEnvironmentReader, DotNetEnvironmentReader>();
        services.AddSingleton<ITemplatePackageManager, DotNetTemplatePackageManager>();
        services.AddSingleton<IProjectGenerator, DotNetProjectGenerator>();
        services.AddSingleton<IProjectValidator, DotNetProjectValidator>();

        return services;
    }
}
