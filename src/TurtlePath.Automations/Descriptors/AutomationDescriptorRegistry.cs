namespace TurtlePath.Automations.Descriptors
{
    /// <summary>
    /// Collects automation descriptors and resolves precedence between declaration sources.
    /// </summary>
    internal sealed class AutomationDescriptorRegistry
    {
        private readonly Dictionary<AutomationDescriptorKey, AutomationDescriptor> descriptors = new();

        public AutomationDescriptorRegistry()
        {
        }

        public AutomationDescriptorRegistry(IEnumerable<AutomationDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            foreach (var descriptor in descriptors)
                Add(descriptor);
        }

        public IReadOnlyCollection<AutomationDescriptor> Descriptors => descriptors.Values.ToArray();

        public void Add(AutomationDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            AutomationDescriptorValidator.Validate(descriptor);

            if (!descriptors.TryGetValue(descriptor.Key, out var current))
            {
                descriptors.Add(descriptor.Key, descriptor);
                return;
            }

            if (current.IsEquivalentTo(descriptor))
                return;

            if (descriptor.SourcePriority > current.SourcePriority)
            {
                descriptors[descriptor.Key] = descriptor;
                return;
            }

            if (descriptor.SourcePriority < current.SourcePriority)
                return;

            throw new AutomationDescriptorConflictException(current, descriptor);
        }

        public AutomationDescriptor Find(Type requestType, Type responseType)
        {
            if (requestType == null)
                throw new ArgumentNullException(nameof(requestType));

            if (responseType == null)
                throw new ArgumentNullException(nameof(responseType));

            return descriptors.Values.FirstOrDefault(descriptor =>
                descriptor.RequestType == requestType &&
                descriptor.ResponseType == responseType);
        }
    }
}
