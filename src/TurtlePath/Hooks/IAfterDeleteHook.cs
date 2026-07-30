namespace TurtlePath.Hooks
{
    /// <summary>
    /// Runs after an entity is deleted.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    public interface IAfterDeleteHook<TRequest, TEntity>
    {
        /// <summary>
        /// Executes after delete.
        /// </summary>
        /// <param name="context">The command hook context.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AfterDeleteAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default);
    }
}

