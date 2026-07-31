namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Exceptions;
    using TurtlePath.Persistence;

    /// <summary>
    /// Default entity lookup step using the configured storage reader adapter.
    /// </summary>
    internal sealed class DefaultEntityLookupStep<TRequest, TEntity, TKey> : IEntityLookupStep<TRequest, TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        private readonly IStorageReaderAdapter storageReaderAdapter;

        public DefaultEntityLookupStep(IStorageReaderAdapter storageReaderAdapter)
        {
            this.storageReaderAdapter = storageReaderAdapter ?? throw new ArgumentNullException(nameof(storageReaderAdapter));
        }

        public async Task<TEntity> GetAsync(TRequest request, TKey key, CancellationToken cancellationToken)
        {
            return await storageReaderAdapter
                .For<TEntity>()
                .AsTracking()
                .Where(EntityKeyExpression.Equals<TEntity, TKey>(key))
                .FirstOrDefaultAsync<TEntity>(cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, key?.ToString());
        }
    }
}
