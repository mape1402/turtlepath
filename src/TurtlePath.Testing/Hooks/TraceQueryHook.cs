namespace TurtlePath.Testing.Hooks
{
    using TurtlePath.Hooks;

    internal sealed class TraceQueryHook<TQuery, TResult>(HookTrace trace) :
        IBeforeQueryHook<TQuery, TResult>,
        IAfterQueryHook<TQuery, TResult>
    {
        public ValueTask BeforeQueryAsync(QueryHookContext<TQuery, TResult> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforeQuery", context);

        public ValueTask AfterQueryAsync(QueryHookContext<TQuery, TResult> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterQuery", context);

        private ValueTask AddAsync(string stage, QueryHookContext<TQuery, TResult> context)
        {
            trace.Add(new HookTraceEntry(
                stage,
                typeof(TQuery),
                null,
                typeof(TResult),
                context.Query,
                null,
                context.Result));

            return ValueTask.CompletedTask;
        }
    }
}
