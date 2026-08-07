namespace TurtlePath.Testing.Mapping
{
    using TurtlePath.Mapping;

    /// <summary>
    /// Mapper adapter backed by explicitly registered delegates for tests.
    /// </summary>
    public sealed class DelegateMapperAdapter : IMapperAdapter
    {
        private readonly Dictionary<MapKey, Func<object, CancellationToken, ValueTask<object>>> maps = [];
        private readonly Dictionary<MapKey, Func<object, object, CancellationToken, ValueTask>> updateMaps = [];

        /// <summary>
        /// Registers a mapping delegate.
        /// </summary>
        public DelegateMapperAdapter WithMap<TSource, TDestination>(Func<TSource, TDestination> map)
            where TSource : class
            where TDestination : class
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            maps[new MapKey(typeof(TSource), typeof(TDestination))] = (source, _) =>
                ValueTask.FromResult<object>(map((TSource)source));

            return this;
        }

        /// <summary>
        /// Registers an async mapping delegate.
        /// </summary>
        public DelegateMapperAdapter WithMap<TSource, TDestination>(Func<TSource, CancellationToken, ValueTask<TDestination>> map)
            where TSource : class
            where TDestination : class
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            maps[new MapKey(typeof(TSource), typeof(TDestination))] = async (source, cancellationToken) =>
                await map((TSource)source, cancellationToken);

            return this;
        }

        /// <summary>
        /// Registers an update mapping delegate.
        /// </summary>
        public DelegateMapperAdapter WithUpdateMap<TSource, TDestination>(Action<TSource, TDestination> map)
            where TSource : class
            where TDestination : class
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            updateMaps[new MapKey(typeof(TSource), typeof(TDestination))] = (source, destination, _) =>
            {
                map((TSource)source, (TDestination)destination);
                return ValueTask.CompletedTask;
            };

            return this;
        }

        /// <inheritdoc />
        public async ValueTask<TDestination> MapAsync<TSource, TDestination>(
            TSource source,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source is TDestination destination)
                return destination;

            if (!maps.TryGetValue(new MapKey(typeof(TSource), typeof(TDestination)), out var map))
                throw new InvalidOperationException($"Mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} is not configured.");

            return (TDestination)await map(source, cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask UpdateMapAsync<TSource, TDestination>(
            TSource source,
            TDestination destination,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (!updateMaps.TryGetValue(new MapKey(typeof(TSource), typeof(TDestination)), out var map))
                throw new InvalidOperationException($"Update mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} is not configured.");

            return map(source, destination, cancellationToken);
        }

        private readonly record struct MapKey(Type Source, Type Destination);
    }
}
