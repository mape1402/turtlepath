namespace TurtlePath.Application.Hooks
{
    /// <summary>
    /// Runs before an entity is deleted.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    public interface IBeforeDeleteHook<TRequest, TEntity>
    {
        /// <summary>
        /// Executes before delete.
        /// </summary>
        /// <param name="context">The command hook context.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask BeforeDeleteAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default);
    }
}
