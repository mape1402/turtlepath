namespace TurtlePath.Commands.Steps
{
    using System.Linq.Expressions;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Mapping;
    using TurtlePath.Persistence;

    /// <summary>
    /// Default response mapping step using mapper or storage projection.
    /// </summary>
    internal sealed class DefaultResponseMappingStep<TRequest, TEntity, TResponse, TKey> : IResponseMappingStep<TRequest, TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class
    {
        private readonly IMapperAdapter mapperAdapter;
        private readonly IStorageReaderAdapter storageReaderAdapter;

        public DefaultResponseMappingStep(IMapperAdapter mapperAdapter, IStorageReaderAdapter storageReaderAdapter)
        {
            this.mapperAdapter = mapperAdapter ?? throw new ArgumentNullException(nameof(mapperAdapter));
            this.storageReaderAdapter = storageReaderAdapter ?? throw new ArgumentNullException(nameof(storageReaderAdapter));
        }

        public async ValueTask<TResponse> MapAsync(
            TRequest request,
            TEntity entity,
            bool useProjectionFromStorage,
            Expression<Func<TEntity, bool>> projectionFilter,
            CancellationToken cancellationToken)
        {
            if (!useProjectionFromStorage)
                return await mapperAdapter.MapAsync<TEntity, TResponse>(entity, cancellationToken);

            return await storageReaderAdapter
                .For<TEntity>()
                .AsNoTracking()
                .Where(projectionFilter)
                .FirstOrDefaultAsync<TResponse>(cancellationToken);
        }
    }
}
