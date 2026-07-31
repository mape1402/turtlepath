namespace TurtlePath.OctoMap
{
    using global::OctoMap;
    using TurtlePath.Mapping;

    /// <summary>
    /// Provides an implementation of <see cref="IMapperAdapter"/> using OctoMap for object mapping.
    /// </summary>
    public sealed class OctoMapAdapter : IMapperAdapter
    {
        private readonly IOctoMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OctoMapAdapter"/> class.
        /// </summary>
        /// <param name="mapper">The OctoMap instance to use for mapping.</param>
        public OctoMapAdapter(IOctoMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Asynchronously maps the source object to a new destination object of the specified type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <param name="source">The source object to map.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the mapped destination object as the result.</returns>
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => ValueTask.FromResult(_mapper.Map<TSource, TDestination>(source));

        /// <summary>
        /// Asynchronously maps the source object onto an existing destination object, updating its values.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <param name="source">The source object to map from.</param>
        /// <param name="destination">The destination object to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public ValueTask UpdateMapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            _mapper.Map(source, destination);
            return ValueTask.CompletedTask;
        }
    }
}
