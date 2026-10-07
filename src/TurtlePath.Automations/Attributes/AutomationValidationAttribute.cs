namespace TurtlePath.Automations.Attributes
{
    /// <summary>
    /// Overrides request validation for an attributed mutation automation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AutomationValidationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationValidationAttribute"/> class.
        /// </summary>
        /// <param name="enabled">Whether request validation should run.</param>
        public AutomationValidationAttribute(bool enabled)
        {
            Enabled = enabled;
        }

        /// <summary>
        /// Gets a value indicating whether request validation should run.
        /// </summary>
        public bool Enabled { get; }
    }
}
