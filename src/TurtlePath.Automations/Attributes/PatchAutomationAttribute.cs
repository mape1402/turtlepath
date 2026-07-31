namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a patch automation for a request type.
    /// </summary>
    public sealed class PatchAutomationAttribute : AutomationAttribute
    {
        public PatchAutomationAttribute(Type entityType) : base(entityType)
        {
        }

        public PatchAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Patch;
    }
}
