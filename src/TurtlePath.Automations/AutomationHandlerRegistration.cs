namespace TurtlePath.Automations
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Generation;
    using TurtlePath.Automations.Options;

    internal sealed class AutomationHandlerRegistration
    {
        private readonly IAutomationHandlerTypeGenerator handlerTypeGenerator;
        private readonly IAutomationHandlerServiceTypeResolver serviceTypeResolver;
        private readonly IAutomationQueryOptionsRegistration queryOptionsRegistration;
        private readonly IAutomationCommandResponseOptionsRegistration commandResponseOptionsRegistration;

        public AutomationHandlerRegistration(
            IAutomationHandlerTypeGenerator handlerTypeGenerator,
            IAutomationHandlerServiceTypeResolver serviceTypeResolver,
            IAutomationQueryOptionsRegistration queryOptionsRegistration,
            IAutomationCommandResponseOptionsRegistration commandResponseOptionsRegistration)
        {
            this.handlerTypeGenerator = handlerTypeGenerator ?? throw new ArgumentNullException(nameof(handlerTypeGenerator));
            this.serviceTypeResolver = serviceTypeResolver ?? throw new ArgumentNullException(nameof(serviceTypeResolver));
            this.queryOptionsRegistration = queryOptionsRegistration ?? throw new ArgumentNullException(nameof(queryOptionsRegistration));
            this.commandResponseOptionsRegistration = commandResponseOptionsRegistration ?? throw new ArgumentNullException(nameof(commandResponseOptionsRegistration));
        }

        public void Register(IServiceCollection services, IEnumerable<AutomationDescriptor> descriptors)
        {
            var registry = new AutomationDescriptorRegistry(descriptors);
            services.TryAddSingleton(registry);

            queryOptionsRegistration.Register(services, registry.Descriptors);
            commandResponseOptionsRegistration.Register(services, registry.Descriptors);

            var generationResult = handlerTypeGenerator.Generate(registry.Descriptors);
            foreach (var descriptor in registry.Descriptors)
                Register(services, descriptor, generationResult.Find(descriptor).ImplementationType);
        }

        private void Register(IServiceCollection services, AutomationDescriptor descriptor, Type implementationType)
        {
            services.AddScoped(serviceTypeResolver.Resolve(descriptor), implementationType);
        }
    }
}
