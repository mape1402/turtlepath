namespace TurtlePath.Commands.Steps
{
    /// <summary>
    /// Commits changes for an existing entity.
    /// </summary>
    public interface IEntitySaveStep<TRequest, TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Commits entity changes.
        /// </summary>
        Task SaveAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }
}
