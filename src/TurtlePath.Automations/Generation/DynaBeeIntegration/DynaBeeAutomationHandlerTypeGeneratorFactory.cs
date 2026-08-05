namespace TurtlePath.Automations.Generation.DynaBeeIntegration
{
    using DynaBee.FluentApi.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal sealed class DynaBeeAutomationHandlerTypeGeneratorFactory : IAutomationHandlerTypeGeneratorFactory
    {
        public IAutomationHandlerTypeGenerator Create(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var assemblyBuilderFactory = new DynaBeeAssemblyBuilderFactory();
            services.TryAddSingleton<IDynaBeeAssemblyBuilderFactory>(assemblyBuilderFactory);

            return new DynaBeeAutomationHandlerTypeGenerator(
                assemblyBuilderFactory,
                new AutomationHandlerGenerationOptions(),
                new AutomationHandlerBaseTypeResolver(),
                new DefaultAutomationHandlerTypeNamePolicy());
        }
    }
}
