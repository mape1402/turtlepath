namespace TurtlePath.EventSourcing
{
    /// <summary>
    /// Configures the events emitted by a command/entity pair.
    /// </summary>
    /// <typeparam name="TRequest">The command request type.</typeparam>
    /// <typeparam name="TEntity">The entity type affected by the command.</typeparam>
    public interface IEventSourcingEntityBuilder<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        /// <summary>
        /// Resolves the event stream from the full TurtlePath command hook context.
        /// </summary>
        /// <param name="streamName">The logical stream name.</param>
        /// <param name="streamId">The stream id resolver.</param>
        /// <returns>The same command/entity event mapping builder.</returns>
        IEventSourcingEntityBuilder<TRequest, TEntity> UseStream(
            string streamName,
            Func<TurtlePath.Hooks.CommandHookContext<TRequest, TEntity>, string> streamId);

        /// <summary>
        /// Maps the command/entity pair to an event payload.
        /// </summary>
        /// <typeparam name="TEvent">The event payload type.</typeparam>
        /// <param name="configure">Optional event append configuration.</param>
        /// <returns>The same command/entity event mapping builder.</returns>
        IEventSourcingEntityBuilder<TRequest, TEntity> ToEvent<TEvent>(
            Action<EventSourcingEventOptions<TRequest, TEntity>> configure = null)
            where TEvent : class;

        /// <summary>
        /// Projects the command hook context to a mapper source and maps it to an event payload.
        /// </summary>
        /// <typeparam name="TSource">The mapper source type.</typeparam>
        /// <typeparam name="TEvent">The event payload type.</typeparam>
        /// <param name="source">The mapper source factory.</param>
        /// <param name="configure">Optional event append configuration.</param>
        /// <returns>The same command/entity event mapping builder.</returns>
        IEventSourcingEntityBuilder<TRequest, TEntity> ToEvent<TSource, TEvent>(
            Func<TurtlePath.Hooks.CommandHookContext<TRequest, TEntity>, TSource> source,
            Action<EventSourcingEventOptions<TRequest, TEntity>> configure = null)
            where TSource : class
            where TEvent : class;
    }
}
