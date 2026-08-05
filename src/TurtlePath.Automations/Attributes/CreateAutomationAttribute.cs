namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a create automation for a request type.
    /// </summary>
    public sealed class CreateAutomationAttribute : AutomationAttribute
    {
        /// <summary>
        /// Initializes a create automation that does not return a response.
        /// </summary>
        /// <param name="entityType">The entity type created by the request.</param>
        public CreateAutomationAttribute(Type entityType) : base(entityType)
        {
        }

        /// <summary>
        /// Initializes a create automation that returns a response.
        /// </summary>
        /// <param name="entityType">The entity type created by the request.</param>
        /// <param name="responseType">The response type returned by the request.</param>
        public CreateAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Create;
    }
}
