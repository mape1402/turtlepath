namespace TurtlePath.EventSourcing
{
    /// <summary>
    /// Builds event sourcing mappings for TurtlePath command handlers.
    /// </summary>
    public interface IEventSourcingProfileBuilder
    {
        /// <summary>
        /// Starts configuration for a command/entity pair.
        /// </summary>
        /// <typeparam name="TRequest">The command request type.</typeparam>
        /// <typeparam name="TEntity">The entity type affected by the command.</typeparam>
        /// <returns>A command/entity event mapping builder.</returns>
        IEventSourcingEntityBuilder<TRequest, TEntity> For<TRequest, TEntity>()
            where TRequest : class
            where TEntity : class;
    }
}
