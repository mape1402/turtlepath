namespace TurtlePath.Testing.Hooks
{
    using TurtlePath.Hooks;

    internal sealed class TraceResponseHook<TRequest, TEntity, TResponse>(HookTrace trace) :
        IBeforeResponseHook<TRequest, TEntity, TResponse>,
        IAfterResponseHook<TRequest, TEntity, TResponse>
    {
        public ValueTask BeforeResponseAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforeResponse", context);

        public ValueTask AfterResponseAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterResponse", context);

        private ValueTask AddAsync(string stage, CommandHookContext<TRequest, TEntity, TResponse> context)
        {
            trace.Add(new HookTraceEntry(
                stage,
                typeof(TRequest),
                typeof(TEntity),
                typeof(TResponse),
                context.Request,
                context.Entity,
                context.Response));

            return ValueTask.CompletedTask;
        }
    }
}
