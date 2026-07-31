namespace TurtlePath.Automations.Descriptors
{
    /// <summary>
    /// Collects automation descriptors and resolves precedence between declaration sources.
    /// </summary>
    internal sealed class AutomationDescriptorRegistry
    {
        private readonly Dictionary<AutomationDescriptorKey, AutomationDescriptor> descriptors = new();

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
    }
}
