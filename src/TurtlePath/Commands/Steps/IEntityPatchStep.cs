namespace TurtlePath.Commands.Steps
{
    /// <summary>
    /// Applies patch data from a request to an entity.
    /// </summary>
    public interface IEntityPatchStep<TRequest, TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Applies the patch.
        /// </summary>
        ValueTask PatchAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }
}
