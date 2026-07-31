namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-many automation for a query type.
    /// </summary>
    public sealed class GetManyAutomationAttribute : AutomationAttribute
    {
        public GetManyAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetMany;
    }
}
