namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Persistence;

    /// <summary>
    /// Default entity save step using the configured storage writer adapter.
    /// </summary>
    internal sealed class DefaultEntitySaveStep<TRequest, TEntity> : IEntitySaveStep<TRequest, TEntity>
        where TEntity : class
    {
        private readonly IStorageWriterAdapter storageWriterAdapter;

        public DefaultEntitySaveStep(IStorageWriterAdapter storageWriterAdapter)
        {
            this.storageWriterAdapter = storageWriterAdapter ?? throw new ArgumentNullException(nameof(storageWriterAdapter));
        }

        public Task SaveAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => storageWriterAdapter.SaveChangesAsync(cancellationToken);
    }
}
