namespace TurtlePath.Automations.Generation
{
    using TurtlePath.Automations.Descriptors;

    internal sealed class AutomationHandlerGenerationResult
    {
        private readonly Dictionary<AutomationDescriptor, AutomationGeneratedHandler> handlers;

        public AutomationHandlerGenerationResult(IEnumerable<AutomationGeneratedHandler> handlers)
        {
            if (handlers == null)
                throw new ArgumentNullException(nameof(handlers));

            this.handlers = handlers.ToDictionary(handler => handler.Descriptor);
        }

        public IReadOnlyCollection<AutomationGeneratedHandler> Handlers => handlers.Values.ToArray();

        public AutomationGeneratedHandler Find(AutomationDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            return handlers[descriptor];
        }
    }
}
