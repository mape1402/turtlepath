namespace TurtlePath.Core.Hooks
{
    /// <summary>
    /// Runs after a query is executed.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query request.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public interface IAfterQueryHook<TQuery, TResult>
    {
        /// <summary>
        /// Executes after the query is executed.
        /// </summary>
        /// <param name="context">The query hook context.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AfterQueryAsync(QueryHookContext<TQuery, TResult> context, CancellationToken cancellationToken = default);
    }
}
