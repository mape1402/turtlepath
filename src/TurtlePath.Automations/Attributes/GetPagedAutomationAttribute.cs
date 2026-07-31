namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-paged automation for a query type.
    /// </summary>
    public sealed class GetPagedAutomationAttribute : AutomationAttribute
    {
        public GetPagedAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetPaged;
    }
}
