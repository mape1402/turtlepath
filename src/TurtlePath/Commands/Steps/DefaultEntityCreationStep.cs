namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Mapping;

    /// <summary>
    /// Default entity creation step using the configured mapper adapter.
    /// </summary>
    internal sealed class DefaultEntityCreationStep<TRequest, TEntity> : IEntityCreationStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        private readonly IMapperAdapter mapperAdapter;

        public DefaultEntityCreationStep(IMapperAdapter mapperAdapter)
        {
            this.mapperAdapter = mapperAdapter ?? throw new ArgumentNullException(nameof(mapperAdapter));
        }

        public ValueTask<TEntity> CreateAsync(TRequest request, CancellationToken cancellationToken)
            => mapperAdapter.MapAsync<TRequest, TEntity>(request, cancellationToken);
    }
}
