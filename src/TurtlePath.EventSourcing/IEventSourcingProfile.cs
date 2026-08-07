namespace TurtlePath.EventSourcing
{
    /// <summary>
    /// Configures event sourcing mappings for TurtlePath command handlers.
    /// </summary>
    public interface IEventSourcingProfile
    {
        /// <summary>
        /// Adds event mappings to the supplied builder.
        /// </summary>
        /// <param name="builder">The event sourcing profile builder.</param>
        void Configure(IEventSourcingProfileBuilder builder);
    }
}
