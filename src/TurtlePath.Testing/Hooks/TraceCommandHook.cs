namespace TurtlePath.Testing.Hooks
{
    using TurtlePath.Hooks;

    internal sealed class TraceCommandHook<TRequest, TEntity>(HookTrace trace) :
        IBeforeValidationHook<TRequest, TEntity>,
        IAfterValidationHook<TRequest, TEntity>,
        IBeforeGetEntityHook<TRequest, TEntity>,
        IAfterGetEntityHook<TRequest, TEntity>,
        IBeforeMapHook<TRequest, TEntity>,
        IAfterMapHook<TRequest, TEntity>,
        IBeforePatchHook<TRequest, TEntity>,
        IAfterPatchHook<TRequest, TEntity>,
        IBeforeSaveHook<TRequest, TEntity>,
        IAfterSaveHook<TRequest, TEntity>,
        IBeforeDeleteHook<TRequest, TEntity>,
        IAfterDeleteHook<TRequest, TEntity>
    {
        public ValueTask BeforeValidationAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforeValidation", context);

        public ValueTask AfterValidationAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterValidation", context);

        public ValueTask BeforeGetEntityAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforeGetEntity", context);

        public ValueTask AfterGetEntityAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterGetEntity", context);

        public ValueTask BeforeMapAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforeMap", context);

        public ValueTask AfterMapAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterMap", context);

        public ValueTask BeforePatchAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforePatch", context);

        public ValueTask AfterPatchAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterPatch", context);

        public ValueTask BeforeSaveAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforeSave", context);

        public ValueTask AfterSaveAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterSave", context);

        public ValueTask BeforeDeleteAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("BeforeDelete", context);

        public ValueTask AfterDeleteAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("AfterDelete", context);

        private ValueTask AddAsync(string stage, CommandHookContext<TRequest, TEntity> context)
        {
            trace.Add(new HookTraceEntry(
                stage,
                typeof(TRequest),
                typeof(TEntity),
                null,
                context.Request,
                context.Entity,
                null));

            return ValueTask.CompletedTask;
        }
    }
}
