namespace TurtlePath.Automations
{
    using System.Reflection;

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
            AutomationHandlerRegistration.Register(builder.Services, descriptors);

            return builder;
        }
    }
}
