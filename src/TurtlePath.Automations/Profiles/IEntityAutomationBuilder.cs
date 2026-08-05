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
        /// <summary>
        /// Adds a create command automation that returns a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToCreate<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        /// <summary>
        /// Adds a create command automation that does not return a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToCreate<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IRequest;

        /// <summary>
        /// Adds an update command automation that returns a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToUpdate<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        /// <summary>
        /// Adds an update command automation that does not return a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToUpdate<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest;

        /// <summary>
        /// Adds a delete command automation that returns a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToDelete<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class;

        /// <summary>
        /// Adds a delete command automation that does not return a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToDelete<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest;

        /// <summary>
        /// Adds a patch command automation that returns a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToPatch<TCommand, TResponse>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        /// <summary>
        /// Adds a patch command automation that does not return a response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToPatch<TCommand>(
            Action<IMutationAutomationBuilder<TCommand, TEntity, TKey>> configure = null)
            where TCommand : class, IBaseRequest<TKey>, IRequest;

        /// <summary>
        /// Adds a query automation that retrieves an entity by identifier.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToGetById<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        /// <summary>
        /// Adds a query automation that retrieves one entity by a configured key or value.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToGetOne<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<TResponse>
            where TResponse : class, IBaseResponse<TKey>;

        /// <summary>
        /// Adds a query automation that returns a collection.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToGetMany<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<IEnumerable<TResponse>>
            where TResponse : class, IBaseResponse<TKey>;

        /// <summary>
        /// Adds a query automation that returns a paged response.
        /// </summary>
        IEntityAutomationBuilder<TEntity, TKey> ToGetPaged<TQuery, TResponse>(
            Action<IQueryAutomationBuilder<TQuery, TEntity, TKey>> configure = null)
            where TQuery : class, IRequest<PagedResponse<TResponse>>
            where TResponse : class, IBaseResponse<TKey>;
    }
}
