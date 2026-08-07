namespace TurtlePath.EventSourcing.Internal
{
    using Krackend.EventSourcing.Streams;
    using TurtlePath.Hooks;

    internal sealed class EventSourcingStreamConfiguration<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        public EventSourcingStreamConfiguration(
            Func<CommandHookContext<TRequest, TEntity>, EventStreamReference> resolve)
        {
            Resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
        }

        public Func<CommandHookContext<TRequest, TEntity>, EventStreamReference> Resolve { get; }
    }
}
