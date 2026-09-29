namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares an update automation for a request type.
    /// </summary>
    public sealed class UpdateAutomationAttribute : MutationAutomationAttribute
    {
        /// <summary>
        /// Initializes an update automation that does not return a response.
        /// </summary>
        /// <param name="entityType">The entity type updated by the request.</param>
        public UpdateAutomationAttribute(Type entityType) : base(entityType, validateRequest: true)
        {
        }

        /// <summary>
        /// Initializes an update automation that returns a response.
        /// </summary>
        /// <param name="entityType">The entity type updated by the request.</param>
        /// <param name="responseType">The response type returned by the request.</param>
        public UpdateAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType, validateRequest: true)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Update;
    }
}
