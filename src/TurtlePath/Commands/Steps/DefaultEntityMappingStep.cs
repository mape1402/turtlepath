namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Mapping;

    /// <summary>
    /// Default entity mapping step using the configured mapper adapter.
    /// </summary>
    internal sealed class DefaultEntityMappingStep<TRequest, TEntity> : IEntityMappingStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        private readonly IMapperAdapter mapperAdapter;

        public DefaultEntityMappingStep(IMapperAdapter mapperAdapter)
        {
            this.mapperAdapter = mapperAdapter ?? throw new ArgumentNullException(nameof(mapperAdapter));
        }

        public ValueTask MapAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => mapperAdapter.UpdateMapAsync(request, entity, cancellationToken);
    }
}
