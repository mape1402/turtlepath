namespace TurtlePath.Automations.Descriptors
{
    using TurtlePath.Domain.Contracts;

    internal static class AutomationDescriptorValidator
    {
        public static void Validate(AutomationDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.ReturnMode == AutomationReturnMode.Response && descriptor.ResponseType == null)
                throw new ArgumentException("Response automations require a response type.", nameof(descriptor));

            if (descriptor.ReturnMode == AutomationReturnMode.None && descriptor.ResponseType != null)
                throw new ArgumentException("No-response automations cannot declare a response type.", nameof(descriptor));

            var entityContract = typeof(IEntity<>).MakeGenericType(descriptor.KeyType);
            if (!entityContract.IsAssignableFrom(descriptor.EntityType))
                throw new ArgumentException(
                    $"Entity type '{descriptor.EntityType.FullName}' must implement '{entityContract.FullName}'.",
                    nameof(descriptor));

            if (descriptor.OperationKind is AutomationOperationKind.GetById or AutomationOperationKind.GetOne or AutomationOperationKind.GetMany or AutomationOperationKind.GetPaged &&
                descriptor.ReturnMode != AutomationReturnMode.Response)
            {
                throw new ArgumentException("Query automations require a response type.", nameof(descriptor));
            }
        }
    }
}
