namespace TurtlePath.Testing.EventSourcing
{
    using System.Reflection;
    using Krackend.EventSourcing.Stores;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides event sourcing assertions for TurtlePath test hosts.
    /// </summary>
    public static class TurtlePathEventSourcingTestingExtensions
    {
        /// <summary>
        /// Reads events from the configured Krackend event store.
        /// </summary>
        public static async Task<IReadOnlyCollection<EventSourcingTestEvent>> ReadEventStreamAsync(
            this TurtlePathTestHost host,
            string streamName,
            string streamId,
            long fromVersion = 1,
            int maxCount = 100,
            CancellationToken cancellationToken = default)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var store = host.Resolve<IEventStore>();
            var envelopes = await store.ReadStreamAsync(streamName, streamId, fromVersion, maxCount, cancellationToken);

            return envelopes
                .Select(Project)
                .ToArray();
        }

        /// <summary>
        /// Returns true when the stream contains an event of the specified event type.
        /// </summary>
        public static async Task<bool> StreamContainsEventAsync(
            this TurtlePathTestHost host,
            string streamName,
            string streamId,
            string eventType,
            CancellationToken cancellationToken = default)
        {
            var events = await host.ReadEventStreamAsync(streamName, streamId, cancellationToken: cancellationToken);

            return events.Any(item => item.EventType == eventType);
        }

        private static EventSourcingTestEvent Project(object envelope)
        {
            return new EventSourcingTestEvent(
                GetValue<string>(envelope, "EventType"),
                GetValue<long>(envelope, "StreamVersion"),
                GetValue<string>(envelope, "Payload"),
                GetValue<IReadOnlyDictionary<string, object>>(envelope, "Metadata") ?? new Dictionary<string, object>());
        }

        private static TValue GetValue<TValue>(object source, string propertyName)
        {
            if (source == null)
                return default;

            var property = source
                .GetType()
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            if (property == null)
                return default;

            var value = property.GetValue(source);

            return value is TValue typedValue ? typedValue : default;
        }
    }
}
