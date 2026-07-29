namespace TurtlePath.Persistence
{
    using TurtlePath.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents criteria for retrieving a single entity, supporting both expression-based and string-based filters, and tracking options.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class GetOneCriteria<TEntity>
    {
        /// <summary>
        /// Gets or sets the filter expression to select the entity.
        /// </summary>
        public Expression<Func<TEntity, bool>> FiltersExpression { get; set; }

        /// <summary>
        /// Gets or sets the string-based filter to select the entity.
        /// </summary>
        public string Filters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use tracking when retrieving the entity.
        /// </summary>
        public bool UseTracking { get; set; } = true;
    }

    /// <summary>
    /// Represents criteria for retrieving multiple entities, including filtering, ordering, pagination, and tracking options.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class GetManyCriteria<TEntity>
    {
        /// <summary>
        /// Gets or sets the filter expression to select entities.
        /// </summary>
        public Expression<Func<TEntity, bool>> FiltersExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression to order the entities.
        /// </summary>
        public Expression<Func<TEntity, object>> SortingExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sorting is ascending.
        /// </summary>
        public bool AscendentSort { get; set; }

        /// <summary>
        /// Gets or sets the string-based filter to select entities.
        /// </summary>
        public string Filters { get; set; }

        /// <summary>
        /// Gets or sets the string-based sort to order entities.
        /// </summary>
        public string Sorts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use tracking when retrieving the entities.
        /// </summary>
        public bool UseTracking { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of items per page. Null means no paging.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the page number to retrieve. Null means no paging.
        /// </summary>
        public int? PageNumber { get; set; }

        /// <summary>
        /// Determines if the filter expression should be used.
        /// </summary>
        /// <returns>True if a filter expression is set; otherwise, false.</returns>
        public bool UseFiltersExpression() => FiltersExpression != null;

        /// <summary>
        /// Determines if the string-based filter should be used.
        /// </summary>
        /// <returns>True if a string-based filter is set; otherwise, false.</returns>
        public bool UseFilters() => !string.IsNullOrWhiteSpace(Filters);

        /// <summary>
        /// Determines if the sorting expression should be used.
        /// </summary>
        /// <returns>True if a sorting expression is set; otherwise, false.</returns>
        public bool UseSortingExpression() => SortingExpression != null;

        /// <summary>
        /// Determines if the string-based sort should be used.
        /// </summary>
        /// <returns>True if a string-based sort is set; otherwise, false.</returns>
        public bool UseSorts() => !string.IsNullOrWhiteSpace(Sorts);

        /// <summary>
        /// Determines if paging should be used based on the presence and values of PageSize and PageNumber.
        /// </summary>
        /// <returns>True if both PageSize and PageNumber are set and greater than zero; otherwise, false.</returns>
        public bool UsePaging() => PageSize.HasValue && PageNumber.HasValue && PageSize.Value > 0 && PageNumber.Value > 0;
    }

    /// <summary>
    /// Represents a paged result set for batch queries.
    /// </summary>
    /// <typeparam name="T">The type of the result items.</typeparam>
    public class BatchResult<T>
    {
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the total number of rows available.
        /// </summary>
        public long RowCount { get; set; }

        /// <summary>
        /// Gets or sets the number of pages in the document.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the results for the current page.
        /// </summary>
        public IEnumerable<T> Results { get; set; }

        /// <summary>
        /// Returns the results as an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <remarks>If the results are null, an empty enumerable is returned.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the results, or an empty enumerable if no results are available.</returns>
        public IEnumerable<T> AsEnumerable()
            => Results ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Defines an abstraction for reading entities from storage with support for filtering, ordering, and pagination.
    /// </summary>
    public interface IStorageReaderAdapter
    {
        /// <summary>
        /// Starts a fluent read operation for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A fluent read set for the entity type.</returns>
        IStorageReadSet<TEntity> For<TEntity>() where TEntity : BaseEntity;

        /// <summary>
        /// Asynchronously retrieves a single entity matching the specified criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TExpected">The type of the expected result.</typeparam>
        /// <param name="criteria">The criteria for selecting the entity.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the expected result as the result.</returns>
        Task<TExpected> GetOneAsync<TEntity, TExpected>(GetOneCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TExpected : class;

        /// <summary>
        /// Asynchronously retrieves multiple entities matching the specified criteria, with support for paging.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TExpected">The type of the expected result.</typeparam>
        /// <param name="criteria">The criteria for selecting the entities.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with a batch result containing the expected results as the result.</returns>
        Task<BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(GetManyCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TExpected : class;
    }

    /// <summary>
    /// Provides a fluent, provider-neutral read surface for common entity queries.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IStorageReadSet<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// Applies a typed filter.
        /// </summary>
        /// <param name="filter">The expression used to filter the entity set.</param>
        /// <returns>The current read set with the filter applied.</returns>
        IStorageReadSet<TEntity> Where(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Applies string-based filters supported by the active storage provider.
        /// </summary>
        /// <param name="filters">The string-based filters to apply.</param>
        /// <returns>The current read set with the filters applied.</returns>
        IStorageReadSet<TEntity> FilterBy(string filters);

        /// <summary>
        /// Applies an ascending typed sort.
        /// </summary>
        /// <param name="sort">The expression used to sort the entity set in ascending order.</param>
        /// <returns>The current read set with the sort applied.</returns>
        IStorageReadSet<TEntity> SortBy(Expression<Func<TEntity, object>> sort);

        /// <summary>
        /// Applies a descending typed sort.
        /// </summary>
        /// <param name="sort">The expression used to sort the entity set in descending order.</param>
        /// <returns>The current read set with the sort applied.</returns>
        IStorageReadSet<TEntity> SortByDescending(Expression<Func<TEntity, object>> sort);

        /// <summary>
        /// Applies string-based sorting supported by the active storage provider.
        /// </summary>
        /// <param name="sorts">The string-based sorts to apply.</param>
        /// <returns>The current read set with the sorts applied.</returns>
        IStorageReadSet<TEntity> SortBy(string sorts);

        /// <summary>
        /// Enables tracking for the read operation.
        /// </summary>
        /// <returns>The current read set configured to track entities.</returns>
        IStorageReadSet<TEntity> AsTracking();

        /// <summary>
        /// Disables tracking for the read operation.
        /// </summary>
        /// <returns>The current read set configured to avoid tracking entities.</returns>
        IStorageReadSet<TEntity> AsNoTracking();

        /// <summary>
        /// Applies paging.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>The current read set with paging applied.</returns>
        IStorageReadSet<TEntity> Page(int? pageNumber, int? pageSize);

        /// <summary>
        /// Retrieves the first matching item projected to the expected type.
        /// </summary>
        /// <typeparam name="TExpected">The expected result type.</typeparam>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the first matching result if found.</returns>
        Task<TExpected> FirstOrDefaultAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class;

        /// <summary>
        /// Retrieves a batch projected to the expected type.
        /// </summary>
        /// <typeparam name="TExpected">The expected result type.</typeparam>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with a batch result containing the matching results.</returns>
        Task<BatchResult<TExpected>> ToBatchAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class;
    }
}
