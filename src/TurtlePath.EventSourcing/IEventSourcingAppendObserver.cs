namespace TurtlePath.EventSourcing
{
    /// <summary>
    /// Observes event envelopes appended by TurtlePath event sourcing hooks.
    /// </summary>
    public interface IEventSourcingAppendObserver
    {
        /// <summary>
        /// Runs after TurtlePath successfully appends event payloads to the event store.
        /// </summary>
        /// <param name="context">The append context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that completes when the observer has handled the append.</returns>
        ValueTask OnAppendedAsync(
            EventSourcingAppendContext context,
            CancellationToken cancellationToken = default);
    }
}
