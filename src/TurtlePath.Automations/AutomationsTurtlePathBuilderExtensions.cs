namespace TurtlePath.Automations
{
    using System.Reflection;
    using TurtlePath.Automations.Generation;
    using TurtlePath.Automations.Generation.DynaBeeIntegration;

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
            var handlerTypeGenerator = new DynaBeeAutomationHandlerTypeGeneratorFactory()
                .Create(builder.Services);

            new AutomationHandlerRegistration(
                handlerTypeGenerator,
                new AutomationHandlerServiceTypeResolver(),
                new Options.AutomationQueryOptionsRegistration(),
                new Options.AutomationCommandResponseOptionsRegistration()).Register(builder.Services, descriptors);

            return builder;
        }
    }
}
