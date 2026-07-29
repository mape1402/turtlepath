namespace TurtlePath.Application.Hooks
{
    /// <summary>
    /// Runs after a command response is built.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    /// <typeparam name="TResponse">The type of the command response.</typeparam>
    public interface IAfterResponseHook<TRequest, TEntity, TResponse>
    {
        /// <summary>
        /// Executes after response mapping or construction.
        /// </summary>
        /// <param name="context">The command hook context.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AfterResponseAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken = default);
    }
}
