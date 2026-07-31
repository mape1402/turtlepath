namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-one automation for a query type.
    /// </summary>
    public sealed class GetOneAutomationAttribute : AutomationAttribute
    {
        public GetOneAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetOne;
    }
}
