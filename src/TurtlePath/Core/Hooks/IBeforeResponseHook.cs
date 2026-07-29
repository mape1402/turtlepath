namespace TurtlePath.Core.Hooks
{
    /// <summary>
    /// Runs before a command response is built.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    /// <typeparam name="TResponse">The type of the command response.</typeparam>
    public interface IBeforeResponseHook<TRequest, TEntity, TResponse>
    {
        /// <summary>
        /// Executes before response mapping or construction.
        /// </summary>
        /// <param name="context">The command hook context.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask BeforeResponseAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken = default);
    }
}
