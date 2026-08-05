namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-by-id automation for a query type.
    /// </summary>
    public sealed class GetByIdAutomationAttribute : AutomationAttribute
    {
        /// <summary>
        /// Initializes a get-by-id automation.
        /// </summary>
        /// <param name="entityType">The entity type queried by the request.</param>
        /// <param name="responseType">The response type returned by the request.</param>
        public GetByIdAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetById;
    }
}
