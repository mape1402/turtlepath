namespace TurtlePath.Queries
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a query to retrieve multiple entities of a given type, with optional filtering and sorting.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class GenericGetManyQuery<TEntity, TResponse, TKey> : IRequest<IEnumerable<TResponse>>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
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
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    public abstract class GenericGetManyQueryHandler<TQuery, TEntity, TResponse, TKey> : IRequestHandler<TQuery, IEnumerable<TResponse>>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
        where TQuery : GenericGetManyQuery<TEntity, TResponse, TKey>
    {
        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        /// Gets the storage adapter for reading entities.
        /// </summary>
        protected IStorageReaderAdapter StorageReaderAdapter { get; }

        private readonly IQueryHookStageRunner<TQuery, IEnumerable<TResponse>> hookStageRunner;

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected QueryHookContext<TQuery, IEnumerable<TResponse>> Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GenericGetManyQueryHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            hookStageRunner = Services.GetRequiredService<IQueryHookStageRunner<TQuery, IEnumerable<TResponse>>>();
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

            await hookStageRunner.BeforeQueryAsync(Context, cancellationToken);

            var batch = await StorageReaderAdapter
                .For<TEntity>()
                .AsNoTracking()
                .Include(GetIncludeExpressions(request))
                .Where(GetFilterExpression(request))
                .FilterBy(request.Filters)
                .SortBy(GetSortingExpression(request))
                .SortBy(request.Sorts)
                .ToBatchAsync<TResponse>(cancellationToken);

            var response = batch.AsEnumerable();
            Context.Result = response;

            await hookStageRunner.AfterQueryAsync(Context, cancellationToken);

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

        /// <summary>
        /// Gets navigation expressions to include before mapping the entities.
        /// </summary>
        /// <param name="query">The query request.</param>
        /// <returns>The navigation expressions to include.</returns>
        protected virtual Expression<Func<TEntity, object>>[] GetIncludeExpressions(TQuery query) => [];
    }
}
