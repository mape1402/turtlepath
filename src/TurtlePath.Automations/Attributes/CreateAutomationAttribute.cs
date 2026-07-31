namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a create automation for a request type.
    /// </summary>
    public sealed class CreateAutomationAttribute : AutomationAttribute
    {
        public CreateAutomationAttribute(Type entityType) : base(entityType)
        {
        }

        public CreateAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Create;
    }
}
