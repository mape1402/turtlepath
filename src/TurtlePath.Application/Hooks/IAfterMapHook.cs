namespace TurtlePath.Core.Hooks
{
    /// <summary>
    /// Runs after a command request is mapped to or onto an entity.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    public interface IAfterMapHook<TRequest, TEntity>
    {
        /// <summary>
        /// Executes after mapping.
        /// </summary>
        /// <param name="context">The command hook context.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AfterMapAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default);
    }
}
