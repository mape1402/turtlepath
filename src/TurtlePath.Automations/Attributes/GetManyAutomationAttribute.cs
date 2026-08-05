namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-many automation for a query type.
    /// </summary>
    public sealed class GetManyAutomationAttribute : AutomationAttribute
    {
        /// <summary>
        /// Initializes a get-many automation.
        /// </summary>
        /// <param name="entityType">The entity type queried by the request.</param>
        /// <param name="responseType">The item response type returned by the request.</param>
        public GetManyAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetMany;
    }
}
