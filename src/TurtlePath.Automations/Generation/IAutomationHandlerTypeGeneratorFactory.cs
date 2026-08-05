namespace TurtlePath.Automations.Generation
{
    using Microsoft.Extensions.DependencyInjection;

    internal interface IAutomationHandlerTypeGeneratorFactory
    {
        IAutomationHandlerTypeGenerator Create(IServiceCollection services);
    }
}
