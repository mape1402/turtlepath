namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-by-id automation for a query type.
    /// </summary>
    public sealed class GetByIdAutomationAttribute : AutomationAttribute
    {
        public GetByIdAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetById;
    }
}
