namespace TurtlePath.Automations.Options
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Commands;

    internal sealed class AutomationCommandResponseOptionsRegistration : IAutomationCommandResponseOptionsRegistration
    {
        public void Register(IServiceCollection services, IReadOnlyCollection<AutomationDescriptor> descriptors)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            foreach (var descriptor in descriptors)
                Register(services, descriptor);
        }

        private static void Register(IServiceCollection services, AutomationDescriptor descriptor)
        {
            if (!IsSupportedMutationWithResponse(descriptor))
                return;

            if (!descriptor.ReloadBeforeResponse)
                return;

            var serviceType = typeof(ICommandResponseOptions<,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType);
            var implementationType = typeof(DescriptorCommandResponseOptions<,,>).MakeGenericType(
                descriptor.RequestType,
                descriptor.EntityType,
                descriptor.KeyType);

            services.TryAdd(ServiceDescriptor.Scoped(
                serviceType,
                _ => Activator.CreateInstance(implementationType, descriptor)));
        }

        private static bool IsSupportedMutationWithResponse(AutomationDescriptor descriptor)
            => descriptor.HasResponse &&
                descriptor.OperationKind is AutomationOperationKind.Create or AutomationOperationKind.Update or AutomationOperationKind.Patch;
    }
}
