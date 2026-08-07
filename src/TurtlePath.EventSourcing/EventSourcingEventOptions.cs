namespace TurtlePath.EventSourcing
{
    using Krackend.EventSourcing.Stores;
    using TurtlePath.Hooks;

    /// <summary>
    /// Configures how a command/entity pair appends a mapped event.
    /// </summary>
    /// <typeparam name="TRequest">The command request type.</typeparam>
    /// <typeparam name="TEntity">The entity type affected by the command.</typeparam>
    public sealed class EventSourcingEventOptions<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        /// <summary>
        /// Gets or sets the append precondition used for this event.
        /// </summary>
        public Func<CommandHookContext<TRequest, TEntity>, ExpectedVersion> ExpectedVersion { get; set; }
            = _ => Krackend.EventSourcing.Stores.ExpectedVersion.Any;

        /// <summary>
        /// Gets or sets a predicate that decides whether this event should be appended.
        /// </summary>
        public Func<CommandHookContext<TRequest, TEntity>, bool> ShouldAppend { get; set; } = _ => true;

        /// <summary>
        /// Uses the same expected version for every append of this event.
        /// </summary>
        /// <param name="expectedVersion">The expected stream version.</param>
        /// <returns>The same options instance.</returns>
        public EventSourcingEventOptions<TRequest, TEntity> UseExpectedVersion(ExpectedVersion expectedVersion)
        {
            ExpectedVersion = _ => expectedVersion;
            return this;
        }

        /// <summary>
        /// Appends this event only when the predicate evaluates to true.
        /// </summary>
        /// <param name="predicate">The append predicate.</param>
        /// <returns>The same options instance.</returns>
        public EventSourcingEventOptions<TRequest, TEntity> When(Func<CommandHookContext<TRequest, TEntity>, bool> predicate)
        {
            ShouldAppend = predicate ?? throw new ArgumentNullException(nameof(predicate));
            return this;
        }
    }
}
