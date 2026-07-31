namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Persistence;

    /// <summary>
    /// Default entity delete step using the configured storage writer adapter.
    /// </summary>
    internal sealed class DefaultEntityDeleteStep<TRequest, TEntity> : IEntityDeleteStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class, IEntity
    {
        private readonly IStorageWriterAdapter storageWriterAdapter;

        public DefaultEntityDeleteStep(IStorageWriterAdapter storageWriterAdapter)
        {
            this.storageWriterAdapter = storageWriterAdapter ?? throw new ArgumentNullException(nameof(storageWriterAdapter));
        }

        public async Task DeleteAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            storageWriterAdapter.Remove(entity);
            await storageWriterAdapter.SaveChangesAsync(cancellationToken);
        }
    }
}
