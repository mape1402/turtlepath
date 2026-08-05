namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a get-paged automation for a query type.
    /// </summary>
    public sealed class GetPagedAutomationAttribute : AutomationAttribute
    {
        /// <summary>
        /// Initializes a paged query automation.
        /// </summary>
        /// <param name="entityType">The entity type queried by the request.</param>
        /// <param name="responseType">The item response type returned by the request.</param>
        public GetPagedAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.GetPaged;
    }
}
