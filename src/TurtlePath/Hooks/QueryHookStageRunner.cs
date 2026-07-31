namespace TurtlePath.Hooks
{
    internal static class QueryHookStageRunner
    {
        public static ValueTask BeforeQueryAsync<TQuery, TResult>(
            IServiceProvider services,
            QueryHookContext<TQuery, TResult> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforeQueryHook<TQuery, TResult>>(
                hook => hook.BeforeQueryAsync(context, cancellationToken));

        public static ValueTask AfterQueryAsync<TQuery, TResult>(
            IServiceProvider services,
            QueryHookContext<TQuery, TResult> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterQueryHook<TQuery, TResult>>(
                hook => hook.AfterQueryAsync(context, cancellationToken));
    }
}
