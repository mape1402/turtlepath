namespace TurtlePath.Automations.Profiles
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;

    internal sealed class EntityAutomationBuilder<TEntity, TKey> : IEntityAutomationBuilder<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        private readonly AutomationDescriptorRegistry registry;

        public EntityAutomationBuilder(AutomationDescriptorRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public IEntityAutomationBuilder<TEntity, TKey> ToCreate<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>
            => AddMutation<TCommand, TResponse>(AutomationOperationKind.Create, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToCreate<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IRequest
            => AddMutation<TCommand>(AutomationOperationKind.Create, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToUpdate<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>
            => AddMutation<TCommand, TResponse>(AutomationOperationKind.Update, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToUpdate<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest
            => AddMutation<TCommand>(AutomationOperationKind.Update, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToDelete<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class
            => AddMutation<TCommand, TResponse>(AutomationOperationKind.Delete, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToDelete<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest
            => AddMutation<TCommand>(AutomationOperationKind.Delete, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToPatch<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>
            => AddMutation<TCommand, TResponse>(AutomationOperationKind.Patch, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToPatch<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest
            => AddMutation<TCommand>(AutomationOperationKind.Patch, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToGetById<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>
            => AddQuery<TQuery, TResponse>(AutomationOperationKind.GetById, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToGetOne<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>
            => AddQuery<TQuery, TResponse>(AutomationOperationKind.GetOne, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToGetMany<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<IEnumerable<TResponse>>
            where TResponse : class, IBaseResponse<TKey>
            => AddQuery<TQuery, IEnumerable<TResponse>>(AutomationOperationKind.GetMany, configure);

        public IEntityAutomationBuilder<TEntity, TKey> ToGetPaged<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<PagedResponse<TResponse>>
            where TResponse : class, IBaseResponse<TKey>
            => AddQuery<TQuery, PagedResponse<TResponse>>(AutomationOperationKind.GetPaged, configure);

        private IEntityAutomationBuilder<TEntity, TKey> AddMutation<TRequest, TResponse>(
            AutomationOperationKind operationKind,
            Action<IMutationAutomationBuilder<TRequest, TEntity, TKey>> configure)
        {
            var builder = new MutationAutomationBuilder<TRequest, TEntity, TKey>();
            configure?.Invoke(builder);

            registry.Add(builder.CreateDescriptor(
                operationKind,
                typeof(TRequest),
                typeof(TEntity),
                typeof(TKey),
                AutomationReturnMode.Response,
                typeof(TResponse)));

            return this;
        }

        private IEntityAutomationBuilder<TEntity, TKey> AddMutation<TRequest>(
            AutomationOperationKind operationKind,
            Action<IMutationAutomationBuilder<TRequest, TEntity, TKey>> configure)
        {
            var builder = new MutationAutomationBuilder<TRequest, TEntity, TKey>();
            configure?.Invoke(builder);

            registry.Add(builder.CreateDescriptor(
                operationKind,
                typeof(TRequest),
                typeof(TEntity),
                typeof(TKey),
                AutomationReturnMode.None));

            return this;
        }

        private IEntityAutomationBuilder<TEntity, TKey> AddQuery<TQuery, TResponse>(
            AutomationOperationKind operationKind,
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure)
        {
            var builder = new QueryAutomationBuilder<TQuery, TEntity, TKey>();
            configure?.Invoke(builder);

            registry.Add(builder.CreateDescriptor(
                operationKind,
                typeof(TQuery),
                typeof(TEntity),
                typeof(TKey),
                typeof(TResponse)));

            return this;
        }
    }
}
