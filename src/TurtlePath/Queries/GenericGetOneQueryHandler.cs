namespace TurtlePath.Queries
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a query to retrieve a single entity by a specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value used to retrieve the entity.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class GenericGetOneQuery<TValue, TEntity, TResponse, TKey> : IRequest<TResponse>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Gets or sets the value to retrieve.
        /// </summary>
        public TValue Value { get; set; }
    }

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve a single entity by a specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value used to retrieve the entity.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    public abstract class GenericGetOneQueryHandler<TQuery, TValue, TEntity, TResponse, TKey> : IRequestHandler<TQuery, TResponse>
        where TQuery : GenericGetOneQuery<TValue, TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GenericGetOneQueryHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
        }

        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        /// Gets the storage adapter for reading entities.
        /// </summary>
        protected IStorageReaderAdapter StorageReaderAdapter { get; }

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected QueryHookContext<TQuery, TResponse> Context { get; private set; }

        /// <summary>
        /// Handles the query to retrieve a single entity by a specified value.
        /// </summary>
        /// <param name="request">The query request containing the value.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the response as the result.</returns>
        public virtual async Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken = default)
        {
            Context = new QueryHookContext<TQuery, TResponse>(request);

            await Services.RunHooksAsync<IBeforeQueryHook<TQuery, TResponse>>(
                hook => hook.BeforeQueryAsync(Context, cancellationToken));

            var response = await StorageReaderAdapter
                .For<TEntity>()
                .AsNoTracking()
                .Where(GetFilterExpression(request))
                .FirstOrDefaultAsync<TResponse>(cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Value?.ToString() ?? "Unknown");

            Context.Result = response;

            await Services.RunHooksAsync<IAfterQueryHook<TQuery, TResponse>>(
                hook => hook.AfterQueryAsync(Context, cancellationToken));

            return response;
        }

        /// <summary>
        /// Gets the filter expression to apply to the query. Must be implemented by derived classes.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <returns>An expression for filtering entities.</returns>
        protected abstract Expression<Func<TEntity, bool>> GetFilterExpression(TQuery request);
    }
}
