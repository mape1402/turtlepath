namespace TurtlePath.Commands.Steps
{
    /// <summary>
    /// Creates an entity instance from a request.
    /// </summary>
    public interface IEntityCreationStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        /// <summary>
        /// Creates the entity.
        /// </summary>
        ValueTask<TEntity> CreateAsync(TRequest request, CancellationToken cancellationToken);
    }
}
