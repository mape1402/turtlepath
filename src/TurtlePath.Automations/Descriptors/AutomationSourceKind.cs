namespace TurtlePath.Automations.Descriptors
{
    /// <summary>
    /// Represents where an automation descriptor was declared.
    /// </summary>
    public enum AutomationSourceKind
    {
        /// <summary>
        /// The operation was declared with an attribute.
        /// </summary>
        Attribute,

        /// <summary>
        /// The operation was declared in an automation profile.
        /// </summary>
        Profile
    }
}
