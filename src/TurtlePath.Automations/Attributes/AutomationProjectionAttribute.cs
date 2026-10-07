namespace TurtlePath.Automations.Attributes
{
    /// <summary>
    /// Overrides response projection from storage for an attributed mutation automation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AutomationProjectionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationProjectionAttribute"/> class.
        /// </summary>
        /// <param name="enabled">Whether the response should be read from storage before mapping.</param>
        public AutomationProjectionAttribute(bool enabled)
        {
            Enabled = enabled;
        }

        /// <summary>
        /// Gets a value indicating whether the response should be read from storage before mapping.
        /// </summary>
        public bool Enabled { get; }
    }
}
