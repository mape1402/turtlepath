namespace TurtlePath.Automations
{
    using DynaBee.FluentApi.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System.Reflection;
    using TurtlePath.Automations.Generation;

    /// <summary>
    /// Registers TurtlePath automation profiles and attributed requests.
    /// </summary>
    public static class AutomationsTurtlePathBuilderExtensions
    {
        /// <summary>
        /// Discovers automation profiles and attributes from the supplied assemblies.
        /// </summary>
        public static ITurtlePathBuilder UseAutomations(this ITurtlePathBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var descriptors = AutomationDescriptorDiscovery.Discover(assemblies);
            var generationOptions = new AutomationHandlerGenerationOptions();
            var assemblyBuilderFactory = new DynaBeeAssemblyBuilderFactory();
            builder.Services.TryAddSingleton<IDynaBeeAssemblyBuilderFactory>(assemblyBuilderFactory);

            var handlerTypeGenerator = new AutomationHandlerTypeGenerator(
                assemblyBuilderFactory,
                generationOptions,
                new AutomationHandlerBaseTypeResolver(),
                new DefaultAutomationHandlerTypeNamePolicy());

            new AutomationHandlerRegistration(handlerTypeGenerator).Register(builder.Services, descriptors);

            return builder;
        }
    }
}
