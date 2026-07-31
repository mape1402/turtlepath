namespace TurtlePath.AutoMapper
{
    using global::AutoMapper;
    using TurtlePath.Mapping;

    /// <summary>
    /// Provides an implementation of <see cref="IMapperAdapter"/> using AutoMapper for object mapping.
    /// </summary>
    public class MapperAdapter : IMapperAdapter
    {
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperAdapter"/> class.
        /// </summary>
        /// <param name="mapper">The AutoMapper instance to use for mapping.</param>
        public MapperAdapter(IMapper mapper)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => ValueTask.FromResult(mapper.Map<TDestination>(source));

        /// <inheritdoc/>
        public ValueTask UpdateMapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            mapper.Map(source, destination);
            return ValueTask.CompletedTask;
        }
    }
}
