namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a delete automation for a request type.
    /// </summary>
    public sealed class DeleteAutomationAttribute : AutomationAttribute
    {
        public DeleteAutomationAttribute(Type entityType) : base(entityType)
        {
        }

        public DeleteAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Delete;
    }
}
