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

    /// <summary>
    /// Settings for paged queries.
    /// </summary>
    public class PagedSettings
    {
        /// <summary>
        /// Gets or sets the string-based filters to apply.
        /// </summary>
        public string Filters { get; set; }

        /// <summary>
        /// Gets or sets the string-based sorts to apply.
        /// </summary>
        public string Sorts { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the page number to retrieve.
        /// </summary>
        public int? PageNumber { get; set; }
    }

    /// <summary>
    /// Represents a query to retrieve a paged set of entities of a given type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class EntityGetPagedInfoQuery<TEntity, TResponse, TKey> : IRequest<PagedResponse<TResponse>>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="pagedSettings">The paged settings for the query.</param>
        protected EntityGetPagedInfoQuery(PagedSettings pagedSettings)
        {
            PagedSettings = pagedSettings;
        }

        /// <summary>
        /// Gets the paged settings for the query.
        /// </summary>
        public PagedSettings PagedSettings { get; }
    }

    /// <summary>
    /// Provides a base implementation for handling paged queries, including filtering, sorting, and pagination.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    public abstract class EntityGetPagedInfoQueryHandler<TQuery, TEntity, TResponse, TKey> : IRequestHandler<TQuery, PagedResponse<TResponse>>
        where TQuery : EntityGetPagedInfoQuery<TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected EntityGetPagedInfoQueryHandler(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageReaderAdapter = serviceProvider.GetRequiredService<IStorageReaderAdapter>();
        }

        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the storage adapter for reading entities.
        /// </summary>
        protected IStorageReaderAdapter StorageReaderAdapter { get; }

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected QueryHookContext<TQuery, PagedResponse<TResponse>> Context { get; private set; }

        /// <summary>
        /// Gets the default page size to use if not specified in the query.
        /// </summary>
        protected int DefaultPageSize => 50;

        /// <summary>
        /// Gets the default page number to use if not specified in the query.
        /// </summary>
        protected int DefaultPageNumber => 1;

        /// <summary>
        /// Gets the default sorts to use if not specified in the query. Must be implemented by derived classes.
        /// </summary>
        protected abstract string DefaultSorts { get; }

        /// <summary>
        /// Handles the paged query by retrieving entities matching the specified criteria and returning a paged response.
        /// </summary>
        /// <param name="request">The paged query request.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with a paged response as the result.</returns>
        public async Task<PagedResponse<TResponse>> Handle(TQuery request, CancellationToken cancellationToken = default)
        {
            Context = new QueryHookContext<TQuery, PagedResponse<TResponse>>(request);

            await ServiceProvider.RunHooksAsync<IBeforeQueryHook<TQuery, PagedResponse<TResponse>>>(
                hook => hook.BeforeQueryAsync(Context, cancellationToken));

            var batch = await StorageReaderAdapter
                .For<TEntity>()
                .AsNoTracking()
                .Where(GetFiltersExpression(request))
                .FilterBy(request.PagedSettings.Filters)
                .SortBy(GetSortingExpression(request))
                .SortBy(string.IsNullOrWhiteSpace(request.PagedSettings.Sorts) ? DefaultSorts : request.PagedSettings.Sorts)
                .Page(request.PagedSettings.PageNumber ?? DefaultPageNumber, request.PagedSettings.PageSize ?? DefaultPageSize)
                .ToBatchAsync<TResponse>(cancellationToken);

            var response = new PagedResponse<TResponse>
            {
                CurrentPage = batch.PageNumber,
                PageSize = batch.PageSize,
                PageCount = batch.PageCount,
                RowCount = batch.RowCount,
                Results = batch.Results
            };

            Context.Result = response;

            await ServiceProvider.RunHooksAsync<IAfterQueryHook<TQuery, PagedResponse<TResponse>>>(
                hook => hook.AfterQueryAsync(Context, cancellationToken));

            return response;
        }

        /// <summary>
        /// Gets the filter expression to apply to the query. Can be overridden in derived classes.
        /// </summary>
        /// <param name="request">The paged query request.</param>
        /// <returns>An expression for filtering entities, or null if not specified.</returns>
        protected virtual Expression<Func<TEntity, bool>> GetFiltersExpression(TQuery request) => null;

        /// <summary>
        /// Gets the sort expression to apply to the query. Can be overridden in derived classes.
        /// </summary>
        /// <param name="request">The paged query request.</param>
        /// <returns>An expression for sorting entities, or null if not specified.</returns>
        protected virtual Expression<Func<TEntity, object>> GetSortingExpression(TQuery request) => null;
    }
}
