namespace TurtlePath.Hooks
{
    internal interface ICommandHookStageRunner<TRequest, TEntity>
    {
        ValueTask BeforeValidationAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask AfterValidationAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask BeforeGetEntityAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask AfterGetEntityAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask BeforeMapAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask AfterMapAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask BeforePatchAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask AfterPatchAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask BeforeSaveAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask AfterSaveAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask BeforeDeleteAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);

        ValueTask AfterDeleteAsync(CommandHookContext<TRequest, TEntity> context, CancellationToken cancellationToken);
    }

    internal sealed class CommandHookStageRunner<TRequest, TEntity> : ICommandHookStageRunner<TRequest, TEntity>
    {
        private readonly IHandlerHookRunner hookRunner;

        public CommandHookStageRunner(IHandlerHookRunner hookRunner)
        {
            this.hookRunner = hookRunner ?? throw new ArgumentNullException(nameof(hookRunner));
        }

        public ValueTask BeforeValidationAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeValidationHook<TRequest, TEntity>>(
                hook => hook.BeforeValidationAsync(context, cancellationToken));

        public ValueTask AfterValidationAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterValidationHook<TRequest, TEntity>>(
                hook => hook.AfterValidationAsync(context, cancellationToken));

        public ValueTask BeforeGetEntityAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeGetEntityHook<TRequest, TEntity>>(
                hook => hook.BeforeGetEntityAsync(context, cancellationToken));

        public ValueTask AfterGetEntityAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterGetEntityHook<TRequest, TEntity>>(
                hook => hook.AfterGetEntityAsync(context, cancellationToken));

        public ValueTask BeforeMapAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeMapHook<TRequest, TEntity>>(
                hook => hook.BeforeMapAsync(context, cancellationToken));

        public ValueTask AfterMapAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterMapHook<TRequest, TEntity>>(
                hook => hook.AfterMapAsync(context, cancellationToken));

        public ValueTask BeforePatchAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforePatchHook<TRequest, TEntity>>(
                hook => hook.BeforePatchAsync(context, cancellationToken));

        public ValueTask AfterPatchAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterPatchHook<TRequest, TEntity>>(
                hook => hook.AfterPatchAsync(context, cancellationToken));

        public ValueTask BeforeSaveAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeSaveHook<TRequest, TEntity>>(
                hook => hook.BeforeSaveAsync(context, cancellationToken));

        public ValueTask AfterSaveAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterSaveHook<TRequest, TEntity>>(
                hook => hook.AfterSaveAsync(context, cancellationToken));

        public ValueTask BeforeDeleteAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeDeleteHook<TRequest, TEntity>>(
                hook => hook.BeforeDeleteAsync(context, cancellationToken));

        public ValueTask AfterDeleteAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterDeleteHook<TRequest, TEntity>>(
                hook => hook.AfterDeleteAsync(context, cancellationToken));
    }

    internal interface ICommandHookStageRunner<TRequest, TEntity, TResponse>
    {
        ValueTask BeforeValidationAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask AfterValidationAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask BeforeGetEntityAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask AfterGetEntityAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask BeforeMapAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask AfterMapAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask BeforePatchAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask AfterPatchAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask BeforeSaveAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask AfterSaveAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask BeforeDeleteAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask AfterDeleteAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask BeforeResponseAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);

        ValueTask AfterResponseAsync(CommandHookContext<TRequest, TEntity, TResponse> context, CancellationToken cancellationToken);
    }

    internal sealed class CommandHookStageRunner<TRequest, TEntity, TResponse> : ICommandHookStageRunner<TRequest, TEntity, TResponse>
    {
        private readonly IHandlerHookRunner hookRunner;

        public CommandHookStageRunner(IHandlerHookRunner hookRunner)
        {
            this.hookRunner = hookRunner ?? throw new ArgumentNullException(nameof(hookRunner));
        }

        public ValueTask BeforeValidationAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeValidationHook<TRequest, TEntity>>(
                hook => hook.BeforeValidationAsync(context, cancellationToken));

        public ValueTask AfterValidationAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterValidationHook<TRequest, TEntity>>(
                hook => hook.AfterValidationAsync(context, cancellationToken));

        public ValueTask BeforeGetEntityAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeGetEntityHook<TRequest, TEntity>>(
                hook => hook.BeforeGetEntityAsync(context, cancellationToken));

        public ValueTask AfterGetEntityAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterGetEntityHook<TRequest, TEntity>>(
                hook => hook.AfterGetEntityAsync(context, cancellationToken));

        public ValueTask BeforeMapAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeMapHook<TRequest, TEntity>>(
                hook => hook.BeforeMapAsync(context, cancellationToken));

        public ValueTask AfterMapAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterMapHook<TRequest, TEntity>>(
                hook => hook.AfterMapAsync(context, cancellationToken));

        public ValueTask BeforePatchAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforePatchHook<TRequest, TEntity>>(
                hook => hook.BeforePatchAsync(context, cancellationToken));

        public ValueTask AfterPatchAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterPatchHook<TRequest, TEntity>>(
                hook => hook.AfterPatchAsync(context, cancellationToken));

        public ValueTask BeforeSaveAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeSaveHook<TRequest, TEntity>>(
                hook => hook.BeforeSaveAsync(context, cancellationToken));

        public ValueTask AfterSaveAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterSaveHook<TRequest, TEntity>>(
                hook => hook.AfterSaveAsync(context, cancellationToken));

        public ValueTask BeforeDeleteAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeDeleteHook<TRequest, TEntity>>(
                hook => hook.BeforeDeleteAsync(context, cancellationToken));

        public ValueTask AfterDeleteAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterDeleteHook<TRequest, TEntity>>(
                hook => hook.AfterDeleteAsync(context, cancellationToken));

        public ValueTask BeforeResponseAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IBeforeResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.BeforeResponseAsync(context, cancellationToken));

        public ValueTask AfterResponseAsync(
            CommandHookContext<TRequest, TEntity, TResponse> context,
            CancellationToken cancellationToken)
            => hookRunner.RunAsync<IAfterResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.AfterResponseAsync(context, cancellationToken));
    }
}
