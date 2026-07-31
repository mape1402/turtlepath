namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares an update automation for a request type.
    /// </summary>
    public sealed class UpdateAutomationAttribute : AutomationAttribute
    {
        public UpdateAutomationAttribute(Type entityType) : base(entityType)
        {
        }

        public UpdateAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Update;
    }
}
