namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Declares a patch automation for a request type.
    /// </summary>
    public sealed class PatchAutomationAttribute : AutomationAttribute
    {
        /// <summary>
        /// Initializes a patch automation that does not return a response.
        /// </summary>
        /// <param name="entityType">The entity type patched by the request.</param>
        public PatchAutomationAttribute(Type entityType) : base(entityType)
        {
        }

        /// <summary>
        /// Initializes a patch automation that returns a response.
        /// </summary>
        /// <param name="entityType">The entity type patched by the request.</param>
        /// <param name="responseType">The response type returned by the request.</param>
        public PatchAutomationAttribute(Type entityType, Type responseType) : base(entityType, responseType)
        {
        }

        internal override AutomationOperationKind OperationKind => AutomationOperationKind.Patch;
    }
}
