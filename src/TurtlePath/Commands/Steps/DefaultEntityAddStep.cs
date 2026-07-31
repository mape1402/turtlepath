namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Persistence;

    /// <summary>
    /// Default entity add step using the configured storage writer adapter.
    /// </summary>
    internal sealed class DefaultEntityAddStep<TRequest, TEntity> : IEntityAddStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class, IEntity
    {
        private readonly IStorageWriterAdapter storageWriterAdapter;

        public DefaultEntityAddStep(IStorageWriterAdapter storageWriterAdapter)
        {
            this.storageWriterAdapter = storageWriterAdapter ?? throw new ArgumentNullException(nameof(storageWriterAdapter));
        }

        public async Task AddAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            await storageWriterAdapter.AddAsync(entity, cancellationToken);
            await storageWriterAdapter.SaveChangesAsync(cancellationToken);
        }
    }
}
