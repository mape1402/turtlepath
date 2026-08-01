namespace TurtlePath.Automations.Generation
{
    using TurtlePath.Automations.Descriptors;

    internal sealed class DefaultAutomationHandlerTypeNamePolicy : IAutomationHandlerTypeNamePolicy
    {
        public string CreateName(AutomationDescriptor descriptor, int index)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            return $"Generated{descriptor.OperationKind}Handler_{Sanitize(descriptor.RequestType.Name)}_{Sanitize(descriptor.EntityType.Name)}_{index}";
        }

        private static string Sanitize(string value)
        {
            var chars = value.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray();
            return new string(chars);
        }
    }
}
