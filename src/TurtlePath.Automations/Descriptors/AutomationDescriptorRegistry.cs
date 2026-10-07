namespace TurtlePath.Automations.Descriptors
{
    /// <summary>
    /// Collects automation descriptors and resolves precedence between declaration sources.
    /// </summary>
    public sealed class AutomationDescriptorRegistry
    {
        private readonly Dictionary<AutomationDescriptorKey, AutomationDescriptor> descriptors = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationDescriptorRegistry"/> class.
        /// </summary>
        public AutomationDescriptorRegistry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationDescriptorRegistry"/> class.
        /// </summary>
        /// <param name="descriptors">The descriptors to add to the registry.</param>
        public AutomationDescriptorRegistry(IEnumerable<AutomationDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            foreach (var descriptor in descriptors)
                Add(descriptor);
        }

        /// <summary>
        /// Gets the descriptors currently registered.
        /// </summary>
        public IReadOnlyCollection<AutomationDescriptor> Descriptors => descriptors.Values.ToArray();

        /// <summary>
        /// Adds a descriptor and applies source precedence rules.
        /// </summary>
        /// <param name="descriptor">The descriptor to add.</param>
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

        /// <summary>
        /// Finds a response-based descriptor for the supplied request and response type.
        /// </summary>
        /// <param name="requestType">The request type.</param>
        /// <param name="responseType">The response type.</param>
        /// <returns>The matching descriptor, or <c>null</c> when no descriptor exists.</returns>
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
