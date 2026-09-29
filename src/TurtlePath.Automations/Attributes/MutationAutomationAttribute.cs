namespace TurtlePath.Automations.Attributes
{
    /// <summary>
    /// Base attribute for TurtlePath mutation automation declarations.
    /// </summary>
    public abstract class MutationAutomationAttribute : AutomationAttribute
    {
        private protected MutationAutomationAttribute(
            Type entityType,
            Type responseType = null,
            bool validateRequest = false) : base(entityType, responseType)
        {
            ValidateRequest = validateRequest;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the generated handler validates the request.
        /// </summary>
        public bool ValidateRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether responses should be read again from storage before mapping.
        /// </summary>
        public bool ReloadBeforeResponse { get; set; }
    }
}
