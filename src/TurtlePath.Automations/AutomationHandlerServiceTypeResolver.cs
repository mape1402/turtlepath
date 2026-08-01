namespace TurtlePath.Automations
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;

    internal sealed class AutomationHandlerServiceTypeResolver : IAutomationHandlerServiceTypeResolver
    {
        public Type Resolve(AutomationDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            return descriptor.HasResponse
                ? typeof(IRequestHandler<,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType)
                : typeof(IRequestHandler<>).MakeGenericType(descriptor.RequestType);
        }
    }
}
