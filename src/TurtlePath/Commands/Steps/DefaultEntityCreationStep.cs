namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Mapping;

    /// <summary>
    /// Default entity creation step using the configured mapper adapter.
    /// </summary>
    internal sealed class DefaultEntityCreationStep<TRequest, TEntity> : IEntityCreationStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        private readonly IMapperAdapter mapperAdapter;
        private readonly ICIdDefinitionRegistry cIdDefinitionRegistry;

        public DefaultEntityCreationStep(IMapperAdapter mapperAdapter, ICIdDefinitionRegistry cIdDefinitionRegistry = null)
        {
            this.mapperAdapter = mapperAdapter ?? throw new ArgumentNullException(nameof(mapperAdapter));
            this.cIdDefinitionRegistry = cIdDefinitionRegistry;
        }

        public async ValueTask<TEntity> CreateAsync(TRequest request, CancellationToken cancellationToken)
        {
            var entity = await mapperAdapter.MapAsync<TRequest, TEntity>(request, cancellationToken);

            AssignClientGeneratedCId(entity);

            return entity;
        }

        private void AssignClientGeneratedCId(TEntity entity)
        {
            if (cIdDefinitionRegistry == null ||
                entity is not IEntity<CId> identifiedEntity ||
                !identifiedEntity.Id.IsEmpty)
                return;

            if (!cIdDefinitionRegistry.TryGet(typeof(TEntity), CIdDefinition.DefaultPropertyName, out var definition))
                return;

            if (definition.GenerationStrategy != CIdGenerationStrategy.ClientGenerated)
                return;

            identifiedEntity.Id = definition.Factory();
        }
    }
}
