namespace TurtlePath.EventSourcing
{
    /// <summary>
    /// Provides the source data used to map a TurtlePath command execution to a committed event.
    /// </summary>
    /// <typeparam name="TRequest">The command request type.</typeparam>
    /// <typeparam name="TEntity">The entity type affected by the command.</typeparam>
    public sealed class EventSourcingMapContext<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourcingMapContext{TRequest, TEntity}"/> class.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="entity">The entity affected by the command.</param>
        public EventSourcingMapContext(TRequest request, TEntity entity)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        /// <summary>
        /// Gets the command request.
        /// </summary>
        public TRequest Request { get; }

        /// <summary>
        /// Gets the entity affected by the command.
        /// </summary>
        public TEntity Entity { get; }
    }
}
