namespace TurtlePath.Hooks
{
    internal static class CommandHookStageRunner
    {
        public static ValueTask BeforeValidationAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforeValidationHook<TRequest, TEntity>>(
                hook => hook.BeforeValidationAsync(context, cancellationToken));

        public static ValueTask AfterValidationAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterValidationHook<TRequest, TEntity>>(
                hook => hook.AfterValidationAsync(context, cancellationToken));

        public static ValueTask BeforeGetEntityAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforeGetEntityHook<TRequest, TEntity>>(
                hook => hook.BeforeGetEntityAsync(context, cancellationToken));

        public static ValueTask AfterGetEntityAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterGetEntityHook<TRequest, TEntity>>(
                hook => hook.AfterGetEntityAsync(context, cancellationToken));

        public static ValueTask BeforeMapAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforeMapHook<TRequest, TEntity>>(
                hook => hook.BeforeMapAsync(context, cancellationToken));

        public static ValueTask AfterMapAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterMapHook<TRequest, TEntity>>(
                hook => hook.AfterMapAsync(context, cancellationToken));

        public static ValueTask BeforePatchAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforePatchHook<TRequest, TEntity>>(
                hook => hook.BeforePatchAsync(context, cancellationToken));

        public static ValueTask AfterPatchAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterPatchHook<TRequest, TEntity>>(
                hook => hook.AfterPatchAsync(context, cancellationToken));

        public static ValueTask BeforeSaveAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforeSaveHook<TRequest, TEntity>>(
                hook => hook.BeforeSaveAsync(context, cancellationToken));

        public static ValueTask AfterSaveAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterSaveHook<TRequest, TEntity>>(
                hook => hook.AfterSaveAsync(context, cancellationToken));

        public static ValueTask BeforeDeleteAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforeDeleteHook<TRequest, TEntity>>(
                hook => hook.BeforeDeleteAsync(context, cancellationToken));

        public static ValueTask AfterDeleteAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterDeleteHook<TRequest, TEntity>>(
                hook => hook.AfterDeleteAsync(context, cancellationToken));

        public static ValueTask BeforeResponseAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IBeforeResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.BeforeResponseAsync(context, cancellationToken));

        public static ValueTask AfterResponseAsync<TRequest, TEntity, TResponse>(
            IServiceProvider services,
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => services.RunHooksAsync<IAfterResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.AfterResponseAsync(context, cancellationToken));
    }
}
