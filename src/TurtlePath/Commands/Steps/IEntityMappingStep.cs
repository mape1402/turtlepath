namespace TurtlePath.Commands.Steps
{
    /// <summary>
    /// Applies request values to an existing entity.
    /// </summary>
    public interface IEntityMappingStep<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        /// <summary>
        /// Applies request values to the entity.
        /// </summary>
        ValueTask MapAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }
}
