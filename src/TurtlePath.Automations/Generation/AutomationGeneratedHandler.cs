namespace TurtlePath.Automations.Generation
{
    using TurtlePath.Automations.Descriptors;

    internal sealed class AutomationGeneratedHandler
    {
        public AutomationGeneratedHandler(AutomationDescriptor descriptor, string typeName, Type implementationType)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            TypeName = string.IsNullOrWhiteSpace(typeName) ? throw new ArgumentException(nameof(typeName)) : typeName;
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }

        public AutomationDescriptor Descriptor { get; }

        public string TypeName { get; }

        public Type ImplementationType { get; }
    }
}
