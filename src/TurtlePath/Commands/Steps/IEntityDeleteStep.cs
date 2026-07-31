namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Deletes an entity and commits changes.
    /// </summary>
    public interface IEntityDeleteStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Deletes the entity.
        /// </summary>
        Task DeleteAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }
}
