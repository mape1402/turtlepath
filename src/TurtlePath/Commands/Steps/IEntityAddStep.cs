namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Persists a newly-created entity.
    /// </summary>
    public interface IEntityAddStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Adds the entity and commits changes.
        /// </summary>
        Task AddAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }
}
