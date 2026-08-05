namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a delete automation for a request type.
    /// </summary>
    public sealed class DeleteAutomationAttribute : AutomationAttribute
    {
        /// <summary>
        /// Initializes a delete automation that does not return a response.
        /// </summary>
        /// <param name="entityType">The entity type deleted by the request.</param>
        public DeleteAutomationAttribute(Type entityType) : base(entityType)
        {
        }

        /// <summary>
        /// Initializes a delete automation that returns a response.
        /// </summary>
        /// <param name="entityType">The entity type deleted by the request.</param>
        /// <param name="responseType">The response type returned by the request.</param>
        public DeleteAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Delete;
    }
}
