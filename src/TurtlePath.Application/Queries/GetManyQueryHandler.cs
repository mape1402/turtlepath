namespace TurtlePath.Application.Queries
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Application.Hooks;
    using TurtlePath.Application.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Contracts;
    using Pelican.Mediator;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a query to retrieve multiple entities of a given type, with optional filtering and sorting.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetManyQuery<TEntity, TResponse> : IRequest<IEnumerable<TResponse>>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the string-based filters to apply to the query.
        /// </summary>
        public string Filters { get; set; }

        /// <summary>
        /// Gets or sets the string-based sorts to apply to the query.
        /// </summary>
        public string Sorts { get; set; }
    }

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve multiple entities of a given type, with support for filtering and sorting.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetManyQueryHandler<TQuery, TEntity, TResponse> : IRequestHandler<TQuery, IEnumerable<TResponse>>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
        where TQuery : GetManyQuery<TEntity, TResponse>
    {
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
        protected QueryHookContext<TQuery, IEnumerable<TResponse>> Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetManyQueryHandler{TQuery, TEntity, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetManyQueryHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
        }

        /// <summary>
        /// Handles the query by retrieving all entities matching the specified filters and sorts.
        /// </summary>
        /// <param name="request">The query request containing filters and sorts.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with a collection of responses as the result.</returns>
        public virtual async Task<IEnumerable<TResponse>> Handle(TQuery request, CancellationToken cancellationToken = default)
        {
            Context = new QueryHookContext<TQuery, IEnumerable<TResponse>>(request);

            await Services.RunHooksAsync<IBeforeQueryHook<TQuery, IEnumerable<TResponse>>>(
                hook => hook.BeforeQueryAsync(Context, cancellationToken));

            var batch = await StorageReaderAdapter
                .For<TEntity>()
                .AsNoTracking()
                .Where(GetFilterExpression(request))
                .FilterBy(request.Filters)
                .SortBy(GetSortingExpression(request))
                .SortBy(request.Sorts)
                .ToBatchAsync<TResponse>(cancellationToken);

            var response = batch.AsEnumerable();
            Context.Result = response;

            await Services.RunHooksAsync<IAfterQueryHook<TQuery, IEnumerable<TResponse>>>(
                hook => hook.AfterQueryAsync(Context, cancellationToken));

            return response;
        }

        /// <summary>
        /// Gets the filter expression to apply to the query. Can be overridden in derived classes.
        /// </summary>
        /// <param name="query">The query request.</param>
        /// <returns>An expression for filtering entities, or null if not specified.</returns>
        protected virtual Expression<Func<TEntity, bool>> GetFilterExpression(TQuery query) => null;

        /// <summary>
        /// Gets the sort expression to apply to the query. Can be overridden in derived classes.
        /// </summary>
        /// <param name="query">The query request.</param>
        /// <returns>An expression for sorting entities, or null if not specified.</returns>
        protected virtual Expression<Func<TEntity, object>> GetSortingExpression(TQuery query) => null;
    }
}
