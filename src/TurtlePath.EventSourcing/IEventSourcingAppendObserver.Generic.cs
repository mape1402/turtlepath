namespace TurtlePath.EventSourcing
{
    /// <summary>
    /// Observes event envelopes appended by TurtlePath event sourcing hooks for a specific command/entity pair.
    /// </summary>
    /// <typeparam name="TRequest">The command request type.</typeparam>
    /// <typeparam name="TEntity">The entity type affected by the command.</typeparam>
    public interface IEventSourcingAppendObserver<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        /// <summary>
        /// Runs after TurtlePath successfully appends event payloads to the event store.
        /// </summary>
        /// <param name="context">The typed append context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that completes when the observer has handled the append.</returns>
        ValueTask OnAppendedAsync(
            EventSourcingAppendContext<TRequest, TEntity> context,
            CancellationToken cancellationToken = default);
    }
}
