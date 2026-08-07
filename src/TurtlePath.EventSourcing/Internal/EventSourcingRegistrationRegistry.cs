namespace TurtlePath.EventSourcing.Internal
{
    internal sealed class EventSourcingRegistrationRegistry
    {
        private readonly Dictionary<(Type RequestType, Type EntityType), object> registrations = new();
        private readonly Dictionary<(Type RequestType, Type EntityType), object> streams = new();

        public void SetStream<TRequest, TEntity>(EventSourcingStreamConfiguration<TRequest, TEntity> stream)
            where TRequest : class
            where TEntity : class
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            streams[(typeof(TRequest), typeof(TEntity))] = stream;
        }

        public void Add<TRequest, TEntity>(EventSourcingRegistration<TRequest, TEntity> registration)
            where TRequest : class
            where TEntity : class
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            var key = (typeof(TRequest), typeof(TEntity));

            if (!registrations.TryGetValue(key, out var value))
            {
                value = new List<EventSourcingRegistration<TRequest, TEntity>>();
                registrations[key] = value;
            }

            ((List<EventSourcingRegistration<TRequest, TEntity>>)value).Add(registration);
        }

        public IReadOnlyCollection<EventSourcingRegistration<TRequest, TEntity>> Get<TRequest, TEntity>()
            where TRequest : class
            where TEntity : class
        {
            var key = (typeof(TRequest), typeof(TEntity));

            return registrations.TryGetValue(key, out var value)
                ? (IReadOnlyCollection<EventSourcingRegistration<TRequest, TEntity>>)value
                : Array.Empty<EventSourcingRegistration<TRequest, TEntity>>();
        }

        public EventSourcingStreamConfiguration<TRequest, TEntity> GetStream<TRequest, TEntity>()
            where TRequest : class
            where TEntity : class
        {
            var key = (typeof(TRequest), typeof(TEntity));

            return streams.TryGetValue(key, out var value)
                ? (EventSourcingStreamConfiguration<TRequest, TEntity>)value
                : null;
        }
    }
}
