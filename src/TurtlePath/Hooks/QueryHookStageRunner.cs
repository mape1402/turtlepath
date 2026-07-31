namespace TurtlePath.Hooks
{
    internal interface IQueryHookStageRunner<TQuery, TResult>
    {
        ValueTask BeforeQueryAsync(QueryHookContext<TQuery, TResult> context, CancellationToken cancellationToken);

        ValueTask AfterQueryAsync(QueryHookContext<TQuery, TResult> context, CancellationToken cancellationToken);
    }

    internal sealed class QueryHookStageRunner<TQuery, TResult> : IQueryHookStageRunner<TQuery, TResult>
    {
        private readonly IHandlerHookRunner hookRunner;

        public QueryHookStageRunner(IHandlerHookRunner hookRunner)
        {
            this.hookRunner = hookRunner ?? throw new ArgumentNullException(nameof(hookRunner));
        }

        public ValueTask BeforeQueryAsync(
            QueryHookContext<TQuery, TResult> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeQueryHook<TQuery, TResult>>(
                hook => hook.BeforeQueryAsync(context, cancellationToken));

        public ValueTask AfterQueryAsync(
            QueryHookContext<TQuery, TResult> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterQueryHook<TQuery, TResult>>(
                hook => hook.AfterQueryAsync(context, cancellationToken));
    }
}
