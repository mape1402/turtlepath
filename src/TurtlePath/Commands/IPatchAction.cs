namespace TurtlePath.Commands
{
    /// <summary>
    /// Represents a request that can apply its own patch to an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to patch.</typeparam>
    public interface IPatchAction<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Applies the patch to the supplied entity.
        /// </summary>
        /// <param name="entity">The entity being patched.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask PatchAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
