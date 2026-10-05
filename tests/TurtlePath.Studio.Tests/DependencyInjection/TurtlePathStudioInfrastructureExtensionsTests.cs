using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Environment;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Infrastructure.Commands;
using TurtlePath.Studio.Infrastructure.DependencyInjection;
using TurtlePath.Studio.Infrastructure.Environment;
using TurtlePath.Studio.Infrastructure.Projects;
using TurtlePath.Studio.Infrastructure.Templates;
using TurtlePath.Studio.Infrastructure.Validation;

namespace TurtlePath.Studio.Tests.DependencyInjection;

public sealed class TurtlePathStudioInfrastructureExtensionsTests
{
    [Fact]
    public void AddTurtlePathStudioInfrastructure_registers_services()
    {
        var services = new ServiceCollection();

        var result = services.AddTurtlePathStudioInfrastructure();

        Assert.Same(services, result);
        var httpClient = Assert.IsType<HttpClient>(services.Single(service => service.ServiceType == typeof(HttpClient)).ImplementationInstance);
        Assert.Equal(TimeSpan.FromSeconds(15), httpClient.Timeout);
        Assert.Contains(services, service => service.ServiceType == typeof(ICommandExecutor) && service.ImplementationType == typeof(ProcessCommandExecutor));
        Assert.Contains(services, service => service.ServiceType == typeof(IDotNetEnvironmentReader) && service.ImplementationType == typeof(DotNetEnvironmentReader));
        Assert.Contains(services, service => service.ServiceType == typeof(ITemplatePackageManager) && service.ImplementationType == typeof(DotNetTemplatePackageManager));
        Assert.Contains(services, service => service.ServiceType == typeof(IProjectGenerator) && service.ImplementationType == typeof(DotNetProjectGenerator));
        Assert.Contains(services, service => service.ServiceType == typeof(IProjectValidator) && service.ImplementationType == typeof(DotNetProjectValidator));
    }

    [Fact]
    public void AddTurtlePathStudioInfrastructure_validates_services_argument()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null).AddTurtlePathStudioInfrastructure());
    }
}
