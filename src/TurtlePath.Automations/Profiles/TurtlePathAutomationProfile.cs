namespace TurtlePath.Automations.Profiles
{
    /// <summary>
    /// Base class for declaring TurtlePath automated flows.
    /// </summary>
    public abstract class TurtlePathAutomationProfile
    {
        /// <summary>
        /// Configures automated handlers for a bounded application area.
        /// </summary>
        public abstract void Configure(ITurtlePathAutomationBuilder builder);
    }
}
