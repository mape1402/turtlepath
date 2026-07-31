namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Loads an entity for a command operation.
    /// </summary>
    public interface IEntityLookupStep<TRequest, TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Loads an entity by key.
        /// </summary>
        Task<TEntity> GetAsync(TRequest request, TKey key, CancellationToken cancellationToken);
    }
}
