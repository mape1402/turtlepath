namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-one automation for a query type.
    /// </summary>
    public sealed class GetOneAutomationAttribute : AutomationAttribute
    {
        /// <summary>
        /// Initializes a get-one automation.
        /// </summary>
        /// <param name="entityType">The entity type queried by the request.</param>
        /// <param name="responseType">The response type returned by the request.</param>
        public GetOneAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetOne;
    }
}
