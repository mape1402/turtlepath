namespace TurtlePath.Core.Hooks
{
    /// <summary>
    /// Runs after an existing entity is loaded for a command.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    public interface IAfterGetEntityHook<TRequest, TEntity>
    {
        /// <summary>
        /// Executes after entity retrieval.
        /// </summary>
        /// <param name="context">The command hook context.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AfterGetEntityAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default);
    }
}
