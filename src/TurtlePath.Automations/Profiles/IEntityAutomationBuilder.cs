namespace TurtlePath.Automations.Profiles
{
    using Pelican.Mediator;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;

    /// <summary>
    /// Builds automations for a single entity.
    /// </summary>
    public interface IEntityAutomationBuilder<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        IEntityAutomationBuilder<TEntity, TKey> ToCreate<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        IEntityAutomationBuilder<TEntity, TKey> ToCreate<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IRequest;

        IEntityAutomationBuilder<TEntity, TKey> ToUpdate<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        IEntityAutomationBuilder<TEntity, TKey> ToUpdate<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest;

        IEntityAutomationBuilder<TEntity, TKey> ToDelete<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class;

        IEntityAutomationBuilder<TEntity, TKey> ToDelete<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest;

        IEntityAutomationBuilder<TEntity, TKey> ToPatch<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        IEntityAutomationBuilder<TEntity, TKey> ToPatch<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest;

        IEntityAutomationBuilder<TEntity, TKey> ToGetById<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        IEntityAutomationBuilder<TEntity, TKey> ToGetOne<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        IEntityAutomationBuilder<TEntity, TKey> ToGetMany<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<IEnumerable<TResponse>>
            where TResponse : class, IBaseResponse<TKey>;

        IEntityAutomationBuilder<TEntity, TKey> ToGetPaged<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<PagedResponse<TResponse>>
            where TResponse : class, IBaseResponse<TKey>;
    }
}
